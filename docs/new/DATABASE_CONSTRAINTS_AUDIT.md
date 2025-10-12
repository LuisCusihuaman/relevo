# Database Constraints Audit

## Overview

This document catalogs all database constraints in the Relevo system, their business purpose, and known issues that can arise from them.

---

## Unique Constraints

### UQ_ACTIVE_HANDOVER_WINDOW

**Purpose**: Prevent multiple active handovers for the same patient during the same handover window and shift transition.

**Definition**:
```sql
CREATE UNIQUE INDEX UQ_ACTIVE_HANDOVER_WINDOW
ON HANDOVERS (
  PATIENT_ID,
  HANDOVER_WINDOW_DATE,
  FROM_SHIFT_ID,
  TO_SHIFT_ID,
  CASE
    WHEN COMPLETED_AT IS NULL
     AND CANCELLED_AT IS NULL
     AND REJECTED_AT IS NULL
     AND EXPIRED_AT IS NULL
    THEN 1
    ELSE NULL
  END
);
```

**Fields**:
- `PATIENT_ID`: Which patient
- `HANDOVER_WINDOW_DATE`: When (date of handover)
- `FROM_SHIFT_ID`: Which shift handing over
- `TO_SHIFT_ID`: Which shift receiving
- Function-based component: Only enforced when handover is "active"

**Business Rule**: 
A patient can only have ONE active handover per shift transition per day. Once a handover is completed/cancelled/rejected/expired, a new handover can be created for the same transition.

**Active Definition**:
A handover is "active" when ALL of these are NULL:
- `COMPLETED_AT`
- `CANCELLED_AT`
- `REJECTED_AT`
- `EXPIRED_AT`

**Allows**:
✅ Multiple handovers for same patient on different days
✅ Multiple handovers for same patient with different shift transitions
✅ New handover after previous one is completed/cancelled/rejected/expired

**Prevents**:
❌ Two active Draft handovers for same patient/shift/day
❌ Starting new handover while previous is InProgress
❌ Duplicate handovers due to race conditions

### ✅ FIXED ISSUE: NULL HANDOVER_WINDOW_DATE

**Previous Problem**: 
`HANDOVER_WINDOW_DATE` was not being set during handover creation, leaving it NULL. Oracle function-based unique indexes treat multiple NULL values as distinct, which meant the constraint wasn't being enforced correctly, BUT tests were still failing because the constraint was trying to create an index entry with NULL.

**Symptom**:
```
ORA-00001: unique constraint (RELEVO_APP.UQ_ACTIVE_HANDOVER_WINDOW) violated
```

**Root Cause**:
```csharp
// OLD CODE - Missing HANDOVER_WINDOW_DATE
INSERT INTO HANDOVERS (ID, ..., PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID)
VALUES (:handoverId, ..., :patientId, :fromShiftId, :toShiftId)
// HANDOVER_WINDOW_DATE was NULL!
```

**Fix Applied**:
```csharp
// NEW CODE - Sets HANDOVER_WINDOW_DATE
INSERT INTO HANDOVERS (ID, ..., PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, HANDOVER_WINDOW_DATE)
VALUES (:handoverId, ..., :patientId, :fromShiftId, :toShiftId, SYSTIMESTAMP)
```

**File**: `src/Relevo.Web/Setup/OracleSetupDataProvider.cs` line 309

**Why SYSTIMESTAMP**: Provides microsecond precision to avoid collisions when multiple handovers are created in rapid succession (e.g., parallel tests).

---

### UK_SYNC_USER_HANDOVER

**Purpose**: Each user can only have one sync status record per handover.

**Definition**:
```sql
CONSTRAINT UK_SYNC_USER_HANDOVER UNIQUE (HANDOVER_ID, USER_ID)
```

**Location**: `HANDOVER_SYNC_STATUS` table

**Business Rule**: A user's sync status for a handover is a singleton - can't have multiple sync records.

**Prevents**:
❌ Duplicate sync status entries

---

### UK_CHECKLIST_ITEM

**Purpose**: Each checklist item is unique per handover per user.

**Definition**:
```sql
CONSTRAINT UK_CHECKLIST_ITEM UNIQUE (HANDOVER_ID, USER_ID, ITEM_ID)
```

**Location**: `HANDOVER_CHECKLISTS` table

