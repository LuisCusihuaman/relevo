# RELEVO System: Database Documentation [v2]

This document provides a comprehensive overview of the Oracle 11g R2 database schema for the RELEVO system. It has been updated to align with the detailed UX flows and Backend Architecture specifications.

The schema is designed to support all core functionalities, including patient management, clinical handovers (I-PASS), real-time collaboration, Role-Based Access Control (RBAC), a persistent notification system, and comprehensive auditing for HIPAA compliance.

## 1\. Data Relationship Diagram (DRD)

The following diagram has been updated to reflect that chat messages are now directly related to a patient.

```
erDiagram
    USERS {
        VARCHAR2(255) UserId PK "Clerk User ID"
        VARCHAR2(10) Theme
        CHAR(1) NotificationsEnabled
    }

    PATIENTS {
        VARCHAR2(255) PatientId PK
        VARCHAR2(255) MRN "Medical Record Number"
        VARCHAR2(255) FullName
        DATE DOB "Date of Birth"
        VARCHAR2(50) Gender
        TIMESTAMP AdmissionDate
        VARCHAR2(255) CurrentUnitId FK
        VARCHAR2(50) RoomNumber
        VARCHAR2(1000) PrimaryDiagnosis
        CLOB Allergies "JSON Array"
        CLOB Medications "JSON Array"
    }

    UNITS {
        VARCHAR2(255) UnitId PK
        VARCHAR2(255) Name
    }

    SHIFTS {
        VARCHAR2(255) ShiftId PK
        VARCHAR2(255) Name
        VARCHAR2(5) StartTime "HH24:MI"
        VARCHAR2(5) EndTime "HH24:MI"
    }

    ASSIGNMENTS {
        VARCHAR2(255) AssignmentId PK
        VARCHAR2(255) UserId FK
        VARCHAR2(255) PatientId FK
        VARCHAR2(255) ShiftId FK
        DATE AssignmentDate
    }

    HANDOVERS {
        VARCHAR2(255) HandoverId PK
        VARCHAR2(255) PatientId FK
        VARCHAR2(255) InitiatingUserId FK
        VARCHAR2(255) SituationAwarenessDocId
        VARCHAR2(20) Status "InProgress, Completed"
        VARCHAR2(20) IllnessSeverity "Stable, Watcher, Unstable"
        CLOB PatientSummary
        CLOB SynthesisByReceiver
        TIMESTAMP CreatedAt
        TIMESTAMP CompletedAt
    }

    ACTION_ITEMS {
        VARCHAR2(255) ActionItemId PK
        VARCHAR2(255) HandoverId FK
        VARCHAR2(4000) Description
        CHAR(1) IsCompleted "Y/N"
        TIMESTAMP CreatedAt
    }

    COLLAB_DOCUMENTS {
        VARCHAR2(255) DocumentId PK "e.g., HandoverId-sa"
        VARCHAR2(255) HandoverId FK
        CLOB Content "Prosemirror JSON or similar"
        TIMESTAMP LastUpdatedAt
    }

    CHAT_MESSAGES {
        VARCHAR2(255) MessageId PK
        VARCHAR2(255) PatientId FK
        VARCHAR2(255) UserId FK
        VARCHAR2(255) UserName
        CLOB Content
        TIMESTAMP Timestamp
    }

    NOTIFICATIONS {
        VARCHAR2(255) NotificationId PK
        VARCHAR2(255) UserId FK
        VARCHAR2(255) HandoverId FK "Optional"
        VARCHAR2(100) Title
        VARCHAR2(4000) Message
        CHAR(1) IsRead "Y/N"
        TIMESTAMP CreatedAt
    }

    AUDIT_LOGS {
        NUMBER AuditLogId PK
        VARCHAR2(255) UserId "Performer"
        VARCHAR2(100) ActionType "e.g., PATIENT_UPDATE"
        VARCHAR2(255) EntityId "ID of the affected entity"
        VARCHAR2(100) EntityType "e.g., PATIENTS"
        CLOB OldValue
        CLOB NewValue
        TIMESTAMP Timestamp
    }

    HUMAN_READABLE_HISTORY {
        NUMBER HistoryId PK
        VARCHAR2(255) HandoverId FK
        VARCHAR2(255) UserId FK
        VARCHAR2(255) UserName
        VARCHAR2(255) Action
        CLOB Details
        TIMESTAMP Timestamp
    }

    USERS ||--o{ ASSIGNMENTS : "has"
    PATIENTS ||--o{ ASSIGNMENTS : "has"
    SHIFTS ||--o{ ASSIGNMENTS : "has"
    UNITS ||--o{ PATIENTS : "houses"
    PATIENTS ||--o{ HANDOVERS : "has"
    PATIENTS ||--o{ CHAT_MESSAGES : "has"
    USERS ||--o{ HANDOVERS : "initiates"
    HANDOVERS ||--o{ ACTION_ITEMS : "contains"
    HANDOVERS ||--o{ COLLAB_DOCUMENTS : "has"
    USERS ||--o{ CHAT_MESSAGES : "sends"
    USERS ||--o{ AUDIT_LOGS : "performs"
    USERS ||--o{ NOTIFICATIONS : "receives"
    HANDOVERS ||--o{ NOTIFICATIONS : "can trigger"
    HANDOVERS ||--o{ HUMAN_READABLE_HISTORY : "has"
    USERS ||--o{ HUMAN_READABLE_HISTORY : "generates"

```

