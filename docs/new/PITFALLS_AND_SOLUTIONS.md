# Pitfalls & Solutions

## Overview

This document catalogs all known issues, edge cases, and their solutions in the Relevo handover system.

---

## Fixed Issues ✅

### 1. Unique Constraint Violation: NULL HANDOVER_WINDOW_DATE

**Symptom**:
```
ORA-00001: unique constraint (RELEVO_APP.UQ_ACTIVE_HANDOVER_WINDOW) violated
```

**Root Cause**:
`HANDOVER_WINDOW_DATE` was not being set during handover creation, leaving it NULL. This caused the unique constraint to fail unexpectedly, especially during parallel test execution.

**Code Location**: `src/Relevo.Web/Setup/OracleSetupDataProvider.cs`

**Before**:
```csharp
INSERT INTO HANDOVERS (
  ID, ASSIGNMENT_ID, PATIENT_ID, STATUS,
  SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID,
  CREATED_BY, CREATED_AT, INITIATED_AT, HANDOVER_TYPE
) VALUES (
  :handoverId, :assignmentId, :patientId, 'Draft',
  :shiftName, :fromShiftId, :toShiftId, :fromDoctorId, :toDoctorId,
  :initiatedBy, SYSTIMESTAMP, SYSTIMESTAMP, 'ShiftToShift'
)
```

**After**:
```csharp
INSERT INTO HANDOVERS (
  ID, ASSIGNMENT_ID, PATIENT_ID, STATUS,
  SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID,
  CREATED_BY, CREATED_AT, INITIATED_AT, HANDOVER_TYPE, HANDOVER_WINDOW_DATE
) VALUES (
  :handoverId, :assignmentId, :patientId, 'Draft',
  :shiftName, :fromShiftId, :toShiftId, :fromDoctorId, :toDoctorId,
  :initiatedBy, SYSTIMESTAMP, SYSTIMESTAMP, 'ShiftToShift', SYSTIMESTAMP
)
```

**Why SYSTIMESTAMP**: Provides microsecond precision to prevent collisions when multiple handovers are created rapidly (e.g., during parallel tests).

**Lesson Learned**: Always populate fields that are part of unique constraints, even if they seem optional.

---

### 2. Missing State Transition Validation

**Symptom**:
E2E tests showed handovers could be Accepted without being Started, or Completed without being Accepted, violating business rules.

**Root Cause**:
Repository methods (`AcceptHandover`, `CompleteHandover`) didn't check preconditions in their WHERE clauses.

**Code Location**: `src/Relevo.Infrastructure/Repositories/OracleSetupRepository.cs`

**Before (`AcceptHandover`)**:
```csharp
UPDATE HANDOVERS
SET ACCEPTED_AT = SYSTIMESTAMP, UPDATED_AT = SYSTIMESTAMP
WHERE ID = :handoverId AND ACCEPTED_AT IS NULL  // Missing: STARTED_AT check!
```

**After (`AcceptHandover`)**:
```csharp
UPDATE HANDOVERS
SET ACCEPTED_AT = SYSTIMESTAMP, UPDATED_AT = SYSTIMESTAMP
WHERE ID = :handoverId AND STARTED_AT IS NOT NULL AND ACCEPTED_AT IS NULL  // ✅ Now enforced
```

**Before (`CompleteHandover`)**:
```csharp
UPDATE HANDOVERS
SET COMPLETED_AT = SYSTIMESTAMP, STATUS = 'Completed', ...
WHERE ID = :handoverId AND COMPLETED_AT IS NULL  // Missing: ACCEPTED_AT check!
```

**After (`CompleteHandover`)**:
```csharp
UPDATE HANDOVERS
SET COMPLETED_AT = SYSTIMESTAMP, STATUS = 'Completed', ...
WHERE ID = :handoverId AND ACCEPTED_AT IS NOT NULL AND COMPLETED_AT IS NULL  // ✅ Now enforced
```

**Lesson Learned**: State machine transitions must be enforced at the database level, not just in application logic.

---

### 3. Patient Filtering Excluded Assigned Patients

**Symptom**:
`GetPatientsByUnit` returned empty results even though patients existed in the database.

**Root Cause**:
Query had a `NOT EXISTS` clause that excluded patients with assignments.

**Code Location**: `src/Relevo.Infrastructure/Repositories/OracleSetupRepository.cs`

