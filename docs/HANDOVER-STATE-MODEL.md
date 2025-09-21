## Handover and Doctor States — Source of Truth

This document describes how handovers and doctor-related states are modeled in the system, how the backend computes the “active handover,” and where potential product/design decisions may remove ambiguity. It compiles current behavior from code and schema to support debugging and planning.

### Entities and Key Fields

- **Handover (table `HANDOVERS`)**
  - **Status**: `Active` | `InProgress` | `Completed` | `Cancelled`
  - **From/To doctor**: `FROM_DOCTOR_ID`, `TO_DOCTOR_ID` (receiving doctor)
  - **Timing**: `CREATED_AT`, `ACCEPTED_AT`, `COMPLETED_AT`
  - **Context**: `PATIENT_ID`, `SHIFT_NAME`, `ILLNESS_SEVERITY`, `PATIENT_SUMMARY`, `SYNTHESIS`
  - Indices: `STATUS`, `FROM_DOCTOR_ID`, `TO_DOCTOR_ID`, `PATIENT_ID`, `CREATED_BY`

- **Handover Sections (`HANDOVER_SECTIONS`)**
  - **Section type**: `illness_severity` | `patient_summary` | `action_items` | `situation_awareness` | `synthesis`
  - **Section status**: `draft` | `in_progress` | `completed`

- **Participants (`HANDOVER_PARTICIPANTS`)**
  - **Status**: `active` | `inactive` | `viewing`
  - Tracks who is currently involved in the handover session and their role.

- **Sync status (`HANDOVER_SYNC_STATUS`)**
  - **Sync state**: `synced` | `syncing` | `pending` | `offline` | `error`
  - Per-user sync/version metadata for collaborative editing.

- **Checklists (`HANDOVER_CHECKLISTS`)** and **Contingency Plans (`HANDOVER_CONTINGENCY`)**
  - Checklist items do not carry a global state beyond their own completion flags.
  - Contingency plan status: `active` | `planned` | `completed`.

### What is the “Active Handover”?

- The backend computes “active handover” for the current user by selecting the most recent handover where the user is the receiving doctor (`TO_DOCTOR_ID`) and the handover status is either `Active` or `InProgress`.
- Implementation (repository):

```516:555:relevo/relevo-api/src/Relevo.Infrastructure/Repositories/OracleSetupRepository.cs
// Find the active handover for this user's assigned patients
const string handoverSql = "SELECT ... FROM HANDOVERS h ... WHERE h.TO_DOCTOR_ID = :userId AND h.STATUS IN ('Active', 'InProgress') ORDER BY h.CREATED_AT DESC";
```

- Endpoint contract:

```14:60:relevo/relevo-api/src/Relevo.Web/Me/ActiveHandover.Get.cs
Get("/me/handovers/active");
// ...
var activeHandover = await _setupService.GetActiveHandoverAsync(user.Id);
if (activeHandover == null) { await SendNotFoundAsync(ct); return; }
Response = new GetActiveHandoverResponse
{
  Handover = activeHandover,
  Participants = await _setupService.GetHandoverParticipantsAsync(activeHandover.Id),
  Sections = await _setupService.GetHandoverSectionsAsync(activeHandover.Id),
  SyncStatus = await _setupService.GetHandoverSyncStatusAsync(activeHandover.Id, user.Id)
};
```

- Frontend API wrapper/hook:

```19:54:relevo/relevo-frontend/src/api/endpoints/handover.ts
export async function getActiveHandover(): Promise<ActiveHandoverData> {
  const { data } = await api.get<ActiveHandoverData>("/me/handovers/active");
  return data;
}
export function useActiveHandover() {
  return useQuery({ queryKey: ["handover","active"], queryFn: () => getActiveHandover(), ... });
}
```

### Handover Lifecycle (State Machine)

- **Created → Active**
  - Created by a sending doctor with a target `TO_DOCTOR_ID` (receiving doctor).
  - Awaiting acceptance by the receiving doctor.

- **Active → InProgress** (Accepted)
  - Trigger: receiving doctor accepts the handover.
  - Backend action:

```294:306:relevo/relevo-api/src/Relevo.Web/Setup/OracleSetupDataProvider.cs
UPDATE HANDOVERS SET STATUS = 'InProgress', ACCEPTED_AT = SYSTIMESTAMP
WHERE ID = :handoverId AND TO_DOCTOR_ID = :userId AND STATUS = 'Active'
```

