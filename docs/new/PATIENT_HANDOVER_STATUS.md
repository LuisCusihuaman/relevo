# Patient Handover Status Documentation

## Overview

This document explains how patient status relates to handovers, how patient assignment tracking works, and how the `GetPatientsByUnit` endpoint calculates and returns patient handover status.

---

## Patient Assignment States

Patients don't have an explicit "status" field. Instead, their handover status is **derived** from the most recent handover record associated with them.

### Derived Patient States

| Patient State | Meaning | How Determined |
|---------------|---------|----------------|
| **NotStarted** | No handover initiated | No handover records for patient, or all handovers are terminal (Completed/Cancelled/Rejected/Expired) |
| **Assigned** | Patient assigned to doctor, no active handover | Has assignment in `USER_ASSIGNMENTS`, but no active handover |
| **InHandover** | Currently being handed over | Has active handover (Draft/Ready/InProgress/Accepted states) |
| **Completed** | Handover finished | Most recent handover is in Completed state |

**Note**: These states are **logical concepts** used by the frontend. The backend returns handover state names directly.

---

## Database Relationships

### USER_ASSIGNMENTS Table

Tracks which doctors are assigned to which patients during which shifts.

```sql
CREATE TABLE USER_ASSIGNMENTS (
    ASSIGNMENT_ID VARCHAR2(255) PRIMARY KEY,
    USER_ID VARCHAR2(255) NOT NULL,        -- Doctor's Clerk User ID
    SHIFT_ID VARCHAR2(50) NOT NULL,        -- Which shift (e.g., shift-day, shift-night)
    PATIENT_ID VARCHAR2(50) NOT NULL,      -- Patient being assigned
    ASSIGNED_AT TIMESTAMP DEFAULT SYSTIMESTAMP
);
```

**Purpose**: 
- Tracks current doctor-patient assignments
- Used to determine "who's responsible for this patient right now"
- Referenced by `HANDOVERS.ASSIGNMENT_ID`

### HANDOVERS Table Relationship

```sql
CREATE TABLE HANDOVERS (
    ID VARCHAR2(50) PRIMARY KEY,
    ASSIGNMENT_ID VARCHAR2(255) NOT NULL,  -- FK to USER_ASSIGNMENTS
    PATIENT_ID VARCHAR2(50) NOT NULL,      -- FK to PATIENTS
    FROM_DOCTOR_ID VARCHAR2(255),          -- Doctor handing over
    TO_DOCTOR_ID VARCHAR2(255),            -- Doctor receiving
    ...timestamp fields...
);
```

**Flow**:
1. Doctor A is assigned to Patient X → entry in `USER_ASSIGNMENTS`
2. Doctor A initiates handover to Doctor B → creates `HANDOVER` record
3. During handover, Patient X's "handover status" reflects the handover state
4. When handover completes, Doctor B becomes responsible → `USER_ASSIGNMENTS` updated

---

## GetPatientsByUnit Logic

### Endpoint: `GET /units/{unitId}/patients`

Returns all patients in a unit with their current handover status.

### SQL Query (Simplified)

```sql
SELECT 
  p.ID AS Id, 
  p.NAME AS Name, 
  'NotStarted' AS HandoverStatus,  -- Default if no handover
  CAST(NULL AS VARCHAR(255)) AS HandoverId,
  FLOOR((SYSDATE - p.DATE_OF_BIRTH)/365.25) AS Age,
  p.ROOM_NUMBER AS Room,
  p.DIAGNOSIS AS Diagnosis,
  CASE
    WHEN h.STATUS = 'Completed' AND h.COMPLETED_AT IS NOT NULL THEN 'Completed'
    WHEN h.CANCELLED_AT IS NOT NULL THEN 'Cancelled'
    WHEN h.REJECTED_AT IS NOT NULL THEN 'Rejected'
    WHEN h.EXPIRED_AT IS NOT NULL THEN 'Expired'
    WHEN h.ACCEPTED_AT IS NOT NULL THEN 'Accepted'
    WHEN h.STARTED_AT IS NOT NULL THEN 'InProgress'
    WHEN h.READY_AT IS NOT NULL THEN 'Ready'
    ELSE 'Draft'
  END AS Status,
  hpd.ILLNESS_SEVERITY AS Severity
FROM (
  -- All patients in unit with ROW_NUMBER for pagination
  SELECT ID, NAME, DATE_OF_BIRTH, ROOM_NUMBER, DIAGNOSIS, 
         ROW_NUMBER() OVER (ORDER BY ID) AS RN
  FROM PATIENTS
  WHERE UNIT_ID = :unitId
) p
LEFT JOIN (
  -- Most recent handover per patient
  SELECT ID, PATIENT_ID, STATUS, COMPLETED_AT, CANCELLED_AT, REJECTED_AT, 
         EXPIRED_AT, ACCEPTED_AT, STARTED_AT, READY_AT,
         ROW_NUMBER() OVER (PARTITION BY PATIENT_ID ORDER BY CREATED_AT DESC) AS rn
  FROM HANDOVERS
) h ON p.ID = h.PATIENT_ID AND h.rn = 1
LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
WHERE p.RN BETWEEN :startRow AND :endRow
```