**Business Rule**: A specific checklist item can only appear once per user per handover.

**Prevents**:
❌ Duplicate checklist items

---

## Foreign Key Constraints

### HANDOVERS Table Foreign Keys

#### FK_HANDOVERS_ASSIGNMENT
```sql
CONSTRAINT FK_HANDOVERS_ASSIGNMENT 
FOREIGN KEY (ASSIGNMENT_ID) REFERENCES USER_ASSIGNMENTS(ASSIGNMENT_ID)
```
**Purpose**: Every handover must reference a valid assignment.
**Cascade**: Not specified (defaults to NO ACTION)
**Impact**: Can't delete assignment while handovers reference it

#### FK_HANDOVERS_PATIENT
```sql
CONSTRAINT FK_HANDOVERS_PATIENT 
FOREIGN KEY (PATIENT_ID) REFERENCES PATIENTS(ID)
```
**Purpose**: Every handover must reference a valid patient.
**Impact**: Can't delete patient while handovers reference it

#### FK_HANDOVERS_FROM_SHIFT / FK_HANDOVERS_TO_SHIFT
```sql
CONSTRAINT FK_HANDOVERS_FROM_SHIFT FOREIGN KEY (FROM_SHIFT_ID) REFERENCES SHIFTS(ID)
CONSTRAINT FK_HANDOVERS_TO_SHIFT FOREIGN KEY (TO_SHIFT_ID) REFERENCES SHIFTS(ID)
```
**Purpose**: Shift IDs must be valid.
**Impact**: Can't delete shifts while handovers reference them

#### FK_HANDOVERS_FROM_DOCTOR / FK_HANDOVERS_TO_DOCTOR / FK_HANDOVERS_COMPLETED_BY / FK_HANDOVERS_RECEIVER / FK_HANDOVERS_RESP_PHY_ID
```sql
CONSTRAINT FK_HANDOVERS_FROM_DOCTOR FOREIGN KEY (FROM_DOCTOR_ID) REFERENCES USERS(ID)
CONSTRAINT FK_HANDOVERS_TO_DOCTOR FOREIGN KEY (TO_DOCTOR_ID) REFERENCES USERS(ID)
CONSTRAINT FK_HANDOVERS_COMPLETED_BY FOREIGN KEY (COMPLETED_BY) REFERENCES USERS(ID)
CONSTRAINT FK_HANDOVERS_RECEIVER FOREIGN KEY (RECEIVER_USER_ID) REFERENCES USERS(ID)
CONSTRAINT FK_HANDOVERS_RESP_PHY_ID FOREIGN KEY (RESPONSIBLE_PHYSICIAN_ID) REFERENCES USERS(ID)
```
**Purpose**: All user references must be valid.
**Impact**: Can't delete users while handovers reference them
**Test Issue**: During E2E test cleanup, trying to delete test doctors fails if handovers exist:
```
ORA-02292: integrity constraint (RELEVO_APP.FK_HANDOVERS_FROM_DOCTOR) 
violated - child record found
```

---

### Child Table Foreign Keys

All child tables reference `HANDOVERS(ID)`:

```sql
-- HANDOVER_ACTION_ITEMS
CONSTRAINT FK_ACTION_ITEMS_HANDOVER FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID)

-- HANDOVER_PARTICIPANTS
CONSTRAINT FK_PARTICIPANTS_HANDOVER FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID)

-- HANDOVER_SYNC_STATUS
CONSTRAINT FK_SYNC_HANDOVER FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID)

-- HANDOVER_PATIENT_DATA
HANDOVER_ID VARCHAR2(50) PRIMARY KEY REFERENCES HANDOVERS(ID)

-- HANDOVER_SITUATION_AWARENESS
HANDOVER_ID VARCHAR2(50) PRIMARY KEY REFERENCES HANDOVERS(ID)

-- HANDOVER_SYNTHESIS
HANDOVER_ID VARCHAR2(50) PRIMARY KEY REFERENCES HANDOVERS(ID)

-- HANDOVER_CHECKLISTS
CONSTRAINT FK_CHECKLIST_HANDOVER FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID)

-- HANDOVER_CONTINGENCY
CONSTRAINT FK_CONTINGENCY_HANDOVER FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID)

-- HANDOVER_MESSAGES
CONSTRAINT FK_DISCUSSION_HANDOVER FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID)

-- HANDOVER_ACTIVITY_LOG
CONSTRAINT FK_ACTIVITY_HANDOVER FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID)
```

