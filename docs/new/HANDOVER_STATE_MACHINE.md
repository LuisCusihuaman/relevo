# Handover State Machine Documentation

## Overview

This document provides a complete specification of the handover lifecycle state machine, including all possible states, valid transitions, database implementation, and business rules.

---

## State Definitions

The handover state is determined by timestamp fields in the `HANDOVERS` table and calculated via the `VW_HANDOVERS_STATE` view. The state is **not** stored directly but derived using a priority-based CASE statement.

### States (in priority order)

| State | Description | Determining Field | Priority |
|-------|-------------|------------------|----------|
| **Completed** | Handover successfully finished | `COMPLETED_AT IS NOT NULL` | 1 (Highest) |
| **Cancelled** | Handover was cancelled | `CANCELLED_AT IS NOT NULL` | 2 |
| **Rejected** | Handover was rejected by receiver | `REJECTED_AT IS NOT NULL` | 3 |
| **Expired** | Handover window expired | `EXPIRED_AT IS NOT NULL` | 4 |
| **Accepted** | Receiver accepted the handover | `ACCEPTED_AT IS NOT NULL` | 5 |
| **InProgress** | Handover actively being communicated | `STARTED_AT IS NOT NULL` | 6 |
| **Ready** | Creator marked handover ready for receiver | `READY_AT IS NOT NULL` | 7 |
| **Draft** | Initial state, being prepared | All timestamps NULL | 8 (Default) |

**Important**: Priority matters! If multiple timestamp fields are set, the view returns the highest-priority state. For example, if both `COMPLETED_AT` and `CANCELLED_AT` are set, the state will be "Completed".

---

## Database Implementation

### Timestamp Fields (HANDOVERS table)

```sql
CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP      -- When handover record was created
INITIATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP    -- When handover was initiated (same as CREATED_AT)
READY_AT TIMESTAMP                             -- When marked ready for handover
STARTED_AT TIMESTAMP                           -- When handover communication started
ACKNOWLEDGED_AT TIMESTAMP                      -- Optional: when receiver acknowledged seeing it
ACCEPTED_AT TIMESTAMP                          -- When receiver accepted
COMPLETED_AT TIMESTAMP                         -- When handover finished
CANCELLED_AT TIMESTAMP                         -- When handover was cancelled
REJECTED_AT TIMESTAMP                          -- When handover was rejected
EXPIRED_AT TIMESTAMP                           -- When handover window expired
```

### State View (VW_HANDOVERS_STATE)

```sql
CREATE OR REPLACE VIEW VW_HANDOVERS_STATE AS
SELECT
  h.ID as HandoverId,
  CASE
    WHEN h.COMPLETED_AT IS NOT NULL THEN 'Completed'
    WHEN h.CANCELLED_AT IS NOT NULL THEN 'Cancelled'
    WHEN h.REJECTED_AT  IS NOT NULL THEN 'Rejected'
    WHEN h.EXPIRED_AT   IS NOT NULL THEN 'Expired'
    WHEN h.ACCEPTED_AT  IS NOT NULL THEN 'Accepted'
    WHEN h.STARTED_AT   IS NOT NULL THEN 'InProgress'
    WHEN h.READY_AT     IS NOT NULL THEN 'Ready'
    ELSE 'Draft'
  END AS StateName
FROM HANDOVERS h;
```

### Legacy STATUS Field

The `STATUS` VARCHAR2(20) field exists for backward compatibility but is **not the source of truth**. The actual state is determined by `VW_HANDOVERS_STATE`. Some methods update both `STATUS` and the timestamp fields to maintain consistency.

---

## Valid State Transitions

### Happy Path (Normal Handover Flow)

```
Draft → Ready → InProgress → Accepted → Completed
```

**1. Draft → Ready**
- **API**: `POST /handovers/{id}/ready`
- **Method**: `ReadyHandover(handoverId, userId)`
- **SQL**: `UPDATE HANDOVERS SET READY_AT = SYSTIMESTAMP WHERE ID = :handoverId AND READY_AT IS NULL`
- **Business Rule**: Can only transition if not already ready