-----

## 2\. Design Rationale and Key Concepts

This section explains the core principles behind the database design, now updated with considerations from the UX and Backend Architecture documents.

### 2.1. Relationships and Normalization

The schema remains in Third Normal Form (3NF).

  - **Role-Based Access Control (RBAC)**: To support the RBAC mentioned in the backend architecture, user roles are managed externally by Clerk and passed to the backend via JWT custom claims. The C# backend is responsible for interpreting these claims to enforce permissions. This decouples role management from the application's primary database.
  - **Junction Tables**: The `ASSIGNMENTS` table remains the key to the many-to-many relationship between `USERS` and `PATIENTS`.

### 2.2. Indexing Strategy and Search

The indexing strategy has been expanded to support the global `CommandPalette` search.

  - **Full-Text Search**: To power the `CommandPalette` search for both patients and action items, **Oracle Text** context indexes are defined.

    ```sql
    -- Index for searching patient name and diagnosis
    CREATE INDEX idx_patients_search ON PATIENTS(FullName, PrimaryDiagnosis)
    INDEXTYPE IS CTXSYS.CONTEXT;

    -- Index for searching action item descriptions
    CREATE INDEX idx_actionitems_search ON ACTION_ITEMS(Description)
    INDEXTYPE IS CTXSYS.CONTEXT;
    ```

    The C\# backend can now use the `CONTAINS` operator on both `PATIENTS` and `ACTION_ITEMS` tables to provide comprehensive search results as described in the API specification.

### 2.3. Auditing for HIPAA Compliance

The `AUDIT_LOGS` table is the cornerstone of the compliance strategy. To ensure a highly performant user interface, a background process transforms these raw logs into user-friendly text and stores them in the `HUMAN_READABLE_HISTORY` table for fast retrieval. Log entries are generated by a dedicated **`AuditService`** in the C# application layer, which provides rich, context-aware data for every change. This application-level approach is more flexible than database triggers, as it has access to user identity and other contextual information from the application itself.

### 2.4. Real-time Features and Data Persistence

  - **Notifications**: The UX documentation specifies a `NotificationsView`. To support this with persistence, a `NOTIFICATIONS` table has been added. While the NestJS service pushes alerts in real-time, this table stores a history of notifications for a user, allowing them to review past alerts.
  - **Collaborative Content**: The `COLLAB_DOCUMENTS` table is the designated persistence layer for the Hocuspocus-managed `Situation Awareness` content.
  - **Patient-Scoped Chat**: To provide a continuous and contextually relevant communication channel, `CHAT_MESSAGES` are linked directly to a `PatientId`. This creates a single, persistent discussion thread for each patient throughout their stay.

### 2.5. Concurrency and Data Integrity

  - **Patient Assignment Atomicity**: A unique constraint is established on the `ASSIGNMENTS` table across `(PatientId, ShiftId, AssignmentDate)`. This is a critical feature to prevent a race condition where two clinicians might attempt to assign the same patient simultaneously during the "Daily Setup" flow. This constraint guarantees at the database level that a patient can only be assigned to one clinician per shift, ensuring data integrity.