- **InProgress → Completed**
  - Trigger: receiving doctor completes the handover after performing required actions.
  - Backend action:

```308:319:relevo/relevo-api/src/Relevo.Web/Setup/OracleSetupDataProvider.cs
UPDATE HANDOVERS SET STATUS = 'Completed', COMPLETED_AT = SYSTIMESTAMP, COMPLETED_BY = :userId
WHERE ID = :handoverId AND TO_DOCTOR_ID = :userId AND STATUS = 'InProgress'
```

- **Cancellation**
  - Optional path to `Cancelled` (not yet wired in endpoints). Would typically be allowed from `Active` or `InProgress` with business rules.

### Section Workflow (I-PASS)

- Sections exist per handover and carry their own status independent of the handover’s global status.
- Suggested usage:
  - `draft`: Newly created or partly filled
  - `in_progress`: Being edited collaboratively
  - `completed`: Finalized for handover

### Participants and Presence

- Participants represent users present in a handover session. Status progression is not enforced by code today; it indicates presence/activity.
  - `active`: currently participating
  - `viewing`: passively observing
  - `inactive`: not currently active

### Doctor-Centric Views and States

- For a given doctor (current user):
  - **Pending handovers list**: the set of handovers where `TO_DOCTOR_ID = currentUserId` and `STATUS IN ('Active','InProgress')`.
  - **Active handover (singular)**: most recent matching handover ordered by `CREATED_AT DESC`.
  - A sending doctor (`FROM_DOCTOR_ID = currentUserId`) will NOT see their own created-but-unaccepted handover as “active” via `/me/handovers/active` because the filter only checks `TO_DOCTOR_ID`.

### API Endpoints (relevant)

- `GET /me/handovers/active`
  - 200 with payload `{ handover, participants, sections, syncStatus }` when found
  - 404 when no handover for current user matches criteria

- `POST /handovers/{handoverId}/accept`
  - Transitions `Active → InProgress` for the receiving doctor.

- `POST /handovers/{handoverId}/complete`
  - Transitions `InProgress → Completed` for the receiving doctor.

- `GET /patients/{patientId}/handovers`
  - Historical per-patient list, regardless of doctor.

### Known Pitfalls / Likely Causes of 404 on “Active”

- **Receiving-doctor filter**: Only `TO_DOCTOR_ID` is considered when searching for active handovers. If you are the sending doctor, you will receive 404 even if you just created a handover.
- **Status filter**: Only `Active` and `InProgress` are considered. Handovers in `Completed` or `Cancelled` will not appear.
- **Demo user mismatch**: If the current (fallback) demo user ID doesn’t match `TO_DOCTOR_ID` in seeded handovers, the query returns no rows.
  - Logs show fallback user as `user_2demo12345678901234567890123456`.
  - Schema seeding and sample rows commonly use IDs like `user_demo12345678901234567890123456` (note the missing `2`).
  - Result: `GET /me/handovers/active` returns 404 even though seed contains an `Active` handover.

### Decision Points for Product

- **Definition of “active handover”**
  - Current: receiving doctor’s most recent handover with status in `Active|InProgress`.
  - Options:
    - Restrict to `InProgress` only, making “active” represent accepted/ongoing work.
    - Include `Active` (awaiting acceptance) only when the current user is the receiving doctor.
    - Include sending-doctor perspective (e.g., a separate endpoint or toggle).

- **Multiple concurrent handovers**
  - If multiple `Active|InProgress` handovers exist for the same doctor, the system picks the latest by `CREATED_AT`. Consider surfacing all in UI or enforcing one-at-a-time.

- **Cancellation semantics**
  - Clarify who can cancel, at which states, and how UI should surface cancellation vs completion.

- **Section completion gating**
  - Decide if handover can be completed only when certain sections reach `completed`.

### Troubleshooting Checklist

- Verify `TO_DOCTOR_ID` equals the current user ID.
- Verify `STATUS` is `Active` or `InProgress`.
- Check for demo ID mismatches in seed data vs middleware fallback.
- Confirm endpoint is called with the same auth context as other endpoints that succeed.

### Glossary

- **Receiving doctor**: The doctor assigned to take over care (`TO_DOCTOR_ID`).
- **Sending doctor**: The doctor handing off care (`FROM_DOCTOR_ID`).
- **Active handover** (current impl): Latest handover for receiving doctor with status `Active` or `InProgress`.