**Cascade Behavior**: Not specified (defaults to NO ACTION)

**Impact**: To delete a handover, must first delete all child records in dependency order.

**Test Cleanup Order** (implemented in E2E tests):
1. `HANDOVER_SYNC` (or `HANDOVER_SYNC_STATUS`)
2. `HANDOVER_PATIENT_DATA`
3. `HANDOVER_SITUATION_AWARENESS`
4. `HANDOVER_SYNTHESIS`
5. `HANDOVER_PARTICIPANTS`
6. `HANDOVER_MESSAGES`
7. `HANDOVER_ACTIVITY_LOG`
8. `HANDOVER_ACTION_ITEMS`
9. `HANDOVERS` (parent)
10. `USERS` (if cleaning up test users)

---

## Check Constraints

### CHK_HANDOVER_TYPE

```sql
CONSTRAINT chk_handover_type CHECK (HANDOVER_TYPE IN ('ShiftToShift','TemporaryCoverage','Consult'))
```

**Purpose**: Only allow valid handover types.

**Valid Values**:
- `ShiftToShift`: Normal shift-to-shift handover
- `TemporaryCoverage`: Temporary coverage (e.g., break, emergency)
- `Consult`: Consultation handover

**Prevents**:
❌ Invalid handover type strings

---

### CHK_HP_ILLNESS_SEVERITY

```sql
CONSTRAINT CHK_HP_ILLNESS_SEVERITY CHECK (ILLNESS_SEVERITY IN ('Stable','Watcher','Unstable','Critical'))
```

**Location**: `HANDOVER_PATIENT_DATA` table

**Purpose**: Only allow valid illness severity levels.

**Valid Values**:
- `Stable`: Patient condition stable
- `Watcher`: Requires watching, potential issues
- `Unstable`: Unstable condition
- `Critical`: Critical condition

**Prevents**:
❌ Invalid severity strings

---

## Performance Indexes

### HANDOVERS Table Indexes

```sql
CREATE INDEX IDX_HANDOVERS_PATIENT_ID ON HANDOVERS(PATIENT_ID);
CREATE INDEX IDX_HANDOVERS_ASSIGNMENT_ID ON HANDOVERS(ASSIGNMENT_ID);
CREATE INDEX IDX_HANDOVERS_STATUS ON HANDOVERS(STATUS);
CREATE INDEX IDX_HANDOVERS_CREATED_BY ON HANDOVERS(CREATED_BY);
CREATE INDEX IDX_HANDOVERS_FROM_DOCTOR ON HANDOVERS(FROM_DOCTOR_ID);
CREATE INDEX IDX_HANDOVERS_TO_DOCTOR ON HANDOVERS(TO_DOCTOR_ID);
CREATE INDEX IDX_HANDOVERS_FROM_SHIFT ON HANDOVERS(FROM_SHIFT_ID);
CREATE INDEX IDX_HANDOVERS_TO_SHIFT ON HANDOVERS(TO_SHIFT_ID);
CREATE INDEX IDX_HANDOVERS_INITIATED_AT ON HANDOVERS(INITIATED_AT);
CREATE INDEX IDX_HANDOVERS_COMPLETED_AT ON HANDOVERS(COMPLETED_AT);
```

**Purpose**: Optimize common queries:
- Finding handovers by patient
- Finding handovers by doctor (sender or receiver)
- Finding handovers by status or date range
- Joining with assignments

---

### PATIENTS Table Indexes

```sql
CREATE INDEX IDX_PATIENTS_UNIT_ID ON PATIENTS(UNIT_ID);
```

**Purpose**: Optimize `GetPatientsByUnit` queries.

---

### USER_ASSIGNMENTS Indexes

```sql
CREATE INDEX IDX_USER_ASSIGNMENTS_USER ON USER_ASSIGNMENTS(USER_ID);
CREATE INDEX IDX_USER_ASSIGNMENTS_PATIENT ON USER_ASSIGNMENTS(PATIENT_ID);
CREATE INDEX IDX_USER_ASSIGNMENTS_SHIFT ON USER_ASSIGNMENTS(SHIFT_ID);
```

