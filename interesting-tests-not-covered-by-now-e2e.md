# Interesting E2E Tests Not Yet Covered in `./api`

This document lists interesting functional tests from `relevo-api/tests/Relevo.FunctionalTests` that are **not yet implemented** or **partially covered** in the new `./api` project. These tests represent valuable scenarios that should be replicated for comprehensive E2E coverage.

---

## 🔴 Priority 1: Handover Lifecycle Tests

### Full Handover Lifecycle Flow
**Source:** `HandoverLifecycleEndpoints.cs`

These tests verify the complete handover workflow between two doctors.

| Test Name | Description | Status |
|-----------|-------------|--------|
| `HandoverLifecycle_DoctorA_To_DoctorB_Should_Succeed` | Complete lifecycle: Create → Ready → Start → Accept → Complete | ❌ Not covered |
| `HandoverLifecycle_Cannot_Accept_Before_Start` | Validates state machine: cannot accept a handover that hasn't been started | ❌ Not covered |
| `HandoverLifecycle_Cannot_Complete_Before_Accept` | Validates state machine: cannot complete a handover that hasn't been accepted | ❌ Not covered |

**Key scenarios to replicate:**
```csharp
// 1. Create handover (Draft status)
// 2. Ready handover (Ready status)
// 3. Start handover (InProgress status) - verify StartedAt is set
// 4. Accept handover (Accepted status) - verify AcceptedAt is set
// 5. Complete handover (Completed status) - verify CompletedAt is set
// 6. Verify all timestamps are set correctly at each step
// 7. Verify patient data is still accessible after completion
```

---

## 🔴 Priority 2: Handover Constraint Tests (Database-Level Business Rules)

### Unique Constraint Validation
**Source:** `HandoverConstraintTests.cs`

These tests verify that database constraints enforce business rules.

| Test Name | Description | Status |
|-----------|-------------|--------|
| `Parallel_Creation_Same_Patient_Fails_With_Conflict` | Cannot create duplicate active handovers for same patient/shift/day | ❌ Not covered |
| `Can_Create_Second_Handover_After_Completing_First` | Can create new handover after completing previous one | ❌ Not covered |
| `Can_Create_Different_Shift_Transitions_Same_Patient` | Can have multiple handovers for same patient with different shift pairs | ❌ Not covered |

**Key scenarios to replicate:**
```csharp
// 1. Create handover for Patient A, Shift Day→Night
// 2. Try to create another handover for Patient A, Shift Day→Night (same day)
// 3. Should fail with unique constraint violation (ORA-00001)
// 4. Complete the first handover
// 5. Now should be able to create a new handover for same patient/shift/day
```

---

## 🟡 Priority 3: Detailed GetHandoverById Tests

### Comprehensive Response Validation
**Source:** `HandoverByIdEndpoints.cs`

The current `api` tests are basic. These tests provide more thorough validation.

| Test Name | Description | Status |
|-----------|-------------|--------|
| `GetHandoverById_ReturnsCorrectHandoverStructure` | Validates all fields in response structure | ⚠️ Partial |
| `GetHandoverById_ReturnsHandoverWithPatientName_WhenPatientExists` | Verifies patient name is included | ❌ Not covered |
| `GetHandoverById_ReturnsCorrectIllnessSeverity` | Validates illness severity values (Stable/Watcher/Unstable) | ❌ Not covered |
| `GetHandoverById_ReturnsCorrectShiftInformation` | Validates shift name format | ❌ Not covered |
| `GetHandoverById_HandlesDifferentHandoverStatuses` | Tests all status values (Active/InProgress/Completed/Ready) | ❌ Not covered |
| `GetHandoverById_ReturnsOptionalFields_WhenPresent` | Verifies optional fields are returned when data exists | ❌ Not covered |

**Key assertions to add:**
```csharp
// Verify illness severity structure
Assert.NotNull(result.illnessSeverity);
Assert.Contains(result.illnessSeverity.severity, new[] { "Stable", "Watcher", "Unstable" });

// Verify patient summary structure
Assert.NotNull(result.patientSummary);
Assert.NotNull(result.patientSummary.content);

// Verify action items have correct structure
foreach (var actionItem in result.actionItems)
{
    Assert.NotNull(actionItem.id);
    Assert.NotNull(actionItem.description);
}
```

---

## 🟡 Priority 4: Me Endpoints - Integration Flow

### Assignments and Patients Flow
**Source:** `MeEndpoints.cs`

| Test Name | Description | Status |
|-----------|-------------|--------|
| `AssignmentsAndGetMyPatients_Flow_Works` | Full flow: POST assignments → GET my patients → verify assigned patients | ❌ Not covered |