## 3\. Database Security

  - **Authentication**: Handled by Clerk.
  - **Authorization**: User roles are provided in the JWT from Clerk. The C# backend uses these roles to implement fine-grained authorization logic.
  - **Data Encryption at Rest**: Oracle Transparent Data Encryption (TDE) remains a critical security requirement.

## 4\. Oracle 11g R2 Database Schema

This section details the `CREATE TABLE` statements, now including the new `ROLES` and `NOTIFICATIONS` tables.

### **Table: `USERS`**

Stores user profile information. Roles are managed externally.

```sql
CREATE TABLE USERS (
    UserId VARCHAR2(255) NOT NULL,
    Theme VARCHAR2(10) DEFAULT 'dark' NOT NULL,
    NotificationsEnabled CHAR(1) DEFAULT 'Y' NOT NULL,
    -- CONSTRAINTS
    CONSTRAINT pk_users PRIMARY KEY (UserId),
    CONSTRAINT chk_users_theme CHECK (Theme IN ('light', 'dark')),
    CONSTRAINT chk_users_notif CHECK (NotificationsEnabled IN ('Y', 'N'))
);

COMMENT ON TABLE USERS IS 'Stores user profile information and preferences, linked to their Clerk ID. Roles are managed in Clerk and passed via JWT claims.';
COMMENT ON COLUMN USERS.UserId IS 'Primary Key. The unique identifier provided by the Clerk authentication service.';
```

### **Table: `UNITS`**

```sql
CREATE TABLE UNITS (
    UnitId VARCHAR2(255) NOT NULL,
    Name VARCHAR2(255) NOT NULL,
    -- CONSTRAINTS
    CONSTRAINT pk_units PRIMARY KEY (UnitId)
);
COMMENT ON TABLE UNITS IS 'Lookup table for hospital units (e.g., PICU, NICU).';
```

### **Table: `SHIFTS`**

```sql
CREATE TABLE SHIFTS (
    ShiftId VARCHAR2(255) NOT NULL,
    Name VARCHAR2(255) NOT NULL,
    StartTime VARCHAR2(5) NOT NULL, -- Stored in HH24:MI format
    EndTime VARCHAR2(5) NOT NULL,   -- Stored in HH24:MI format
    -- CONSTRAINTS
    CONSTRAINT pk_shifts PRIMARY KEY (ShiftId)
);
COMMENT ON TABLE SHIFTS IS 'Lookup table for available work shifts (e.g., Day Shift, Night Shift).';
```

### **Table: `PATIENTS`**

```sql
CREATE TABLE PATIENTS (
    PatientId VARCHAR2(255) NOT NULL,
    MRN VARCHAR2(255) NOT NULL,
    FullName VARCHAR2(255) NOT NULL,
    DOB DATE,
    Gender VARCHAR2(50),
    AdmissionDate TIMESTAMP,
    CurrentUnitId VARCHAR2(255),
    RoomNumber VARCHAR2(50),
    PrimaryDiagnosis VARCHAR2(1000),
    Allergies CLOB, -- Stored as a JSON array of strings
    Medications CLOB, -- Stored as a JSON array of strings
    -- CONSTRAINTS
    CONSTRAINT pk_patients PRIMARY KEY (PatientId),
    CONSTRAINT uq_patients_mrn UNIQUE (MRN),
    CONSTRAINT fk_patients_unit FOREIGN KEY (CurrentUnitId) REFERENCES UNITS(UnitId),
    CONSTRAINT chk_patient_gender CHECK (Gender IN ('Male', 'Female', 'Other', 'Unknown')),
    CONSTRAINT chk_allergies_json CHECK (Allergies IS JSON),
    CONSTRAINT chk_medications_json CHECK (Medications IS JSON)
);

CREATE INDEX idx_patients_fullname ON PATIENTS(FullName);
CREATE INDEX idx_patients_unitid ON PATIENTS(CurrentUnitId);

COMMENT ON TABLE PATIENTS IS 'Stores core demographic and clinical information for patients.';
COMMENT ON COLUMN PATIENTS.MRN IS 'Medical Record Number, unique for each patient.';
COMMENT ON COLUMN PATIENTS.Allergies IS 'Stores a JSON array of known allergies, e.g., ["Penicillin", "Peanuts"].';
COMMENT ON COLUMN PATIENTS.Medications IS 'Stores a JSON array of current medications, e.g., ["Albuterol", "Prednisone"].';
```

