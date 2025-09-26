Go with **Normalized Schema + Specific Endpoints** for singletons, and **plural collection endpoints** for lists. Keep the current “/sections/{sectionId}” route only as a short-term compatibility shim while you migrate the UI.

Why: it gives you strong data integrity, predictable URLs, easier queries/indices, and a clean path to immutable historical snapshots—all of which your domain really needs.

---

# What you have today (fact check)

## Backend

* You **update handover sections** through a generic endpoint:

  * `PUT /me/handovers/{handoverId}/sections/{sectionId}` handled by `UpdateHandoverSectionEndpoint`. &#x20;
* Those sections are stored in a **generic table** with a big text blob:

  * `HANDOVER_SECTIONS(ID, HANDOVER_ID, SECTION_TYPE, CONTENT CLOB, STATUS, ...)`.&#x20;
  * Repository queries pull `SECTION_TYPE`, `CONTENT`, etc. from `HANDOVER_SECTIONS`.&#x20;
* Meanwhile, you also keep some **singleton-ish fields directly on HANDOVERS** (dup risk):

  * `ILLNESS_SEVERITY`, `PATIENT_SUMMARY`, `SYNTHESIS` selected from `HANDOVERS`.&#x20;
* Collections are already modeled right:

  * **Action items** have their own table and dedicated endpoints:

    * Table: `HANDOVER_ACTION_ITEMS(...)`.&#x20;
    * Endpoint: `PUT /me/handovers/{handoverId}/action-items/{itemId}`.&#x20;
* You have **state transition endpoints** (`/ready`, `/start`, `/accept`, `/complete`, `/cancel`, `/reject`)—great for immutability rules later.&#x20;
* There’s also a **separate patient summaries table** (`PATIENT_SUMMARIES`) that’s independent of handovers.&#x20;

## Frontend

* The UI still **talks to the generic sections endpoint**:

  * `api.put("/me/handovers/{handoverId}/sections/{sectionId}", { content, status })`.&#x20;
* UI also treats sections as a **list** and finds by type via helper: `getSectionByType(sections, sectionType)`.&#x20;
* For collections, the UI already uses **plural routes** (action items, contingency plans, etc.): e.g., `PUT /action-items/{actionItemId}`, `GET /contingency-plans`. &#x20;

---

# Why this is risky to keep as-is

1. **Data integrity & queryability**
   A single `HANDOVER_SECTIONS` CLOB can’t enforce structure or allow indexed queries by field/value (e.g., “find all with Critical severity”).&#x20;
2. **Duplication & drift**
   You store section content both in `HANDOVERS` and in `HANDOVER_SECTIONS`, which invites inconsistency. &#x20;
3. **Ambiguous contracts**
   A single `PUT /sections/{sectionId}` must accept different payloads depending on `SECTION_TYPE`. This makes validation and typing fuzzy (you called this out in your doc).

---

# Target design (recommended)

## 1) Database (normalized singletons + collections)

Keep your good collection tables (Action Items, Contingency Plans, Messages). Split the “big ball of mud” singletons into **one table per singleton**:

* `HANDOVER_PATIENT_DATA(HANDOVER_ID PK/FK, ILLNESS_SEVERITY, SUMMARY_TEXT, ... )`
* `HANDOVER_SITUATION_AWARENESS(HANDOVER_ID PK/FK, CONTENT, STATUS, LAST_EDITED_BY, ...)`
* `HANDOVER_SYNTHESIS(HANDOVER_ID PK/FK, CONTENT, STATUS, LAST_EDITED_BY, ...)`

You already demonstrated the pattern with `HANDOVER_ACTION_ITEMS` and `PATIENT_SUMMARIES` (separate, typed tables). Build on that. &#x20;

**Immutability:**
Treat each `HANDOVER_ID` as the snapshot boundary; create new handovers for new windows, and **prevent updates** after `Completed`/`Expired` with DB constraints/triggers or repository guards (you already have state transitions to hang these rules on).&#x20;

## 2) API endpoints (explicit contracts)

* **Singletons** (GET|PUT):

  * `/api/handovers/{id}/patient-data`
  * `/api/handovers/{id}/situation-awareness`
  * `/api/handovers/{id}/synthesis`
* **Collections** (GET|POST and item-level GET|PUT|DELETE):

  * `/api/handovers/{id}/action-items`
  * `/api/handovers/{id}/contingency-plans`
  * `/api/handovers/{id}/messages`

This mirrors the frontend’s existing plural patterns (action items, contingency) and removes ambiguity for singletons. (Contrast with today’s `/sections/{sectionId}`.) &#x20;

---

# Migration plan (no downtime, minimum churn)

**Phase 0 — add read paths (no UI changes yet)**

1. Create the new singleton tables and repository methods.
2. Add **read-only** `GET` endpoints for the singletons. Internally, populate their DTOs from the current source of truth (for now, likely from `HANDOVERS` fields or `HANDOVER_SECTIONS` until backfilled). &#x20;