### Key Logic

1. **All Patients Returned**: ✅ As of latest fix, the query returns ALL patients in a unit, regardless of assignment status
2. **Most Recent Handover**: Uses `ROW_NUMBER() ... ORDER BY CREATED_AT DESC` to get latest handover per patient
3. **State Calculation**: Mimics `VW_HANDOVERS_STATE` logic directly in query for performance
4. **Severity**: Joins with `HANDOVER_PATIENT_DATA` to get illness severity from active handover

---

## Issues Fixed

### ✅ FIXED: Assigned Patients Were Excluded

**Previous Issue**: The query had this clause:

```sql
WHERE UNIT_ID = :unitId
  AND NOT EXISTS (
    SELECT 1 FROM USER_ASSIGNMENTS ua 
    WHERE ua.PATIENT_ID = p.ID
  )
```

This **excluded** patients who had assignments, which was incorrect behavior. Assigned patients should still appear in the unit list.

**Fix**: Removed the `NOT EXISTS` clause. Now all patients in the unit are returned, and their handover status is determined by the most recent handover record.

```sql
WHERE UNIT_ID = :unitId  -- Only filter by unit
```

---

## Patient Visibility in Different Views

### Daily Setup View

**Endpoint**: `GET /units/{unitId}/patients`

**Returns**: All patients in unit with handover status

**Frontend Behavior**:
- Shows all patients regardless of handover status
- Can initiate new handover for any patient
- Visual indicators show patient's current handover state

### My Patients View

**Endpoint**: `GET /my-patients` (via `ISetupService.GetMyPatientsAsync`)

**Returns**: Patients assigned to current user

**Query Logic**:
```sql
SELECT * FROM PATIENTS p
INNER JOIN USER_ASSIGNMENTS ua ON p.ID = ua.PATIENT_ID
WHERE ua.USER_ID = :userId
```

**Frontend Behavior**:
- Shows only patients currently assigned to logged-in doctor
- Different from "all patients in my unit"

### Pending Handovers View

**Endpoint**: `GET /pending-handovers` (via `ISetupService.GetPendingHandoversForUserAsync`)

**Returns**: Handovers where user is sender OR receiver and handover is not terminal

**Query Logic**:
```sql
SELECT * FROM HANDOVERS h
WHERE (h.FROM_DOCTOR_ID = :userId OR h.TO_DOCTOR_ID = :userId)
  AND h.COMPLETED_AT IS NULL
  AND h.CANCELLED_AT IS NULL
  AND h.REJECTED_AT IS NULL
  AND h.EXPIRED_AT IS NULL
```

**Frontend Behavior**:
- Shows active handovers user is involved in
- Action buttons based on user's role (sender vs receiver) and handover state

---

## Handover Status Affects Patient Actions

### Frontend Action Availability Matrix