### **Table: `ASSIGNMENTS`**

```sql
CREATE TABLE ASSIGNMENTS (
    AssignmentId VARCHAR2(255) NOT NULL,
    UserId VARCHAR2(255) NOT NULL,
    PatientId VARCHAR2(255) NOT NULL,
    ShiftId VARCHAR2(255) NOT NULL,
    AssignmentDate DATE DEFAULT SYSDATE NOT NULL,
    -- CONSTRAINTS
    CONSTRAINT pk_assignments PRIMARY KEY (AssignmentId),
    CONSTRAINT fk_assignments_user FOREIGN KEY (UserId) REFERENCES USERS(UserId),
    CONSTRAINT fk_assignments_patient FOREIGN KEY (PatientId) REFERENCES PATIENTS(PatientId),
    CONSTRAINT fk_assignments_shift FOREIGN KEY (ShiftId) REFERENCES SHIFTS(ShiftId),
    CONSTRAINT uq_assignments_patient_shift_date UNIQUE (PatientId, ShiftId, AssignmentDate)
);

CREATE INDEX idx_assignments_userid ON ASSIGNMENTS(UserId);
CREATE INDEX idx_assignments_patientid ON ASSIGNMENTS(PatientId);

COMMENT ON TABLE ASSIGNMENTS IS 'Links a clinician (user) to patients for a specific shift and date.';
COMMENT ON CONSTRAINT uq_assignments_patient_shift_date ON ASSIGNMENTS IS 'Ensures a patient can only be assigned to one user per shift on a given day to prevent race conditions.';
```

### **Table: `HANDOVERS`**

```sql
CREATE TABLE HANDOVERS (
    HandoverId VARCHAR2(255) NOT NULL,
    PatientId VARCHAR2(255) NOT NULL,
    InitiatingUserId VARCHAR2(255) NOT NULL,
    SituationAwarenessDocId VARCHAR2(255) NOT NULL,
    Status VARCHAR2(20) DEFAULT 'InProgress' NOT NULL,
    IllnessSeverity VARCHAR2(20) DEFAULT 'Stable' NOT NULL,
    PatientSummary CLOB,
    SynthesisByReceiver CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    CompletedAt TIMESTAMP,
    -- CONSTRAINTS
    CONSTRAINT pk_handovers PRIMARY KEY (HandoverId),
    CONSTRAINT fk_handovers_patient FOREIGN KEY (PatientId) REFERENCES PATIENTS(PatientId),
    CONSTRAINT fk_handovers_user FOREIGN KEY (InitiatingUserId) REFERENCES USERS(UserId),
    CONSTRAINT chk_handovers_status CHECK (Status IN ('InProgress', 'Completed')),
    CONSTRAINT chk_handovers_severity CHECK (IllnessSeverity IN ('Stable', 'Watcher', 'Unstable'))
);

CREATE INDEX idx_handovers_patientid ON HANDOVERS(PatientId);
CREATE INDEX idx_handovers_status ON HANDOVERS(Status);

COMMENT ON TABLE HANDOVERS IS 'Central table for managing the state of an I-PASS handover session.';
COMMENT ON COLUMN HANDOVERS.SituationAwarenessDocId IS 'The document ID for the collaborative Situation Awareness document, linking to COLLAB_DOCUMENTS.DocumentId. An explicit FK constraint is omitted to prevent circular dependencies at creation time.';
```

### **Table: `ACTION_ITEMS`**

```sql
CREATE TABLE ACTION_ITEMS (
    ActionItemId VARCHAR2(255) NOT NULL,
    HandoverId VARCHAR2(255) NOT NULL,
    Description VARCHAR2(4000) NOT NULL,
    IsCompleted CHAR(1) DEFAULT 'N' NOT NULL,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    -- CONSTRAINTS
    CONSTRAINT pk_action_items PRIMARY KEY (ActionItemId),
    CONSTRAINT fk_actionitems_handover FOREIGN KEY (HandoverId) REFERENCES HANDOVERS(HandoverId) ON DELETE CASCADE,
    CONSTRAINT chk_actionitems_completed CHECK (IsCompleted IN ('Y', 'N'))
);

CREATE INDEX idx_actionitems_handoverid ON ACTION_ITEMS(HandoverId);

COMMENT ON TABLE ACTION_ITEMS IS 'Stores individual tasks within a handover''s shared Action List.';
```