**Key scenario:**
```csharp
// 1. POST /me/assignments with ShiftId and PatientIds
// 2. GET /me/patients
// 3. Verify the assigned patients are returned
// 4. Verify pagination works correctly
```

---

## 🟡 Priority 5: Patients Endpoints - Advanced Pagination

### Comprehensive Pagination Tests
**Source:** `PatientsEndpoints.cs`

| Test Name | Description | Status |
|-----------|-------------|--------|
| `GetAllPatients_ReturnsAllPatientsFromAllUnits` | Verifies patients from all units are returned | ⚠️ Partial |
| `GetAllPatients_UsesDefaultPagination` | Verifies default page=1, pageSize=25 | ❌ Not covered |
| `GetAllPatients_WithCustomPagination` | Tests custom page/pageSize parameters | ⚠️ Partial |
| `GetAllPatients_WithPageBeyondAvailableData` | Page 100 returns empty list but correct total | ❌ Not covered |
| `GetAllPatients_CalculatesTotalPagesCorrectly` | Verifies TotalPages calculation | ❌ Not covered |
| `GetAllPatients_WithLargePageSize` | pageSize=1000 returns all patients in one page | ❌ Not covered |

---

## 🟡 Priority 6: Patient Handovers - Edge Cases

### Pagination and Edge Cases
**Source:** `PatientHandoversEndpoints.cs`

| Test Name | Description | Status |
|-----------|-------------|--------|
| `GetPatientHandovers_ReturnsEmptyList_WhenPatientHasNoHandovers` | Non-existent patient returns empty list | ❌ Not covered |
| `GetPatientHandovers_ReturnsCorrectHandoverStructure` | Validates handover response structure | ⚠️ Partial |
| `GetPatientHandovers_ReturnsHandoversInCorrectOrder` | Verifies ordering by creation date | ❌ Not covered |
| `GetPatientHandovers_HandlesInvalidPageNumber` | page=0 defaults to 1 | ❌ Not covered |
| `GetPatientHandovers_HandlesInvalidPageSize` | pageSize=0 defaults to 25 | ❌ Not covered |

---

## 📋 Implementation Checklist

### High Priority (Must Have)
- [x] `HandoverLifecycle_DoctorA_To_DoctorB_Should_Succeed`
- [x] `HandoverLifecycle_Cannot_Accept_Before_Start`
- [x] `HandoverLifecycle_Cannot_Complete_Before_Accept`
- [x] `Parallel_Creation_Same_Patient_Fails_With_Conflict`
- [x] `Can_Create_Second_Handover_After_Completing_First` (Partially covered by Constraint logic)

### Medium Priority (Should Have)
- [x] `AssignmentsAndGetMyPatients_Flow_Works`
- [x] `GetHandoverById_ReturnsCorrectHandoverStructure` (enhance existing)
- [x] `GetHandoverById_ReturnsCorrectIllnessSeverity` (Covered in DetailedTests)
- [x] `GetAllPatients_WithPageBeyondAvailableData`
- [x] `GetPatientHandovers_ReturnsEmptyList_WhenPatientHasNoHandovers`

### Lower Priority (Nice to Have)
- [ ] `GetAllPatients_UsesDefaultPagination`
- [ ] `GetAllPatients_CalculatesTotalPagesCorrectly`
- [ ] `GetPatientHandovers_HandlesInvalidPageNumber`
- [ ] `GetPatientHandovers_HandlesInvalidPageSize`
- [ ] `Can_Create_Different_Shift_Transitions_Same_Patient`

---

## 🛠️ Implementation Notes

### Test Setup Requirements

1. **Dynamic Test Data**: Use `DapperTestSeeder` pattern with unique IDs per test run
2. **Cleanup Strategy**: Each test should clean up its own data using unique test IDs
3. **No DDL in Tests**: Never create/drop tables in tests (shared database)
4. **State Machine Tests**: Need to create fresh handovers per test to avoid state conflicts


---

## 📊 Current Coverage Summary

| Category | Legacy Tests | API Tests | Gap |
|----------|-------------|-----------|-----|
| Handover Lifecycle | 3 | 0 | 3 |
| Constraint Validation | 3 | 0 | 3 |
| GetHandoverById | 10 | 2 | 8 |
| Me Endpoints Flow | 1 | 0 | 1 |
| Patients Pagination | 8 | 2 | 6 |
| Patient Handovers | 8 | 1 | 7 |
| **Total** | **33** | **5** | **28** |

---

## References

- Legacy tests: `relevo-api/tests/Relevo.FunctionalTests/ApiEndpoints/`
- New API tests: `api/tests/Relevo.FunctionalTests/ApiEndpoints/`
- Test seeder: `api/tests/Relevo.FunctionalTests/DapperTestSeeder.cs`