**Phase 1 — backfill & dual-write**
3\. Run a one-time migration:

* For each `HANDOVER_ID`, write singletons into their new tables using the latest `HANDOVER_SECTIONS` row of each `SECTION_TYPE` (or the `HANDOVERS` fields where they exist). Use `UPDATED_AT` to pick winners.&#x20;

4. Update the **existing** `PUT /me/handovers/{handoverId}/sections/{sectionId}` handler to **dual-write** into the new singleton tables based on the section’s type (temporary).&#x20;

**Phase 2 — cut over the UI**
5\. Replace `updateHandoverSection(...)` with specific calls:

* `putSituationAwareness(handoverId, payload)`
* `putSynthesis(handoverId, payload)`
* `putPatientData(handoverId, payload)`
  Your current function hits `/sections/{sectionId}`; migrate to typed endpoints.&#x20;

6. Update React Query keys to be **resource-scoped** (e.g., `["handover", id, "situation-awareness"]`) rather than a generic sections cache. You already use clean keys for contingency/action items; mirror that.&#x20;

**Phase 3 — retire the old**
7\. Stop writing to `HANDOVER_SECTIONS`. Keep it readable for a short deprecation window.
8\. Remove the `/sections/{sectionId}` endpoint when the UI deploy is at 100%.

---

# Frontend changes (quick hits)

* **Replace `getSectionByType`** + the generic “sections” array with dedicated hooks:

  * `useSituationAwareness(handoverId)`, `usePutSituationAwareness(...)`,
  * `usePatientData(...)`, `usePutPatientData(...)`, etc.
    This eliminates runtime guessing by `sectionType`.&#x20;
* Keep your **collection patterns** as-is (they’re already clean and plural): action items, contingency plans, checklists, messages. &#x20;
* Bonus: add Zod (or similar) per-endpoint schemas to match the new explicit contracts.

---

# Backend refactor touches (small, focused)

* **Repository/Service split:**
  Introduce `Get/UpsertSituationAwarenessAsync`, `Get/UpsertSynthesisAsync`, `Get/UpsertPatientDataAsync` in `ISetupService` and repo. (You already have use cases forming—keep pushing there.)&#x20;
* **State guards for immutability:**
  Once a handover is `Completed`/`Expired`, block writes to singleton/collection tables (DB constraints or repository checks). You already model transitions (`/ready`, `/start`, `/accept`, `/complete`, etc.), so this is easy to enforce.&#x20;
* **De-dup source of truth:**
  Stop persisting singleton text on `HANDOVERS` after migration; keep only the foreign key relationship. Today, `HANDOVERS` still has `PATIENT_SUMMARY`/`SYNTHESIS`.&#x20;

---

# Why this is “THE BEST” fit for your domain

* **Predictable & consistent**
  Devs can guess endpoints from the domain: singletons are singular (`/situation-awareness`), lists are plural (`/action-items`). It matches your existing collection design (and your own doc’s guidance).
* **Robust & queryable**
  Typed columns per table + indices ⇒ fast “show me all Critical severities,” etc. The generic CLOB cannot do that reliably.&#x20;
* **Scalable for new sections**
  Add a new singleton (e.g., `handover_risks`) as a new table + `GET|PUT`—zero ripple effects on unrelated tables or endpoints.
* **Historically accurate**
  With state enforcement and `HANDOVER_ID` as the snapshot boundary, you keep exact past views while allowing new handovers to capture new truth. Your transition endpoints already carve out that lifecycle.&#x20;

---

# Minimal “before → after” map

| Today                                                 | After                                                                                        |
| ----------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| `PUT /me/handovers/{id}/sections/{sectionId}`         | `PUT /api/handovers/{id}/situation-awareness` (and `/patient-data`, `/synthesis`)            |
| `HANDOVER_SECTIONS(SECTION_TYPE, CONTENT CLOB, ...)`  | `HANDOVER_SITUATION_AWARENESS(...)`, `HANDOVER_PATIENT_DATA(...)`, `HANDOVER_SYNTHESIS(...)` |
| `HANDOVERS` stores `PATIENT_SUMMARY`, `SYNTHESIS` too | Remove content columns from `HANDOVERS`, rely on FK to singleton tables                      |
| Frontend infers by `sectionType` in an array          | Frontend calls specific hooks per singleton                                                  |

---

# Suggested next tickets

1. **DDL & repo:** Create singleton tables + repo methods; add GETs (read-only).
2. **Backfill:** Migrate data from `HANDOVER_SECTIONS`/`HANDOVERS` into new tables; add dual-write in `/sections` handler. &#x20;
3. **UI cutover:** Replace `updateHandoverSection` and `getSectionByType` flows with typed endpoints/hooks. &#x20;
4. **Lock writes post-completion** and remove old columns + `/sections` once traffic is off.&#x20;