| Patient Handover Status | Can View | Can Edit | Can Initiate New Handover | Can Accept/Complete |
|------------------------|----------|----------|---------------------------|---------------------|
| **No Active Handover** | ✅ | ✅ | ✅ | ❌ |
| **Draft** (I'm creator) | ✅ | ✅ | ❌ | ❌ |
| **Ready** (I'm creator) | ✅ | ✅ | ❌ | ❌ |
| **Ready** (I'm receiver) | ✅ | ❌ | ❌ | ✅ Can Start |
| **InProgress** (I'm creator) | ✅ | ✅ | ❌ | ❌ |
| **InProgress** (I'm receiver) | ✅ | ❌ | ❌ | ✅ Can Accept |
| **Accepted** (I'm receiver) | ✅ | ✅ | ❌ | ✅ Can Complete |
| **Completed** | ✅ | ❌ | ✅ (new handover) | ❌ |
| **Cancelled/Rejected** | ✅ | ❌ | ✅ (new handover) | ❌ |

**Note**: These rules are enforced in **frontend only**. Backend always requires proper authorization checks.

---

## Physician Status Calculation

When fetching patient data for a handover (`GET /handovers/{id}/patient`), the backend calculates physician status based on:
1. Handover state
2. User's relationship to handover (creator vs receiver)

### Calculation Logic (from `GetById.cs`)

```csharp
private static string CalculatePhysicianStatus(HandoverRecord handover, string relationship)
{
    var state = handover.StateName?.ToLower();
    
    return state switch
    {
        "completed" => "completed",
        "cancelled" => "cancelled",
        "rejected" => "rejected",
        "expired" => "expired",
        "accepted" => relationship == "creator" ? "handed-off" : "accepted",
        "draft" => relationship == "creator" ? "handing-off" : "pending",
        "ready" => relationship == "creator" ? "handing-off" : "ready-to-receive",
        "inprogress" => relationship == "creator" ? "handing-off" : "receiving",
        _ => "unknown"
    };
}
```

### Physician Status Values

| Handover State | Creator Status | Receiver Status |
|----------------|----------------|-----------------|
| Draft | handing-off | pending |
| Ready | handing-off | ready-to-receive |
| InProgress | handing-off | receiving |
| Accepted | handed-off | accepted |
| Completed | completed | completed |
| Cancelled | cancelled | cancelled |
| Rejected | rejected | rejected |
| Expired | expired | expired |

---

## Data Flow Example

### Scenario: Dr. Smith hands over Patient John to Dr. Jones

**Initial State**:
```
USER_ASSIGNMENTS:
  - User: Dr. Smith, Patient: John, Shift: Day

HANDOVERS:
  - (none for John)
```

**Step 1: Dr. Smith initiates handover**
```
POST /handovers
→ Creates handover in Draft state

HANDOVERS:
  - ID: h-123, Patient: John, From: Dr. Smith, To: Dr. Jones, 
    CREATED_AT: 2025-10-12 14:00, State: Draft
```

**GET /units/icu/patients** for John returns:
```json
{
  "id": "john-123",
  "name": "John Doe",
  "status": "Draft",     // ← From most recent handover
  "handoverId": "h-123"
}
```

**Step 2: Dr. Smith marks ready**
```
POST /handovers/h-123/ready
→ Sets READY_AT

GET /units/icu/patients for John:
{
  "status": "Ready"  // ← Updated
}
```

**Step 3-5: Start → Accept → Complete**
Each transition updates the handover state, which is reflected in the patient status when queried.

**Final State**:
```
HANDOVERS:
  - ID: h-123, State: Completed, COMPLETED_AT: 2025-10-12 15:00

USER_ASSIGNMENTS:
  - User: Dr. Jones, Patient: John, Shift: Evening (new assignment)
```

---

## Known Issues & Edge Cases

### 1. ✅ FIXED: Assigned Patients Not Shown

**Previously**: Patients with active assignments were filtered out from unit views.

**Now**: All patients shown, assignment status tracked separately via handover state.

### 2. Stale Patient Status

**Issue**: If frontend caches patient list and a handover state changes, the cached status becomes stale.

**Current Behavior**: Frontend must refetch or use real-time updates (WebSocket).

**Recommendation**: Implement real-time sync or periodic polling for patient lists.

### 3. Multiple Doctors Viewing Same Patient

**Issue**: If two doctors open the same patient handover simultaneously, they might see different states if one performs an action.

**Current Behavior**: Last write wins, no conflict resolution.

**Recommendation**: Implement optimistic locking or real-time sync.

### 4. Patient with Multiple Handovers

**Issue**: The query only shows most recent handover. What if patient has multiple active handovers (different shifts)?

**Current Protection**: `UQ_ACTIVE_HANDOVER_WINDOW` prevents multiple active handovers for same patient/shift/window.

**Edge Case**: Patient could have handover for Day→Evening AND Evening→Night simultaneously if they're for different shift transitions.

---

## Testing Recommendations

### Unit Tests Needed
- `GetPatientsByUnit` returns all patients (✅ implicitly tested in E2E)
- Handover status correctly derived from most recent handover
- Severity joined correctly from `HANDOVER_PATIENT_DATA`
- Pagination works correctly

### Integration Tests Needed
- Patient status updates when handover state changes
- Multiple handovers for same patient handled correctly
- Edge case: No handovers vs completed handovers vs active handovers

---

## Summary

- Patient handover status is **derived** from the most recent `HANDOVER` record
- `GetPatientsByUnit` returns **all patients** (fixed) with their derived status
- Physician status calculation differs based on creator vs receiver role
- Frontend uses this status to enable/disable actions
- Real-time sync and concurrent access need improvement