**Before**:
```sql
SELECT COUNT(1) FROM PATIENTS p
WHERE p.UNIT_ID = :unitId
  AND NOT EXISTS (
    SELECT 1 FROM USER_ASSIGNMENTS ua 
    WHERE ua.PATIENT_ID = p.ID
  )  -- ❌ Excluded assigned patients!
```

**After**:
```sql
SELECT COUNT(1) FROM PATIENTS p
WHERE p.UNIT_ID = :unitId  -- ✅ Return ALL patients
```

**Rationale**: Patients should be visible in unit view regardless of assignment status. Handover status is tracked separately.

**Lesson Learned**: Be careful with exclusion filters - they can hide valid data.

---

### 4. Test Parallelization Conflicts

**Symptom**:
When running multiple E2E tests together, they would randomly fail with unique constraint violations or foreign key errors.

**Root Cause**:
Tests were running in parallel and creating handovers for the same patients simultaneously.

**Solutions Applied**:

**A. Sequential Test Execution**:
Created `SequentialCollectionDefinition.cs`:
```csharp
[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialCollectionDefinition { }
```

Applied to test class:
```csharp
[Collection("Sequential")]
public class HandoverLifecycleEndpoints { ... }
```

**B. Better Test Cleanup**:
Implemented comprehensive cleanup that deletes all handover-related data in dependency order:
```csharp
private void CleanupTestDoctorsFromDatabase(string doctorAId, string doctorBId)
{
  // Delete child tables first
  SafeDelete("HANDOVER_SYNC");
  SafeDelete("HANDOVER_PATIENT_DATA");
  SafeDelete("HANDOVER_SITUATION_AWARENESS");
  // ... etc
  SafeDelete("HANDOVERS");  // Parent last
  
  // Delete test users
  SafeDelete("USERS");
}
```

**C. Graceful Handling of Missing Tables**:
```csharp
void SafeDelete(string table)
{
  try {
    connection.Execute($"DELETE FROM {table} WHERE ...");
  }
  catch (OracleException) { /* Table doesn't exist, skip */ }
}
```

**Lesson Learned**: 
- E2E tests need proper isolation (sequential execution or better data isolation)
- Cleanup code must handle missing tables gracefully
- Delete in correct dependency order (children before parents)

---

### 5. Simplified View Logic for Completed State

**Symptom**:
Tests showed handovers in "InProgress" state even after completing them.

**Root Cause**:
`VW_HANDOVERS_STATE` had complex logic checking both `COMPLETED_AT` and `STATUS` field together.

**Code Location**: `src/Relevo.Infrastructure/Sql/03-views.sql`

**Before**:
```sql
CASE
  WHEN h.STATUS = 'Completed' AND h.COMPLETED_AT IS NOT NULL THEN 'Completed'
  -- Complex condition checking both fields
```

**After**:
```sql
CASE
  WHEN h.COMPLETED_AT IS NOT NULL THEN 'Completed'
  -- Simplified: timestamp is source of truth
```

**Rationale**: The timestamp fields are the source of truth for state, not the legacy `STATUS` VARCHAR field.

**Lesson Learned**: Keep state determination logic simple and based on a single source of truth.

---

## Remaining Issues & Edge Cases ⚠️

### 6. Expired Handover Handling

**Issue**: No automated process to set `EXPIRED_AT` when handover window passes.

**Current State**: 
- `EXPIRED_AT` field exists in database
- `Expired` state defined in view
- No background job to mark handovers as expired

**Impact**:
Handovers can remain in Draft/Ready state indefinitely, even after their window has passed.

**Proposed Solution**:
```csharp
// Background job (e.g., Hangfire, Quartz)
public async Task MarkExpiredHandoversAsync()
{
  await _dbConnection.ExecuteAsync(@"
    UPDATE HANDOVERS
    SET EXPIRED_AT = SYSTIMESTAMP,
        STATUS = 'Expired'
    WHERE HANDOVER_WINDOW_DATE < TRUNC(SYSDATE) - 1  -- Older than yesterday
      AND COMPLETED_AT IS NULL
      AND CANCELLED_AT IS NULL
      AND REJECTED_AT IS NULL
      AND EXPIRED_AT IS NULL
  ");
}
```

**Recommendation**: Implement daily scheduled job to mark expired handovers.

---