### **Table: `COLLAB_DOCUMENTS`**

```sql
CREATE TABLE COLLAB_DOCUMENTS (
    DocumentId VARCHAR2(255) NOT NULL,
    HandoverId VARCHAR2(255) NOT NULL,
    Content CLOB, -- Stores Prosemirror JSON state
    LastUpdatedAt TIMESTAMP,
    -- CONSTRAINTS
    CONSTRAINT pk_collab_documents PRIMARY KEY (DocumentId),
    CONSTRAINT fk_collabdocs_handover FOREIGN KEY (HandoverId) REFERENCES HANDOVERS(HandoverId) ON DELETE CASCADE,
    CONSTRAINT chk_collab_content_json CHECK (Content IS JSON)
);

COMMENT ON TABLE COLLAB_DOCUMENTS IS 'Persists the state of collaborative documents (e.g., Situation Awareness).';
```

### **Table: `CHAT_MESSAGES`**

This table stores the persistent chat log for each patient.

```sql
CREATE TABLE CHAT_MESSAGES (
    MessageId VARCHAR2(255) NOT NULL,
    PatientId VARCHAR2(255) NOT NULL,
    UserId VARCHAR2(255) NOT NULL,
    UserName VARCHAR2(255), -- Denormalized for quick display
    Content CLOB NOT NULL,
    Timestamp TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    -- CONSTRAINTS
    CONSTRAINT pk_chat_messages PRIMARY KEY (MessageId),
    CONSTRAINT fk_chat_patient FOREIGN KEY (PatientId) REFERENCES PATIENTS(PatientId) ON DELETE CASCADE,
    CONSTRAINT fk_chat_user FOREIGN KEY (UserId) REFERENCES USERS(UserId)
);

CREATE INDEX idx_chat_patientid ON CHAT_MESSAGES(PatientId);

COMMENT ON TABLE CHAT_MESSAGES IS 'Stores the persistent chat message history for each patient. It uses a hybrid model, storing both UserId (for permanent reference) and a denormalized UserName (for performance and historical accuracy).';
COMMENT ON COLUMN CHAT_MESSAGES.PatientId IS 'Links the message to the relevant patient, creating a patient-specific discussion thread.';
COMMENT ON COLUMN CHAT_MESSAGES.UserName IS 'Denormalized for performance. Stores the user''s name at the time the message was sent to ensure historical accuracy.';
```

### **Table: `NOTIFICATIONS`**

A new table to persist user notifications for historical review.

```sql
CREATE TABLE NOTIFICATIONS (
    NotificationId VARCHAR2(255) NOT NULL,
    UserId VARCHAR2(255) NOT NULL,
    HandoverId VARCHAR2(255), -- Optional, for context
    Title VARCHAR2(100) NOT NULL,
    Message VARCHAR2(4000) NOT NULL,
    IsRead CHAR(1) DEFAULT 'N' NOT NULL,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    -- CONSTRAINTS
    CONSTRAINT pk_notifications PRIMARY KEY (NotificationId),
    CONSTRAINT fk_notifications_user FOREIGN KEY (UserId) REFERENCES USERS(UserId) ON DELETE CASCADE,
    CONSTRAINT fk_notifications_handover FOREIGN KEY (HandoverId) REFERENCES HANDOVERS(HandoverId) ON DELETE SET NULL,
    CONSTRAINT chk_notifications_read CHECK (IsRead IN ('Y', 'N'))
);

CREATE INDEX idx_notifications_userid ON NOTIFICATIONS(UserId);

COMMENT ON TABLE NOTIFICATIONS IS 'Stores persistent notifications for users, supporting the NotificationsView feature.';
COMMENT ON COLUMN NOTIFICATIONS.HandoverId IS 'Optional foreign key to link a notification to a specific handover.';
COMMENT ON COLUMN NOTIFICATIONS.IsRead IS 'Flag (Y/N) to track if the user has viewed the notification.';
```