**2. Ready → InProgress** 
- **API**: `POST /handovers/{id}/start`
- **Method**: `StartHandoverAsync(handoverId, userId)`
- **SQL**: `UPDATE HANDOVERS SET STARTED_AT = SYSTIMESTAMP, STATUS = 'InProgress' WHERE ID = :handoverId AND READY_AT IS NOT NULL AND STARTED_AT IS NULL`
- **Business Rule**: Must be Ready before starting (enforced)

**3. InProgress → Accepted**
- **API**: `POST /handovers/{id}/accept`
- **Method**: `AcceptHandover(handoverId, userId)`
- **SQL**: `UPDATE HANDOVERS SET ACCEPTED_AT = SYSTIMESTAMP WHERE ID = :handoverId AND STARTED_AT IS NOT NULL AND ACCEPTED_AT IS NULL`
- **Business Rule**: ✅ **Must be InProgress (STARTED_AT IS NOT NULL) before accepting** (enforced as of latest fix)

**4. Accepted → Completed**
- **API**: `POST /handovers/{id}/complete`
- **Method**: `CompleteHandover(handoverId, userId)`
- **SQL**: `UPDATE HANDOVERS SET COMPLETED_AT = SYSTIMESTAMP, STATUS = 'Completed', COMPLETED_BY = :userId WHERE ID = :handoverId AND ACCEPTED_AT IS NOT NULL AND COMPLETED_AT IS NULL`
- **Business Rule**: ✅ **Must be Accepted (ACCEPTED_AT IS NOT NULL) before completing** (enforced as of latest fix)

### Cancellation Paths

**From Draft/Ready/InProgress → Cancelled**
- **API**: `POST /handovers/{id}/cancel`
- **Method**: `CancelHandover(handoverId, userId)`
- **SQL**: `UPDATE HANDOVERS SET CANCELLED_AT = SYSTIMESTAMP, STATUS = 'Cancelled' WHERE ID = :handoverId AND CANCELLED_AT IS NULL AND ACCEPTED_AT IS NULL`
- **Business Rule**: Can cancel before acceptance only
- **Valid From States**: Draft, Ready, InProgress

### Rejection Path

**From InProgress → Rejected**
- **API**: `POST /handovers/{id}/reject`
- **Method**: `RejectHandover(handoverId, userId, reason)`
- **SQL**: `UPDATE HANDOVERS SET REJECTED_AT = SYSTIMESTAMP, REJECTION_REASON = :reason, STATUS = 'Rejected' WHERE ID = :handoverId AND REJECTED_AT IS NULL AND ACCEPTED_AT IS NULL`
- **Business Rule**: Can reject before acceptance only
- **Valid From States**: InProgress (typically)
- **Required Field**: `REJECTION_REASON`

### Expiration Path

**From Draft/Ready → Expired**
- **Trigger**: Automated process (time-based, not yet implemented)
- **Method**: Not yet implemented
- **Expected SQL**: `UPDATE HANDOVERS SET EXPIRED_AT = SYSTIMESTAMP WHERE HANDOVER_WINDOW_DATE < CURRENT_DATE AND COMPLETED_AT IS NULL...`
- **Business Rule**: Handover window passed without completion

---

## State Transition Matrix

| From State | Can Transition To | API Endpoint | Business Rule |
|------------|------------------|--------------|---------------|
| **Draft** | Ready | `POST /{id}/ready` | Always allowed |
| **Draft** | Cancelled | `POST /{id}/cancel` | Always allowed |
| **Draft** | Expired | (Automated) | Window passed |
| **Ready** | InProgress | `POST /{id}/start` | Always allowed |
| **Ready** | Cancelled | `POST /{id}/cancel` | Always allowed |
| **Ready** | Expired | (Automated) | Window passed |
| **InProgress** | Accepted | `POST /{id}/accept` | ✅ Enforced |
| **InProgress** | Cancelled | `POST /{id}/cancel` | Allowed |
| **InProgress** | Rejected | `POST /{id}/reject` | Allowed |
| **Accepted** | Completed | `POST /{id}/complete` | ✅ Enforced |
| **Completed** | *(terminal)* | - | No transitions |
| **Cancelled** | *(terminal)* | - | No transitions |
| **Rejected** | *(terminal)* | - | No transitions |
| **Expired** | *(terminal)* | - | No transitions |