### 7. Concurrent State Changes

**Issue**: Two users might try to transition the same handover simultaneously.

**Example Scenario**:
- User A clicks "Accept" on handover
- User B clicks "Cancel" on same handover at the same time
- Which wins?

**Current Protection**:
Database-level atomicity via WHERE clauses prevents some issues:
```sql
-- Accept only if not already accepted
WHERE ID = :handoverId AND STARTED_AT IS NOT NULL AND ACCEPTED_AT IS NULL
```

**Remaining Issue**:
No optimistic locking or version checking. Last write wins.

**Proposed Solution**:
Add `VERSION` column and use optimistic locking:
```csharp
UPDATE HANDOVERS
SET ACCEPTED_AT = SYSTIMESTAMP,
    VERSION = VERSION + 1,
    UPDATED_AT = SYSTIMESTAMP
WHERE ID = :handoverId 
  AND VERSION = :expectedVersion  -- Fails if version changed
  AND STARTED_AT IS NOT NULL 
  AND ACCEPTED_AT IS NULL
```

**Recommendation**: Implement optimistic locking for state transitions.

---

### 8. Timezone Handling

**Issue**: All timestamps use `SYSTIMESTAMP` which is server timezone. No timezone conversion.

**Current Behavior**:
- Backend stores timestamps in server timezone (likely UTC if properly configured)
- Frontend receives timestamps as strings
- No timezone information included

**Impact**:
Users in different timezones see times in server timezone, not local time.

**Proposed Solution**:
1. **Backend**: Always store in UTC, include timezone in API responses:
```csharp
CreatedAt = handover.CreatedAt?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
```

2. **Frontend**: Parse and display in user's local timezone:
```typescript
new Date(handover.createdAt).toLocaleString()
```

**Recommendation**: Standardize on UTC storage and provide timezone context in API.

---

### 9. Orphaned Timestamps

**Issue**: What if database is manually edited and inconsistent timestamps are created?

**Example**:
```sql
-- Someone manually sets COMPLETED_AT without ACCEPTED_AT
UPDATE HANDOVERS SET COMPLETED_AT = SYSTIMESTAMP WHERE ID = 'h-123';
-- Now state is "Completed" but ACCEPTED_AT is NULL!
```

**Current Protection**: None at database level.

**Proposed Solution**:
Add check constraints to enforce consistency:
```sql
ALTER TABLE HANDOVERS ADD CONSTRAINT CHK_COMPLETED_REQUIRES_ACCEPTED
CHECK (COMPLETED_AT IS NULL OR ACCEPTED_AT IS NOT NULL);

ALTER TABLE HANDOVERS ADD CONSTRAINT CHK_ACCEPTED_REQUIRES_STARTED
CHECK (ACCEPTED_AT IS NULL OR STARTED_AT IS NOT NULL);

ALTER TABLE HANDOVERS ADD CONSTRAINT CHK_STARTED_REQUIRES_READY
CHECK (STARTED_AT IS NULL OR READY_AT IS NOT NULL);
```

**Pros**: Database enforces business rules
**Cons**: Can't fix bad data retroactively without disabling constraints

**Recommendation**: Add constraints for new environments, migrate existing data first.

---

### 10. Multiple Active Handovers for Same Patient

**Issue**: Can a patient have multiple active handovers simultaneously?

**Current Protection**: 
`UQ_ACTIVE_HANDOVER_WINDOW` prevents multiple active handovers for same:
- Patient ID
- Handover Window Date
- From Shift ID  
- To Shift ID

**What It Allows**:
✅ Day→Evening handover AND Evening→Night handover simultaneously (different shifts)
✅ Multiple handovers on different days for same transition
✅ New handover after previous is terminated

**Edge Case**:
If patient needs to be handed over to two different doctors during same shift transition:
```
Patient: John
From: Day Shift (Dr. Smith)
To: Evening Shift (Dr. Jones)  ✅ OK
To: Evening Shift (Dr. Brown)  ❌ Blocked by constraint
```

**Is This Desired?** Depends on business requirements.

**Proposed Solution** (if multiple receivers needed):
Change handover model to many-to-many:
- One handover can have multiple TO_DOCTOR_IDs
- Or use HANDOVER_PARTICIPANTS table for receivers

**Recommendation**: Clarify business requirements before changing.

---

### 11. Handover Cancellation After Acceptance

