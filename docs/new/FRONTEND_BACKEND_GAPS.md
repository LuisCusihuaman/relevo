# Frontend-Backend Inconsistencies & Gaps

## Overview

This document identifies mismatches, inconsistencies, and integration issues between the frontend (React/TypeScript) and backend (C#/.NET) implementations.

---

## State Naming Differences

### ✅ CONSISTENT: Handover States

**Backend** (`VW_HANDOVERS_STATE` view):
```sql
'Draft' | 'Ready' | 'InProgress' | 'Accepted' | 'Completed' | 'Cancelled' | 'Rejected' | 'Expired'
```

**Frontend** (`relevo-frontend/src/api/types.ts` line 129):
```typescript
stateName: "Draft" | "Ready" | "InProgress" | "Accepted" | "Completed" | "Cancelled" | "Rejected" | "Expired"
```

**Status**: ✅ **CONSISTENT** - State names match exactly between backend and frontend.

---

### ⚠️ INCONSISTENT: Physician Status Values

**Backend** (`GetById.cs` line 179-196):
```csharp
"handing-off" | "handed-off" | "pending" | "ready-to-receive" | "receiving" | 
"accepted" | "completed" | "cancelled" | "rejected" | "expired" | "unknown"
```

**Frontend**: Uses these values but TypeScript types may not be defined.

**Issue**: Frontend may not have strict type checking for physician status values.

**Recommendation**: 
1. Create TypeScript enum/union type for physician status
2. Add validation to ensure consistency

```typescript
// Recommended addition to frontend types
export type PhysicianStatus = 
  | "handing-off" 
  | "handed-off" 
  | "pending" 
  | "ready-to-receive" 
  | "receiving" 
  | "accepted" 
  | "completed" 
  | "cancelled" 
  | "rejected" 
  | "expired" 
  | "unknown";
```

---

### ⚠️ DIFFERENT: Patient Handover Status

**Backend** (`GetPatientsByUnit`):
Returns actual handover state: `"Draft"`, `"Ready"`, `"InProgress"`, etc.

**Frontend Expected** (implied from `relevo-frontend/src/components/home/types.ts`):
```typescript
status: "Error" | "Ready"  // Only two values?
```

**Issue**: Frontend home view types suggest only "Error" or "Ready" statuses, but backend returns all 8 possible states.

**Recommendation**:
1. Update frontend types to match backend states
2. Or map backend states to frontend display states consistently

---

## API Response Mismatches

### ⚠️ CASE SENSITIVITY: Response Field Names

**Backend Response** (`GetHandoverByIdResponse`):
```csharp
public class GetHandoverByIdResponse {
    public string Id { get; set; }
    public IllnessSeverityDto illnessSeverity { get; set; }  // ← camelCase
    public PatientSummaryDto patientSummary { get; set; }    // ← camelCase
    public SynthesisDto? synthesis { get; set; }              // ← camelCase
    public string StateName { get; set; }                     // ← PascalCase
}
```

**Frontend Type** (`relevo-frontend/src/api/types.ts`):
```typescript
export type Handover = {
  id: string;
  illnessSeverity: HandoverIllnessSeverity;  // ← camelCase
  patientSummary: HandoverPatientSummary;    // ← camelCase
  synthesis?: HandoverSynthesis;              // ← camelCase
  stateName: ...                              // ← camelCase
}
```

**Issue**: Backend uses mixed PascalCase and camelCase. Frontend expects camelCase.

**Current Behavior**: Likely works due to JSON serialization settings, but inconsistent.

**Recommendation**:
1. Standardize on camelCase for all JSON responses
2. Configure `System.Text.Json` options:
```csharp
services.Configure<JsonOptions>(options => {
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
```

---

### ✅ TIMESTAMP FORMAT: ISO 8601

**Backend** (`GetHandoverByIdResponse`):
```csharp
public string? CreatedAt { get; set; }  // Returns string
HandoverWindowDate = handover.HandoverWindowDate?.ToString("yyyy-MM-ddTHH:mm:ss")
```

**Frontend**:
Expects ISO 8601 strings, which `"yyyy-MM-ddTHH:mm:ss"` provides.

**Status**: ✅ **CONSISTENT** - Format matches.

---

### ⚠️ NULL vs UNDEFINED: Optional Fields

**Backend**:
```csharp
public string? PatientName { get; set; }  // null if not set
public string? ReadyAt { get; set; }      // null if not set
```

**Frontend**:
```typescript
patientName?: string;  // undefined or string
readyAt?: string;      // undefined or string
```

**Issue**: C# nulls serialize to JSON `null`, not omitted. TypeScript `?` means field can be absent OR null.

**Current Behavior**: Frontend receives `null` for optional fields, not `undefined`.

**Recommendation**:
1. Frontend should check for both `null` and `undefined`
2. Or configure backend to omit null values:
```csharp
options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
```

---

## Missing State Transitions in Frontend

### Frontend State Mutation Hooks

**Available in Frontend** (`relevo-frontend/src/pages/handover.tsx` lines 86-91):
```typescript
const { mutate: readyState } = useReadyHandover();
const { mutate: startState } = useStartHandover();
const { mutate: acceptState } = useAcceptHandover();
const { mutate: completeState } = useCompleteHandover();
const { mutate: cancelState } = useCancelHandover();
const { mutate: rejectState } = useRejectHandover();
```

**Backend Endpoints Available**:
- ✅ POST /handovers/{id}/ready
- ✅ POST /handovers/{id}/start
- ✅ POST /handovers/{id}/accept
- ✅ POST /handovers/{id}/complete
- ✅ POST /handovers/{id}/cancel
- ✅ POST /handovers/{id}/reject

**Status**: ✅ **CONSISTENT** - All backend endpoints have corresponding frontend hooks.

---

## Patient Data Endpoint Consolidation

### ⚠️ DEPRECATED: Separate `/patient` and `/patient-data` Endpoints

**Old Design**:
- `GET /handovers/{id}/patient` - Patient demographics
- `GET /handovers/{id}/patient-data` - Medical data

**New Design** (`GetById.cs` lines 82-294):
- `GET /handovers/{id}/patient` - **Consolidated** - Returns BOTH demographics AND medical data

**Frontend Status**: May still be calling separate endpoints.

**Issue**: If frontend calls non-existent `/patient-data` endpoint, will get 404.

**Recommendation**:
1. Verify frontend only calls consolidated `/patient` endpoint
2. Remove any references to `/patient-data` endpoint from frontend
3. Update API documentation

---

## Physician Data Calculation

### Frontend Expectation vs Backend Calculation

**Frontend Expects** (`relevo-frontend/src/api/types.ts` implied):
Physician data with:
- `name`, `role`, `color`, `shiftStart`, `shiftEnd`, `status`, `patientAssignment`

**Backend Provides** (`GetPatientHandoverDataResponse` line 281-290):
```csharp
public class PhysicianDto {
    public string name { get; set; }           // ✅ From handover
    public string role { get; set; }           // ⚠️ Hardcoded "Doctor"
    public string color { get; set; }          // ❌ Empty string (TODO)
    public string? shiftEnd { get; set; }      // ✅ From SHIFTS table
    public string? shiftStart { get; set; }    // ✅ From SHIFTS table
    public string status { get; set; }         // ✅ Calculated from handover state
    public string patientAssignment { get; set; } // ✅ "assigned" or "receiving"
}
```

**Issues**:
1. ⚠️ **Role**: Hardcoded as "Doctor" instead of from user data
2. ❌ **Color**: Not implemented (returns empty string)
3. ⚠️ **Status Calculation**: Depends on `relationship` parameter ("creator" vs "assignee")

**Frontend Impact**:
- Color-coded physician indicators won't work
- All users show as "Doctor" regardless of actual role

**Recommendations**:
1. Implement user color preferences (store in `USER_PREFERENCES` table)
2. Read role from `USERS.ROLE` field instead of hardcoding
3. Document that physician status calculation differs by user relationship

---

## Handover Types

### ✅ CONSISTENT: Handover Type Enum

**Backend** (check constraint in `01-tables.sql`):
```sql
CHECK (HANDOVER_TYPE IN ('ShiftToShift','TemporaryCoverage','Consult'))
```

**Frontend** (`relevo-frontend/src/api/types.ts` line 124):
```typescript
handoverType?: "ShiftToShift" | "TemporaryCoverage" | "Consult";
```

**Status**: ✅ **CONSISTENT** - Values match exactly.

---

## Sync Status Values

### ✅ CONSISTENT: Sync Status Enum

**Backend** (`HANDOVER_SYNC_STATUS.SYNC_STATUS` field comment):
```sql
SYNC_STATUS VARCHAR2(20) DEFAULT 'synced'  
-- synced, syncing, pending, offline, error
```

**Frontend** (`relevo-frontend/src/common/types.ts` line 492):
```typescript
export type SyncStatus = "synced" | "syncing" | "pending" | "offline" | "error";
```

**Status**: ✅ **CONSISTENT** - Values match exactly.

---

## Authorization & Authentication

### ⚠️ INCONSISTENT: Auth Implementation

**Backend**:
- Uses Clerk for authentication
- Many endpoints use `AllowAnonymous()` with comment "Let our custom middleware handle authentication"
- Fallback demo user: `user_demo12345678901234567890123456`

**Frontend**:
- Uses Clerk `useUser()` hook
- Expects authenticated user data

**Issue**: 
1. Backend endpoints are anonymous but rely on middleware that may not be enforcing auth
2. Demo user fallback might allow unauthorized access in production

**Recommendations**:
1. Remove `AllowAnonymous()` from production endpoints
2. Implement proper `[Authorize]` attributes
3. Only use demo user fallback in development environment
4. Ensure middleware properly validates Clerk tokens

---

## Error Handling

### ⚠️ INCONSISTENT: Error Response Formats

**Backend Behaviors**:
- State transition failures: Return `404 Not Found` (e.g., Accept without Start)
- Validation errors: Return `400 Bad Request` (expected)
- Constraint violations: Return `500 Internal Server Error` (not caught)

**Frontend Expectations**: Unknown (no error type definitions found in search).

**Issues**:
1. Invalid state transitions return 404 (not found) instead of 409 (conflict) or 422 (unprocessable)
2. Unique constraint violations bubble up as 500 errors instead of being caught and returning meaningful errors

**Recommendations**:
1. Define error response DTO:
```csharp
public class ErrorResponse {
    public string Error { get; set; }
    public string Message { get; set; }
    public string? Detail { get; set; }
}
```

2. Return appropriate status codes:
   - `409 Conflict` for unique constraint violations
   - `422 Unprocessable Entity` for invalid state transitions
   - `400 Bad Request` for validation errors

3. Catch database exceptions and map to user-friendly errors

---

## Real-Time Updates

### ❌ MISSING: WebSocket/SignalR Integration

**Backend**:
- No WebSocket or SignalR hub found
- State changes not broadcast to connected clients

**Frontend**:
- Sync status component exists (`useSyncStatus`)
- No real-time updates implementation found

**Issue**: 
When Doctor A updates a handover, Doctor B's view doesn't update until manual refresh.

**Recommendations**:
1. Implement SignalR hub for real-time handover updates
2. Broadcast state changes to all participants
3. Implement optimistic UI updates with conflict resolution

---

## Pagination

### ⚠️ INCONSISTENT: Pagination Parameters

**Backend** (`GetPatientsByUnit`):
```csharp
public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> 
    GetPatientsByUnitAsync(string unitId, int page, int pageSize)
```
Uses `page` and `pageSize`.

**Frontend**: Likely expects same, but not verified in search results.

**Recommendation**: Document pagination contract:
- `page`: 1-based index
- `pageSize`: Number of items per page
- Response includes `totalCount` for pagination UI

---

## Summary of Key Issues

| Issue | Severity | Status | Recommendation |
|-------|----------|--------|----------------|
| Physician color not implemented | Medium | ❌ | Implement user preferences |
| Role hardcoded as "Doctor" | Low | ⚠️ | Read from USERS table |
| Mixed PascalCase/camelCase | Low | ⚠️ | Standardize on camelCase |
| Auth endpoints anonymous | High | ⚠️ | Implement proper authorization |
| Error responses inconsistent | Medium | ⚠️ | Standardize error format & codes |
| No real-time updates | Medium | ❌ | Implement SignalR |
| Constraint violations unhandled | Medium | ⚠️ | Catch and return 409 Conflict |
| Demo user in production | High | ⚠️ | Environment-specific fallback |
| Physician status types undefined | Low | ⚠️ | Add TypeScript types |
| Patient status mapping unclear | Medium | ⚠️ | Document state mapping |

---

## Testing Recommendations

### Integration Tests Needed

1. **API Contract Tests**: Verify frontend types match backend responses
2. **State Transition Tests**: Test all transitions from frontend
3. **Error Handling Tests**: Verify frontend handles all backend error formats
4. **Authentication Tests**: Verify auth works end-to-end
5. **Real-Time Update Tests**: When implemented, test broadcast behavior

### Frontend Type Generation

**Recommendation**: Generate TypeScript types from C# DTOs using tools like:
- NSwag
- Swagger CodeGen
- Custom T4 templates

This would eliminate manual type synchronization and catch mismatches at build time.

---

## Action Items

### High Priority
1. ✅ Fix state transition validation (DONE - Accept/Complete checks added)
2. ✅ Fix HANDOVER_WINDOW_DATE NULL issue (DONE)
3. ⚠️ Implement proper authorization (replace `AllowAnonymous`)
4. ⚠️ Handle constraint violation exceptions (return 409 instead of 500)

### Medium Priority
5. ⚠️ Standardize JSON property naming (camelCase everywhere)
6. ⚠️ Implement user color preferences
7. ⚠️ Read user roles from database instead of hardcoding
8. ❌ Implement real-time updates (SignalR)
9. ⚠️ Standardize error response format

### Low Priority
10. ⚠️ Generate TypeScript types from C# DTOs
11. ⚠️ Add physician status TypeScript enum
12. ⚠️ Document pagination contract
13. ⚠️ Add API versioning strategy

---

## Conclusion

The frontend and backend are **mostly consistent** in core functionality (state names, endpoints, types), but have several areas needing attention:

**Strengths**:
- State machine implementation matches
- API endpoints map to frontend hooks
- Core data types are consistent

**Weaknesses**:
- Authorization not properly enforced
- Error handling inconsistent
- Some features not implemented (colors, roles)
- No real-time synchronization

**Next Steps**:
1. Address high-priority security issues (auth, error handling)
2. Implement missing features (colors, roles, real-time)
3. Add type generation to prevent future mismatches

