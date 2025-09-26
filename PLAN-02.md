# 0) One-shot reset

Because nothing’s deployed and containers are wiped, you can **recreate**. Suggested order:

1. Drop old objects that will conflict (table/columns/indexes).
2. Create new tables (normalized singletons).
3. Recreate views & indexes.
4. Seed with the new model.

---

# 1) DDL — normalized singletons, no more “sections” blob

### 1.1 Drop legacy bits

```sql
-- If present in your dev DB:
DROP TABLE HANDOVER_SECTIONS CASCADE CONSTRAINTS;
ALTER TABLE HANDOVERS DROP COLUMN ILLNESS_SEVERITY;
ALTER TABLE HANDOVERS DROP COLUMN PATIENT_SUMMARY;
ALTER TABLE HANDOVERS DROP COLUMN SITUATION_AWARENESS_DOC_ID;
ALTER TABLE HANDOVERS DROP COLUMN SYNTHESIS;
-- Drop any indexes specifically on HANDOVER_SECTIONS
```

(We’re removing the generic table you created here and seeded here.) &#x20;

### 1.2 Add singleton tables (one per domain object)

```sql
-- Patient Data (Illness Severity + Patient Summary)
CREATE TABLE HANDOVER_PATIENT_DATA (
  HANDOVER_ID        VARCHAR2(50) PRIMARY KEY
    REFERENCES HANDOVERS(ID),
  ILLNESS_SEVERITY   VARCHAR2(20) NOT NULL,
  SUMMARY_TEXT       CLOB,
  LAST_EDITED_BY     VARCHAR2(255) REFERENCES USERS(ID),
  STATUS             VARCHAR2(20) DEFAULT 'draft', -- draft, in_progress, completed
  CREATED_AT         TIMESTAMP DEFAULT SYSTIMESTAMP,
  UPDATED_AT         TIMESTAMP DEFAULT SYSTIMESTAMP,
  CONSTRAINT CHK_HP_ILLNESS_SEVERITY
    CHECK (ILLNESS_SEVERITY IN ('Stable','Watcher','Unstable','Critical'))
);

-- Situation Awareness
CREATE TABLE HANDOVER_SITUATION_AWARENESS (
  HANDOVER_ID     VARCHAR2(50) PRIMARY KEY
    REFERENCES HANDOVERS(ID),
  CONTENT         CLOB,
  LAST_EDITED_BY  VARCHAR2(255) REFERENCES USERS(ID),
  STATUS          VARCHAR2(20) DEFAULT 'draft', -- draft, in_progress, completed
  CREATED_AT      TIMESTAMP DEFAULT SYSTIMESTAMP,
  UPDATED_AT      TIMESTAMP DEFAULT SYSTIMESTAMP
);

-- Synthesis
CREATE TABLE HANDOVER_SYNTHESIS (
  HANDOVER_ID     VARCHAR2(50) PRIMARY KEY
    REFERENCES HANDOVERS(ID),
  CONTENT         CLOB,
  LAST_EDITED_BY  VARCHAR2(255) REFERENCES USERS(ID),
  STATUS          VARCHAR2(20) DEFAULT 'draft', -- draft, in_progress, completed
  CREATED_AT      TIMESTAMP DEFAULT SYSTIMESTAMP,
  UPDATED_AT      TIMESTAMP DEFAULT SYSTIMESTAMP
);
```

### 1.3 Keep existing collections as-is

All your list-like tables (Action Items, Contingency, Messages, Participants, etc.) **stay**. They’re already in good shape and well-indexed in your scripts (e.g., `HANDOVER_CONTINGENCY` seeds and indexes).&#x20;

### 1.4 View remains valid

Your `VW_HANDOVERS_STATE` computes state from timestamps; it’s independent of sections and still fine.

---

# 2) Indexes (focused & minimal)

```sql
-- Singletons
CREATE INDEX IDX_HP_LAST_EDITED_BY ON HANDOVER_PATIENT_DATA(LAST_EDITED_BY);
CREATE INDEX IDX_HSA_LAST_EDITED_BY ON HANDOVER_SITUATION_AWARENESS(LAST_EDITED_BY);
CREATE INDEX IDX_HSYN_LAST_EDITED_BY ON HANDOVER_SYNTHESIS(LAST_EDITED_BY);
CREATE INDEX IDX_HP_STATUS ON HANDOVER_PATIENT_DATA(STATUS);
CREATE INDEX IDX_HSA_STATUS ON HANDOVER_SITUATION_AWARENESS(STATUS);
CREATE INDEX IDX_HSYN_STATUS ON HANDOVER_SYNTHESIS(STATUS);

-- You already have strong indexes on handovers/collections (keep them). :contentReference[oaicite:7]{index=7}
```

---

# 3) Seed data (new model)

Anywhere you previously seeded the 4 fields directly on `HANDOVERS` (illness, summary, awareness\_doc\_id, synthesis) or inserted rows into `HANDOVER_SECTIONS` (patient summary, situation awareness, synthesis), insert into the **new tables** instead. For example, replacing this seed block that used `HANDOVER_SECTIONS` for multiple types :

