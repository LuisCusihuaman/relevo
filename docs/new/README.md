# Relevo Handover System Documentation

## Overview

This directory contains comprehensive technical documentation for the Relevo handover system, covering the state machine, database schema, API contracts, and known issues.

---

## Documentation Files

### 📊 [HANDOVER_STATE_MACHINE.md](./HANDOVER_STATE_MACHINE.md)
Complete specification of the handover lifecycle state machine.

**Contents**:
- All 8 handover states (Draft, Ready, InProgress, Accepted, Completed, Cancelled, Rejected, Expired)
- Valid state transitions with API endpoints
- Database implementation (timestamp-driven via `VW_HANDOVERS_STATE`)
- Business rule enforcement
- Known pitfalls and edge cases

**Key Findings**:
- ✅ State transitions now properly validated (Accept requires Start, Complete requires Accept)
- ✅ `HANDOVER_WINDOW_DATE` now populated to prevent constraint violations
- ⚠️ Expiration automation not yet implemented
- ⚠️ Concurrent state changes need optimistic locking

---

### 🏥 [PATIENT_HANDOVER_STATUS.md](./PATIENT_HANDOVER_STATUS.md)
How patient status relates to handovers and how it's calculated.

**Contents**:
- Patient assignment states (derived from handovers)
- `GetPatientsByUnit` query logic
- Physician status calculation per role (creator vs receiver)
- Relationship between `USER_ASSIGNMENTS` and `HANDOVERS` tables

**Key Findings**:
- ✅ Fixed: Assigned patients were being excluded from unit views (filter removed)
- Patient status is **derived** from most recent handover, not stored
- Physician status differs based on relationship to handover

---

### 🔒 [DATABASE_CONSTRAINTS_AUDIT.md](./DATABASE_CONSTRAINTS_AUDIT.md)
Complete audit of all database constraints and their business purpose.

**Contents**:
- Unique constraints (especially `UQ_ACTIVE_HANDOVER_WINDOW`)
- Foreign key constraints and cascade behavior
- Check constraints for enums
- Performance indexes
- Constraint violation issues and solutions

**Key Findings**:
- ✅ Fixed: `UQ_ACTIVE_HANDOVER_WINDOW` now works correctly with populated dates
- Test cleanup must follow dependency order (children before parents)
- Missing cascade deletes make cleanup complex
- Recommendation: Add timestamp consistency constraints

---