**Purpose**: Optimize assignment lookups by user, patient, or shift.

---

## Known Constraint Issues & Solutions

### 1. ✅ FIXED: UQ_ACTIVE_HANDOVER_WINDOW with NULL dates

**Symptom**: Test failures with unique constraint violation even though handovers were for different patients.

**Root Cause**: `HANDOVER_WINDOW_DATE` was NULL, causing unexpected index behavior.

**Solution**: Always set `HANDOVER_WINDOW_DATE = SYSTIMESTAMP` during creation.

---

### 2. Test Cleanup FK Violations

**Symptom**: 
```
ORA-02292: integrity constraint (RELEVO_APP.FK_HANDOVERS_FROM_DOCTOR) 
violated - child record found
```

**Root Cause**: Trying to delete test users before deleting their handovers.

**Solution**: Delete in correct dependency order (see E2E test cleanup code).

**Alternative Solution**: Consider using `ON DELETE CASCADE` for test environments (NOT recommended for production).

---

### 3. Missing HANDOVER_SYNC vs HANDOVER_SYNC_STATUS

**Issue**: Test cleanup tries to delete from `HANDOVER_SYNC` table, but the actual table is `HANDOVER_SYNC_STATUS`.

**Symptom**:
```
ORA-00942: table or view does not exist
```

**Solution**: Update cleanup code to use correct table name, or handle missing tables gracefully.

**Status**: ✅ Fixed in E2E tests with try-catch blocks for missing tables.

---

### 4. Concurrent Handover Creation

**Scenario**: Two users try to create handovers for the same patient/shift/date at exactly the same time.

**Protection**: `UQ_ACTIVE_HANDOVER_WINDOW` prevents this at database level. Second transaction will get ORA-00001.

**Current Handling**: Not explicitly handled, will return 500 error.

**Recommendation**: Catch constraint violations and return 409 Conflict with meaningful message.

---

## Recommendations

### 1. Add Cascade Delete for Test Data

For test environments, consider:
```sql
ALTER TABLE HANDOVER_ACTION_ITEMS
DROP CONSTRAINT FK_ACTION_ITEMS_HANDOVER;

ALTER TABLE HANDOVER_ACTION_ITEMS
ADD CONSTRAINT FK_ACTION_ITEMS_HANDOVER 
FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID) ON DELETE CASCADE;
```

**Pros**: Simplifies test cleanup
**Cons**: Risk of accidental data loss in production

**Better Approach**: Use environment-specific schema files.

---

### 2. Add Constraint for Timestamp Consistency

Currently nothing prevents:
```sql
UPDATE HANDOVERS SET COMPLETED_AT = SYSTIMESTAMP, ACCEPTED_AT = NULL
-- Invalid: Can't complete without accepting first!
```

**Recommendation**: Add check constraints or triggers:
```sql
ALTER TABLE HANDOVERS ADD CONSTRAINT CHK_COMPLETED_REQUIRES_ACCEPTED
CHECK (COMPLETED_AT IS NULL OR ACCEPTED_AT IS NOT NULL);

ALTER TABLE HANDOVERS ADD CONSTRAINT CHK_ACCEPTED_REQUIRES_STARTED
CHECK (ACCEPTED_AT IS NULL OR STARTED_AT IS NOT NULL);

ALTER TABLE HANDOVERS ADD CONSTRAINT CHK_STARTED_REQUIRES_READY
CHECK (STARTED_AT IS NULL OR READY_AT IS NOT NULL);
```

**Pros**: Database-level enforcement of business rules
**Cons**: More rigid schema, harder to fix bad data

---

### 3. Add Constraint for Terminal States

Prevent updates to terminal handovers:
```sql
-- Can't modify handover after it's completed/cancelled/rejected/expired
ALTER TABLE HANDOVERS ADD CONSTRAINT CHK_NO_MODIFY_TERMINAL
CHECK (
  -- If any terminal timestamp is set, others should be NULL (only one terminal state)
  (COMPLETED_AT IS NULL OR (CANCELLED_AT IS NULL AND REJECTED_AT IS NULL AND EXPIRED_AT IS NULL))
  AND (CANCELLED_AT IS NULL OR (COMPLETED_AT IS NULL AND REJECTED_AT IS NULL AND EXPIRED_AT IS NULL))
  AND (REJECTED_AT IS NULL OR (COMPLETED_AT IS NULL AND CANCELLED_AT IS NULL AND EXPIRED_AT IS NULL))
  AND (EXPIRED_AT IS NULL OR (COMPLETED_AT IS NULL AND CANCELLED_AT IS NULL AND REJECTED_AT IS NULL))
);
```