---

## API Endpoints & State Changes

### POST /handovers
- **Creates handover in** `Draft` state
- **Sets**: `CREATED_AT`, `INITIATED_AT`, `HANDOVER_WINDOW_DATE` (now set to SYSTIMESTAMP as of latest fix)
- **Response**: Returns handover with `Status = "Draft"`

### POST /handovers/{id}/ready
- **Transitions**: Draft → Ready
- **Sets**: `READY_AT = SYSTIMESTAMP`
- **Returns**: `200 OK` with boolean success, or `404` if not found

### POST /handovers/{id}/start
- **Transitions**: Ready → InProgress
- **Sets**: `STARTED_AT = SYSTIMESTAMP`, `STATUS = 'InProgress'`
- **Precondition**: `READY_AT IS NOT NULL`
- **Returns**: `200 OK` with boolean success

### POST /handovers/{id}/accept
- **Transitions**: InProgress → Accepted
- **Sets**: `ACCEPTED_AT = SYSTIMESTAMP`
- **Precondition**: ✅ `STARTED_AT IS NOT NULL` (enforced)
- **Returns**: `200 OK` with success response, or `404` if precondition fails

### POST /handovers/{id}/complete
- **Transitions**: Accepted → Completed
- **Sets**: `COMPLETED_AT = SYSTIMESTAMP`, `STATUS = 'Completed'`, `COMPLETED_BY = userId`
- **Precondition**: ✅ `ACCEPTED_AT IS NOT NULL` (enforced)
- **Returns**: `200 OK` with success response, or `404` if precondition fails