**Issue**: Can a handover be cancelled after it's been accepted?

**Current Behavior**:
```csharp
// CancelHandover checks ACCEPTED_AT IS NULL
WHERE ID = :handoverId AND CANCELLED_AT IS NULL AND ACCEPTED_AT IS NULL
```

✅ Can cancel from: Draft, Ready, InProgress (before acceptance)
❌ Cannot cancel from: Accepted, Completed

**Business Question**: Should accepted handovers be cancellable?

**Arguments For**:
- Mistakes happen, receiver might accept wrong handover
- Need escape hatch for errors

**Arguments Against**:
- Acceptance is a commitment
- Use Reject before accepting if unsure

**Recommendation**: Current behavior (can't cancel after accept) seems reasonable. Document this clearly.

---

### 12. Rejection After Start

**Issue**: Can a handover be rejected before it's started?

**Current Behavior**:
```csharp
// RejectHandover checks ACCEPTED_AT IS NULL
WHERE ID = :handoverId AND REJECTED_AT IS NULL AND ACCEPTED_AT IS NULL
```

Technically allows rejection from Draft/Ready/InProgress, but typically only makes sense for InProgress.

**Edge Case**: Receiver rejects a Draft handover they haven't even seen yet.

**Recommendation**: 
- Either enforce `STARTED_AT IS NOT NULL` for rejection
- Or add business logic to only show Reject button once handover is started

---

## Testing Gaps ❌

### Missing E2E Test Coverage

**Currently Tested**:
✅ Happy path: Draft → Ready → InProgress → Accepted → Completed
✅ Invalid transition: Cannot Accept before Start
✅ Invalid transition: Cannot Complete before Accept

**Not Tested**:
❌ Cancellation flow from various states
❌ Rejection flow with reason
❌ Expired handover handling (not implemented)
❌ Multiple handovers for same patient (edge case)
❌ Concurrent state changes by different users
❌ Edge cases (calling Ready twice, etc.)
❌ Error responses for constraint violations
❌ Authorization checks (all endpoints currently anonymous)

**Recommendation**: Add test coverage for:
1. Cancellation from Draft, Ready, InProgress
2. Cannot cancel from Accepted
3. Rejection with reason text
4. Constraint violation handling
5. Concurrent transitions

---

### Missing Unit Tests

**Currently Tested**:
31 unit tests exist but coverage unknown.

**Gaps**:
- State view logic (`VW_HANDOVERS_STATE`)
- Patient status derivation
- Physician status calculation
- Constraint enforcement
- Timestamp consistency

**Recommendation**: Add unit tests for:
1. State determination logic (mocked timestamps)
2. Physician status calculation per relationship
3. Patient handover status derivation
4. Constraint violation scenarios

---

## Performance Considerations

### 13. N+1 Query Problem in GetPatientsByUnit

**Issue**: Query might not be optimized for large patient lists.

**Current Approach**: Left joins with window functions:
```sql
LEFT JOIN (
  SELECT ... ROW_NUMBER() OVER (PARTITION BY PATIENT_ID ORDER BY CREATED_AT DESC) AS rn
  FROM HANDOVERS
) h ON p.ID = h.PATIENT_ID AND h.rn = 1
```

**Potential Issues**:
- Window function over all handovers (not just for patients in page)
- Left joins could be slow for large datasets

**Recommendation**:
1. Add index on `(PATIENT_ID, CREATED_AT DESC)` for handovers
2. Consider pagination at database level before joins
3. Profile query with EXPLAIN PLAN for large datasets

---

### 14. Missing Indexes on Timestamp Fields

**Current Indexes**: Only `INITIATED_AT` and `COMPLETED_AT` have indexes.

**Missing**:
- `READY_AT`
- `STARTED_AT`
- `ACCEPTED_AT`
- `CANCELLED_AT`
- `REJECTED_AT`
- `EXPIRED_AT`

**Impact**: Queries filtering by state (via timestamps) might be slow.

**Recommendation**:
Add partial indexes for non-NULL timestamps:
```sql
CREATE INDEX IDX_HANDOVERS_READY_AT ON HANDOVERS(READY_AT) WHERE READY_AT IS NOT NULL;
-- etc.
```

---

## Security Concerns

### 15. Authorization Not Enforced

**Issue**: Most endpoints use `AllowAnonymous()` with comment "Let our custom middleware handle authentication".

**Current Behavior**:
- Middleware may or may not be enforcing auth
- Fallback demo user allows access: `user_demo12345678901234567890123456`

**Risks**:
- Unauthorized access to handover data
- Ability to transition handovers without proper authorization
- Data leakage

**Recommendation**:
1. Remove `AllowAnonymous()` from production endpoints
2. Use `[Authorize]` attribute with proper role checks
3. Only use demo user fallback in development environment
4. Audit all endpoints for authorization requirements

---

### 16. No Role-Based Access Control

**Issue**: All authenticated users can perform all actions.

**Example**: Any doctor can:
- Accept handovers not assigned to them
- Complete handovers they're not part of
- Cancel other doctors' handovers

**Current Protection**: None.

**Recommendation**:
Implement checks in endpoints:
```csharp
// Only assigned doctor can accept
if (handover.ToDoctorId != currentUserId) {
  return Forbid();
}

// Only creating doctor can cancel before acceptance
if (handover.FromDoctorId != currentUserId && handover.AcceptedAt == null) {
  return Forbid();
}
```

---

## Summary: Priority Matrix

| Issue | Impact | Difficulty | Priority | Status |
|-------|---------|-----------|----------|--------|
| NULL HANDOVER_WINDOW_DATE | High | Low | P0 | ✅ Fixed |
| Missing state validation | High | Low | P0 | ✅ Fixed |
| Patient filtering bug | High | Low | P0 | ✅ Fixed |
| Test parallelization | Medium | Medium | P1 | ✅ Fixed |
| Authorization not enforced | **High** | Medium | **P0** | ⚠️ Open |
| Expired handover automation | Low | Medium | P2 | ❌ Open |
| Concurrent state changes | Medium | High | P2 | ⚠️ Partial |
| Timezone handling | Low | Low | P3 | ❌ Open |
| Orphaned timestamps | Low | Medium | P3 | ❌ Open |
| Role-based access control | High | High | P1 | ❌ Open |
| Real-time updates | Medium | High | P2 | ❌ Open |
| Missing test coverage | Medium | Medium | P2 | ❌ Open |
| Performance optimization | Low | Medium | P3 | ❌ Open |
| Error handling | Medium | Low | P2 | ⚠️ Partial |

---

## Action Plan

### Immediate (P0)
1. ✅ Fix state transition validation
2. ✅ Fix HANDOVER_WINDOW_DATE
3. ⚠️ **Implement proper authorization**

### Short-term (P1)
4. ⚠️ Add role-based access control
5. ⚠️ Standardize error handling
6. ❌ Add cancellation/rejection E2E tests

### Medium-term (P2)
7. ❌ Implement expired handover automation
8. ❌ Add real-time updates (SignalR)
9. ⚠️ Implement optimistic locking
10. ❌ Expand test coverage

### Long-term (P3)
11. ❌ Implement timezone handling
12. ❌ Add database constraints for timestamp consistency
13. ❌ Performance optimization (indexes, query tuning)
14. ❌ Add comprehensive monitoring and logging

---

## Lessons Learned

1. **Always populate constraint fields** - Even "optional" fields in unique constraints must be populated
2. **Enforce business rules at DB level** - Application logic can be bypassed
3. **Test state machine thoroughly** - All transitions and edge cases
4. **Clean up tests properly** - Delete in dependency order, handle missing tables
5. **Authorization is not optional** - Must be enforced from day one
6. **Timestamps are better than status fields** - Single source of truth
7. **Document edge cases** - Future developers need to know the pitfalls
8. **Concurrent access needs consideration** - Optimistic locking or similar
9. **Integration tests catch more bugs** - E2E tests found issues unit tests missed
10. **Database constraints are your friend** - But must be designed carefully

---

## Conclusion

The handover system has made significant progress with core functionality working correctly. The main areas requiring attention are:

**✅ Working Well**:
- State machine logic (after fixes)
- Data persistence
- API endpoints
- Patient retrieval

**⚠️ Needs Improvement**:
- Authorization & security
- Error handling
- Test coverage
- Edge case handling

**❌ Not Implemented**:
- Real-time updates
- Expired handover automation
- Role-based access control
- Comprehensive monitoring

The fixes implemented during E2E test development have significantly improved the system's reliability. Continuing to address the remaining issues will result in a production-ready handover system.