### **Table: `AUDIT_LOGS`**

```sql
-- For Oracle 11g R2, we use a SEQUENCE to generate primary keys instead of an IDENTITY column.
CREATE SEQUENCE audit_logs_seq
  START WITH 1
  INCREMENT BY 1
  NOCACHE
  NOCYCLE;

CREATE TABLE AUDIT_LOGS (
    AuditLogId NUMBER NOT NULL,
    UserId VARCHAR2(255),
    ActionType VARCHAR2(100) NOT NULL,
    EntityType VARCHAR2(100) NOT NULL,
    EntityId VARCHAR2(255) NOT NULL,
    OldValue CLOB, -- Stores previous state as JSON
    NewValue CLOB, -- Stores new state as JSON
    Timestamp TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    -- CONSTRAINTS
    CONSTRAINT pk_audit_logs PRIMARY KEY (AuditLogId),
    CONSTRAINT fk_audit_user FOREIGN KEY (UserId) REFERENCES USERS(UserId),
    CONSTRAINT chk_oldvalue_json CHECK (OldValue IS JSON),
    CONSTRAINT chk_newvalue_json CHECK (NewValue IS JSON)
);

-- A trigger is used to automatically populate the primary key from the sequence.
CREATE OR REPLACE TRIGGER trg_audit_logs_pk
BEFORE INSERT ON AUDIT_LOGS
FOR EACH ROW
BEGIN
  SELECT audit_logs_seq.NEXTVAL
  INTO   :new.AuditLogId
  FROM   dual;
END;
/

COMMENT ON TRIGGER trg_audit_logs_pk IS 'Populates the primary key from a sequence. The core audit data (OldValue, NewValue) is populated by the application layer.';

CREATE INDEX idx_audit_entity ON AUDIT_LOGS(EntityType, EntityId);
CREATE INDEX idx_audit_userid ON AUDIT_LOGS(UserId);

COMMENT ON TABLE AUDIT_LOGS IS 'Immutable log of all data changes for compliance and auditing. Populated by the application-layer AuditService.';
```

### **Table: `HUMAN_READABLE_HISTORY`**

```sql
-- For Oracle 11g R2, we use a SEQUENCE to generate primary keys instead of an IDENTITY column.
CREATE SEQUENCE human_readable_history_seq
  START WITH 1
  INCREMENT BY 1
  NOCACHE
  NOCYCLE;

CREATE TABLE HUMAN_READABLE_HISTORY (
    HistoryId NUMBER NOT NULL,
    HandoverId VARCHAR2(255) NOT NULL,
    UserId VARCHAR2(255) NOT NULL,
    UserName VARCHAR2(255),
    Action VARCHAR2(255) NOT NULL,
    Details CLOB,
    Timestamp TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    -- CONSTRAINTS
    CONSTRAINT pk_human_readable_history PRIMARY KEY (HistoryId),
    CONSTRAINT fk_history_handover FOREIGN KEY (HandoverId) REFERENCES HANDOVERS(HandoverId) ON DELETE CASCADE,
    CONSTRAINT fk_history_user FOREIGN KEY (UserId) REFERENCES USERS(UserId)
);

CREATE INDEX idx_history_handoverid ON HUMAN_READABLE_HISTORY(HandoverId);
CREATE INDEX idx_history_userid ON HUMAN_READABLE_HISTORY(UserId);

-- A trigger is used to automatically populate the primary key from the sequence.
CREATE OR REPLACE TRIGGER trg_human_readable_history_pk
BEFORE INSERT ON HUMAN_READABLE_HISTORY
FOR EACH ROW
BEGIN
  SELECT human_readable_history_seq.NEXTVAL
  INTO   :new.HistoryId
  FROM   dual;
END;
/

COMMENT ON TABLE HUMAN_READABLE_HISTORY IS 'Stores pre-formatted, human-readable audit log entries for fast retrieval by the UI. It is populated by a background job and uses a hybrid data model, storing both UserId and the point-in-time UserName for historical accuracy and performance.';
COMMENT ON COLUMN HUMAN_READABLE_HISTORY.UserName IS 'Denormalized for performance. Stores the user''s name at the time the action was performed to ensure the audit trail is historically accurate.';
```