### POST /handovers/{id}/cancel
- **Transitions**: Draft/Ready/InProgress → Cancelled
- **Sets**: `CANCELLED_AT = SYSTIMESTAMP`, `STATUS = 'Cancelled'`
- **Precondition**: `ACCEPTED_AT IS NULL` (can't cancel after acceptance)
- **Returns**: `200 OK` with boolean success

### POST /handovers/{id}/reject
- **Transitions**: InProgress → Rejected
- **Sets**: `REJECTED_AT = SYSTIMESTAMP`, `REJECTION_REASON`, `STATUS = 'Rejected'`
- **Precondition**: `ACCEPTED_AT IS NULL` (can't reject after acceptance)
- **Request Body**: `{ reason: string }`
- **Returns**: `200 OK` with boolean success

### GET /handovers/{id}
- **Returns**: Full handover object with all timestamp fields and computed `StateName`
- **State Calculation**: Performed by joining with `VW_HANDOVERS_STATE`

---

## Business Rule Violations & Error Handling

### Attempting Invalid Transitions

When an invalid transition is attempted (e.g., Accept before Start), the repository methods return `false` because the SQL `WHERE` clause doesn't match any rows.

**Example**: Attempting to Accept a Draft handover
```sql
UPDATE HANDOVERS 
SET ACCEPTED_AT = SYSTIMESTAMP 
WHERE ID = :handoverId AND STARTED_AT IS NOT NULL AND ACCEPTED_AT IS NULL
-- Returns 0 rows affected because STARTED_AT IS NULL
```

**API Response**:
- Returns `404 Not Found` (from endpoint logic)
- Does not throw exception
- Client should handle gracefully

### ⚠️ Missing Validation (Potential Issues)

Some transitions may not have explicit precondition checks. For example:
- Can you call `Ready` on an already-Ready handover? (Currently prevented by `READY_AT IS NULL` check)
- Can you call `Start` on an already-Started handover? (Currently prevented by `STARTED_AT IS NULL` check)

---

## Known Pitfalls & Edge Cases

### 1. ✅ FIXED: Missing HANDOVER_WINDOW_DATE

**Issue**: `HANDOVER_WINDOW_DATE` was NULL, causing unique constraint `UQ_ACTIVE_HANDOVER_WINDOW` violations in tests.

**Solution**: Now set to `SYSTIMESTAMP` during handover creation.

```csharp
// Fixed in OracleSetupDataProvider.CreateHandoverAsync
INSERT INTO HANDOVERS (..., HANDOVER_WINDOW_DATE) 
VALUES (..., SYSTIMESTAMP)
```

### 2. ✅ FIXED: Missing State Transition Validation

**Issue**: `AcceptHandover` and `CompleteHandover` didn't check preconditions, allowing invalid state transitions.

**Solution**: Added proper WHERE clauses in repository methods:

```csharp
// OracleSetupRepository.AcceptHandover - NOW ENFORCED
WHERE ID = :handoverId AND STARTED_AT IS NOT NULL AND ACCEPTED_AT IS NULL

// OracleSetupRepository.CompleteHandover - NOW ENFORCED  
WHERE ID = :handoverId AND ACCEPTED_AT IS NOT NULL AND COMPLETED_AT IS NULL
```

### 3. Concurrent State Changes

**Issue**: If two users try to transition the same handover simultaneously, race conditions could occur.

**Current Behavior**: Database-level atomicity via `WHERE` clauses prevents double-transitions (e.g., can't accept twice because `ACCEPTED_AT IS NULL` fails on second attempt).

**Recommendation**: Consider optimistic locking with version numbers for future enhancements.

### 4. Expired Handovers

**Issue**: No automated process to set `EXPIRED_AT` when handover window passes.

**Current State**: Manual or not implemented.

**Recommendation**: Implement background job to check `HANDOVER_WINDOW_DATE` and mark expired handovers.

### 5. Multiple Active Handovers

**Issue**: Can a patient have multiple active handovers at once?

**Current Protection**: `UQ_ACTIVE_HANDOVER_WINDOW` prevents multiple active handovers for the same patient/window/shift combination.

```sql
UNIQUE INDEX UQ_ACTIVE_HANDOVER_WINDOW ON HANDOVERS (
  PATIENT_ID,
  HANDOVER_WINDOW_DATE,
  FROM_SHIFT_ID,
  TO_SHIFT_ID,
  CASE WHEN COMPLETED_AT IS NULL AND CANCELLED_AT IS NULL 
       AND REJECTED_AT IS NULL AND EXPIRED_AT IS NULL 
  THEN 1 ELSE NULL END
)
```

This allows multiple handovers for the same patient IF:
- Different shifts
- Different dates
- Previous handovers are completed/cancelled/rejected/expired

### 6. Timezone Handling

**Issue**: All timestamps use `SYSTIMESTAMP` which is server timezone.

**Current Behavior**: No timezone conversion.

**Recommendation**: Store all timestamps in UTC and convert on client.

### 7. Orphaned Timestamps

**Issue**: What if `COMPLETED_AT` is set but `ACCEPTED_AT` is NULL?

**Current Protection**: Business logic prevents this via precondition checks, but database schema doesn't enforce it.

**Recommendation**: Consider database constraints or triggers to enforce state consistency.

---

## Testing Coverage

### ✅ Covered in E2E Tests

- **Happy Path**: Draft → Ready → InProgress → Accepted → Completed
- **Invalid Transition**: Cannot Accept before Start
- **Invalid Transition**: Cannot Complete before Accept

### ❌ Not Covered in Tests

- Cancellation from various states
- Rejection flow
- Expired handover handling
- Multiple handovers for same patient
- Concurrent state changes
- Edge cases (e.g., calling Ready twice)

---

## Recommendations

1. **Add E2E tests for cancellation and rejection flows**
2. **Implement expiration automation** (background job)
3. **Add database constraints** to enforce timestamp consistency
4. **Consider optimistic locking** for concurrent updates
5. **Standardize error responses** across all state transition endpoints
6. **Add audit logging** for all state transitions
7. **Implement timezone handling** (store UTC, display local)

---

## Summary

The handover state machine is **timestamp-driven** with states calculated from the `VW_HANDOVERS_STATE` view. As of the latest fixes:

✅ State transition validation is enforced (Accept requires Start, Complete requires Accept)
✅ Unique constraint violations are prevented (`HANDOVER_WINDOW_DATE` now set)
✅ Happy path is fully tested

⚠️ Edge cases like cancellation, rejection, and expiration need more testing
⚠️ Timezone handling and concurrent updates need consideration
⚠️ Database-level constraints could enforce state consistency more strongly