### 🌐 [FRONTEND_BACKEND_GAPS.md](./FRONTEND_BACKEND_GAPS.md)
Identifies inconsistencies between frontend (React/TypeScript) and backend (C#/.NET).

**Contents**:
- State naming comparison (mostly consistent ✅)
- API response field case sensitivity (mixed PascalCase/camelCase ⚠️)
- Missing features (physician colors ❌, role hardcoded ⚠️)
- Authorization issues (endpoints use `AllowAnonymous()` ⚠️)
- Error handling inconsistencies

**Key Findings**:
- ✅ Core state names and types are consistent
- ⚠️ Authorization not properly enforced (HIGH PRIORITY)
- ⚠️ Physician color preferences not implemented
- ⚠️ Error responses inconsistent (404 vs 409 vs 422)
- ❌ No real-time updates (need SignalR)

---

### 🐛 [PITFALLS_AND_SOLUTIONS.md](./PITFALLS_AND_SOLUTIONS.md)
Comprehensive catalog of all known issues, edge cases, and their solutions.

**Contents**:
- Fixed issues (5 major bugs resolved ✅)
- Remaining issues (12 open items ⚠️❌)
- Testing gaps
- Performance considerations
- Security concerns
- Priority matrix and action plan

**Key Findings**:
- ✅ 5 critical issues fixed during E2E test development
- ⚠️ Authorization is biggest remaining concern (HIGH PRIORITY)
- ❌ Missing test coverage for cancellation, rejection, edge cases
- ❌ Expired handover automation not implemented
- ⚠️ Concurrent state changes need optimistic locking

---

### 📐 [diagrams/handover-state-machine.mmd](./diagrams/handover-state-machine.mmd)
Mermaid diagram visualizing the handover state machine.

**Usage**:
- View in GitHub (renders automatically)
- Use Mermaid Live Editor: https://mermaid.live
- Include in documentation sites

---

## Quick Reference

### Handover States (Priority Order)
1. **Completed** - Handover successfully finished
2. **Cancelled** - Handover was cancelled
3. **Rejected** - Handover was rejected by receiver  
4. **Expired** - Handover window passed
5. **Accepted** - Receiver accepted the handover
6. **InProgress** - Handover actively being communicated
7. **Ready** - Creator marked handover ready
8. **Draft** - Initial state, being prepared

### Happy Path Transitions
```
POST /handovers              → Draft
POST /handovers/{id}/ready   → Ready
POST /handovers/{id}/start   → InProgress
POST /handovers/{id}/accept  → Accepted (requires InProgress) ✅
POST /handovers/{id}/complete → Completed (requires Accepted) ✅
```

### State Determination
States are calculated by `VW_HANDOVERS_STATE` view based on timestamp fields:
- `COMPLETED_AT` → Completed
- `CANCELLED_AT` → Cancelled
- `REJECTED_AT` → Rejected
- `EXPIRED_AT` → Expired
- `ACCEPTED_AT` → Accepted
- `STARTED_AT` → InProgress
- `READY_AT` → Ready
- All NULL → Draft

---

## Issues Fixed ✅

During E2E test development, the following critical issues were discovered and fixed:

1. **NULL HANDOVER_WINDOW_DATE** - Now set to `SYSTIMESTAMP` during creation
2. **Missing State Validation** - Accept now requires Start, Complete requires Accept
3. **Patient Filtering Bug** - Assigned patients no longer excluded from unit views
4. **Test Parallelization** - Sequential collection and better cleanup
5. **View Logic** - Simplified Completed state determination

---

## Critical Open Issues ⚠️

### HIGH PRIORITY (Security)
1. **Authorization Not Enforced** - Most endpoints use `AllowAnonymous()`
2. **No Role-Based Access Control** - Any user can perform any action
3. **Demo User in Production** - Fallback allows unauthorized access

### MEDIUM PRIORITY (Functionality)
4. **No Real-Time Updates** - Need SignalR for live synchronization
5. **Error Handling Inconsistent** - 404 vs 409 vs 422 confusion
6. **Concurrent State Changes** - Need optimistic locking
7. **Missing Test Coverage** - Cancellation, rejection, edge cases

### LOW PRIORITY (Quality of Life)
8. **Expired Handover Automation** - Background job not implemented
9. **Timezone Handling** - All timestamps in server timezone
10. **Physician Colors** - User preferences not implemented
11. **Performance** - Missing indexes on timestamp fields

---

## Recommendations

### Immediate Actions
1. **Implement proper authorization** - Remove `AllowAnonymous()`, add `[Authorize]` attributes
2. **Add role-based access control** - Verify user can perform actions on handovers
3. **Handle constraint violations** - Return 409 Conflict instead of 500 errors
4. **Standardize error responses** - Create error DTO and use appropriate status codes

### Short-Term Improvements
5. **Add E2E tests** - Cancellation, rejection, authorization checks
6. **Implement real-time updates** - SignalR hub for state change broadcasts
7. **Add optimistic locking** - VERSION field for concurrent update detection
8. **Implement user preferences** - Colors, roles from database

### Long-Term Enhancements
9. **Expired handover automation** - Daily job to mark expired handovers
10. **Timezone handling** - Store UTC, convert on client
11. **Database constraints** - Enforce timestamp consistency
12. **Performance optimization** - Indexes, query tuning, monitoring

---

## Testing Status

### E2E Tests ✅
- Happy path: Draft → Ready → InProgress → Accepted → Completed
- Invalid transitions: Cannot Accept before Start, Cannot Complete before Accept

### Missing Tests ❌
- Cancellation from various states
- Rejection with reason
- Concurrent state changes
- Authorization checks
- Constraint violation handling
- Edge cases (double transitions, etc.)

---

## Database Schema Summary

### Core Tables
- **HANDOVERS** - Main handover records with timestamp fields
- **PATIENTS** - Patient demographics
- **USER_ASSIGNMENTS** - Doctor-patient assignments per shift
- **USERS** - User/doctor information

### Child Tables (1:1)
- **HANDOVER_PATIENT_DATA** - Illness severity and summary (singleton)
- **HANDOVER_SITUATION_AWARENESS** - I-PASS awareness section (singleton)
- **HANDOVER_SYNTHESIS** - Synthesis/summary section (singleton)

### Child Tables (1:many)
- **HANDOVER_ACTION_ITEMS** - Action items/todos
- **HANDOVER_PARTICIPANTS** - Active participants in handover
- **HANDOVER_MESSAGES** - Discussion messages
- **HANDOVER_ACTIVITY_LOG** - Audit trail
- **HANDOVER_CHECKLISTS** - Checklist items
- **HANDOVER_CONTINGENCY** - Contingency plans

### Key Constraints
- **UQ_ACTIVE_HANDOVER_WINDOW** - One active handover per patient/shift/window
- **FK_HANDOVERS_FROM_DOCTOR** - Valid user references
- **CHK_HANDOVER_TYPE** - Valid handover types only
- **CHK_HP_ILLNESS_SEVERITY** - Valid severity levels only

---

## Frontend Integration

### TypeScript Types
Frontend types in `relevo-frontend/src/api/types.ts` are mostly consistent with backend:
- ✅ Handover state names match exactly
- ✅ Timestamp formats compatible (ISO 8601)
- ⚠️ Mixed PascalCase/camelCase in responses
- ⚠️ Physician status types not strictly defined

### API Hooks
Frontend has hooks for all state transitions:
- `useReadyHandover()`
- `useStartHandover()`
- `useAcceptHandover()`
- `useCompleteHandover()`
- `useCancelHandover()`
- `useRejectHandover()`

All map correctly to backend endpoints ✅

---

## Performance Notes

### Optimized
- Indexes on common query fields (patient ID, doctor ID, status)
- Pagination in `GetPatientsByUnit`
- Window functions for most recent handover per patient

### Needs Optimization
- Missing indexes on timestamp fields (READY_AT, STARTED_AT, etc.)
- N+1 query potential in patient list with handovers
- No caching strategy documented

---

## Security Notes

### Current Issues
- ⚠️ **Most endpoints are anonymous** - Only custom middleware for auth
- ⚠️ **No RBAC** - Anyone can act on any handover
- ⚠️ **Demo user fallback** - Allows unauthorized access
- ⚠️ **No audit logging** - Can't track who changed what

### Recommendations
1. Enable proper authentication with `[Authorize]` attributes
2. Implement role checks (only assigned doctor can accept, etc.)
3. Environment-specific demo user (dev only)
4. Add comprehensive audit logging to `HANDOVER_ACTIVITY_LOG`

---

## Related Documentation

### In Repository
- `docs/API_SCHEMA.md` - Original API schema (may be outdated)
- `docs/DATABASE.md` - Database design notes
- `docs/BACKEND_APP.md` - Backend architecture
- `docs/UX_APP.md` - Frontend UX flows

### External
- [TanStack Query Docs](https://tanstack.com/query) - Frontend data fetching
- [FastEndpoints Docs](https://fast-endpoints.com/) - Backend API framework
- [Oracle SQL Reference](https://docs.oracle.com/en/database/) - Database

---

## Contributing

When making changes to the handover system:

1. **Update state machine docs** if adding/modifying states
2. **Update constraints audit** if changing database schema
3. **Update frontend-backend gaps** if API changes
4. **Add to pitfalls** if discovering new issues
5. **Update diagrams** if state machine changes

---

## Changelog

### 2025-10-12 - Initial Documentation
- Created comprehensive documentation based on E2E test development
- Documented all fixed issues and remaining gaps
- Added state machine diagram
- Identified security and functionality concerns

---

## Contact

For questions about this documentation or the handover system:
- Check the source code inline comments
- Review E2E tests in `tests/Relevo.FunctionalTests/ApiEndpoints/HandoverLifecycleEndpoints.cs`
- Consult database schema in `src/Relevo.Infrastructure/Sql/`

---

*Last Updated: October 12, 2025*
*Documentation Generated During: E2E Test Development & System Analysis*