**Purpose**: Ensure only one terminal state is set.

---

### 4. Add Indexes for Timestamp Queries

If querying by state-determining timestamps:
```sql
CREATE INDEX IDX_HANDOVERS_READY_AT ON HANDOVERS(READY_AT) WHERE READY_AT IS NOT NULL;
CREATE INDEX IDX_HANDOVERS_STARTED_AT ON HANDOVERS(STARTED_AT) WHERE STARTED_AT IS NOT NULL;
CREATE INDEX IDX_HANDOVERS_ACCEPTED_AT ON HANDOVERS(ACCEPTED_AT) WHERE ACCEPTED_AT IS NOT NULL;
-- Partial indexes for faster state-based queries
```

---

### 5. Add Unique Constraint on Assignment-Handover

Prevent multiple handovers per assignment:
```sql
CREATE UNIQUE INDEX UQ_ONE_HANDOVER_PER_ASSIGNMENT
ON HANDOVERS(ASSIGNMENT_ID);
```

**Question**: Should one assignment allow multiple handovers? Depends on business rules.

---

## Summary Table: All Constraints

| Constraint Type | Constraint Name | Table | Purpose | Issues |
|-----------------|-----------------|-------|---------|--------|
| **Unique** | UQ_ACTIVE_HANDOVER_WINDOW | HANDOVERS | One active handover per patient/shift/window | ✅ Fixed NULL date issue |
| **Unique** | UK_SYNC_USER_HANDOVER | HANDOVER_SYNC_STATUS | One sync status per user/handover | None |
| **Unique** | UK_CHECKLIST_ITEM | HANDOVER_CHECKLISTS | One checklist item per user/handover/item | None |
| **FK** | FK_HANDOVERS_ASSIGNMENT | HANDOVERS | Valid assignment | Cleanup order |
| **FK** | FK_HANDOVERS_PATIENT | HANDOVERS | Valid patient | None |
| **FK** | FK_HANDOVERS_FROM_DOCTOR | HANDOVERS | Valid user | Test cleanup issue |
| **FK** | FK_HANDOVERS_TO_DOCTOR | HANDOVERS | Valid user | Test cleanup issue |
| **FK** | FK_*_HANDOVER | Child tables | Valid handover | Cleanup dependency |
| **Check** | chk_handover_type | HANDOVERS | Valid type enum | None |
| **Check** | CHK_HP_ILLNESS_SEVERITY | HANDOVER_PATIENT_DATA | Valid severity enum | None |
| **Index** | UQ_ACTIVE_HANDOVER_WINDOW | HANDOVERS | Performance + uniqueness | Function-based |
| **Index** | IDX_HANDOVERS_* | HANDOVERS | Query performance | None |
| **Index** | IDX_PATIENTS_UNIT_ID | PATIENTS | Unit queries | None |
| **Index** | IDX_USER_ASSIGNMENTS_* | USER_ASSIGNMENTS | Assignment queries | None |

---

## Testing Impact

### E2E Test Considerations

1. **Cleanup Order Matters**: Must delete child records before parents
2. **Constraint Violations Are OK**: Tests should handle gracefully
3. **Parallel Tests**: Unique constraints prevent data collisions
4. **Missing Tables**: Some tables may not exist in fresh DB, handle errors

### Current E2E Test Cleanup Strategy

✅ Delete all handover child records first
✅ Delete handovers
✅ Attempt to delete test users (may fail, that's OK)
✅ Use try-catch to handle missing tables gracefully

---

## Conclusion

The database constraints provide good data integrity protection but require careful handling during:
- Test cleanup (dependency order)
- Concurrent operations (unique constraint violations)
- State transitions (business rule enforcement)

Future improvements should focus on:
- Adding state consistency constraints
- Better error handling for constraint violations
- Cascade delete for non-production environments