```sql
-- For handover-001
INSERT INTO HANDOVER_PATIENT_DATA(HANDOVER_ID, ILLNESS_SEVERITY, SUMMARY_TEXT, STATUS, LAST_EDITED_BY)
VALUES ('handover-001', 'Stable',
'Paciente de 14 años con neumonía adquirida en comunidad. Ingreso hace 3 días. Tratamiento con Amoxicilina y oxígeno suplementario.',
'completed', 'user_demo12345678901234567890123456');

INSERT INTO HANDOVER_SITUATION_AWARENESS(HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY)
VALUES ('handover-001',
'Paciente estable, sin complicaciones. Buena respuesta al tratamiento antibiótico.',
'completed', 'user_demo12345678901234567890123456');

INSERT INTO HANDOVER_SYNTHESIS(HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY)
VALUES ('handover-001',
'Continuar tratamiento actual. Alta probable en 48-72 horas si evolución favorable.',
'draft', 'user_demo12345678901234567890123456');
```

Collections (e.g., `HANDOVER_CONTINGENCY`) seed stays exactly as-is (you already have lots of good Spanish examples) .

---

# 4) API — explicit, typed contracts

## 4.1 Routes

* **Singletons** (create-or-replace via PUT, retrieve via GET):

  * `GET|PUT /api/handovers/{id}/patient-data`
  * `GET|PUT /api/handovers/{id}/situation-awareness`
  * `GET|PUT /api/handovers/{id}/synthesis`
* **Collections** (plural list + item routes; keep as you have them):

  * `GET|POST /api/handovers/{id}/action-items`
  * `GET|POST /api/handovers/{id}/contingency-plans`
  * `GET|POST /api/handovers/{id}/messages`
  * `GET|PUT|DELETE /api/handovers/{id}/action-items/{itemId}` …etc.

This removes `/sections/{sectionId}` and its dynamic payloads (your current pattern) .

## 4.2 DTOs (examples)

```json
// GET /api/handovers/{id}/patient-data
{
  "handoverId": "h-001",
  "illnessSeverity": "Stable",
  "summaryText": "…",
  "status": "completed",
  "lastEditedBy": "user_…",
  "updatedAt": "2025-09-25T12:34:56Z"
}

// PUT /api/handovers/{id}/patient-data (idempotent)
{
  "illnessSeverity": "Unstable",
  "summaryText": "…",
  "status": "in_progress"
}
```

Analogous DTOs for `/situation-awareness` and `/synthesis` with `content`, `status`, `lastEditedBy`.

## 4.3 Validation

* `illnessSeverity`: enum (`Stable|Watcher|Unstable|Critical`) — enforced by DB check + request schema.
* `status`: enum (`draft|in_progress|completed`) for all three singletons.
* Reject writes if handover is terminal (`Completed|Cancelled|Rejected|Expired`) — easy to enforce using your state timestamps (you already derive state in the view) in service or via a trigger.

---

# 5) Backend code touches (fast)

* Delete the generic “update section by sectionId” code path that infers `SECTION_TYPE` from string substrings (it disappears with the table) .
* Create 3 simple repository pairs:

  * `Get/UpsertHandoverPatientData(handoverId, dto)`
  * `Get/UpsertHandoverSituationAwareness(handoverId, dto)`
  * `Get/UpsertHandoverSynthesis(handoverId, dto)`
* Keep your sync status logic as-is (it already has a nice default behavior when missing rows) .
* Leave lifecycle endpoints (`/ready`, `/start`, `/accept`, `/complete`, etc.) intact; we’ll rely on them for immutability rules.

---

# 6) Frontend changes (small but decisive)

* Replace `updateHandoverSection(handoverId, sectionId, {content,status})` calls with three typed functions:

  * `putPatientData(handoverId, { illnessSeverity, summaryText, status })`
  * `putSituationAwareness(handoverId, { content, status })`
  * `putSynthesis(handoverId, { content, status })`
* Replace `getSectionByType(sections, type)` with dedicated queries:

  * `usePatientData(handoverId)`
  * `useSituationAwareness(handoverId)`
  * `useSynthesis(handoverId)`
* React Query keys: `["handover", id, "patient-data"]`, etc.

This removes the need for a “sections array” and section-type inference (what your code is doing today) .

---

# 7) Optional: state-driven immutability (DB-level)

If you want hard DB guarantees, add a trigger on each singleton/collection that prevents `INSERT/UPDATE/DELETE` when the parent handover is terminal (as computed in your state logic; same rules your view applies). Your `VW_HANDOVERS_STATE` already formalizes states from timestamps, so the business rule is unambiguous.

---

# 8) Quick checklist to “green”:

* [ ] Run the **DROP/ALTER** for legacy columns & `HANDOVER_SECTIONS`.
* [ ] Create the **three singleton** tables (+ indexes).
* [ ] Seed **singletons** from scratch (swap any old `HANDOVER_SECTIONS` seed lines for inserts into the new tables).
* [ ] Wire **GET/PUT** endpoints for each singleton.
* [ ] Replace FE calls and hooks to hit the new endpoints.
* [ ] (Optional) Add immutability guards.
