-- ========================================
-- CREATE INDEXES
-- ========================================

-- Connect as RELEVO_APP user
CONNECT RELEVO_APP/TuPass123;

-- ========================================
-- BASIC FOREIGN KEY INDEXES
-- ========================================
-- Essential for FK joins and to avoid full scans on parent table operations

CREATE INDEX IDX_PATIENTS_UNIT_ID ON PATIENTS(UNIT_ID);
CREATE INDEX IDX_USER_ASSIGNMENTS_USER ON USER_ASSIGNMENTS(USER_ID);
CREATE INDEX IDX_USER_ASSIGNMENTS_PATIENT ON USER_ASSIGNMENTS(PATIENT_ID);
CREATE INDEX IDX_USER_ASSIGNMENTS_SHIFT ON USER_ASSIGNMENTS(SHIFT_ID);

-- ========================================
-- HANDOVERS INDEXES
-- ========================================

-- Patient timeline: all handovers for a patient ordered by time
CREATE INDEX IX_HO_PAT_TIME ON HANDOVERS(PATIENT_ID, WINDOW_START_AT DESC);

-- Receiver inbox: handovers assigned to a user ordered by time
CREATE INDEX IX_HO_TO_USER_TIME ON HANDOVERS(TO_USER_ID, WINDOW_START_AT DESC);

-- Sender outbox: handovers sent by a user ordered by time
CREATE INDEX IX_HO_FROM_USER_TIME ON HANDOVERS(FROM_USER_ID, WINDOW_START_AT DESC);

-- Index for user inbox by state + time (OPTIONAL - only if filtering by state)
-- Uses virtual column CURRENT_STATE (simpler than function-based index)
-- Optimizes queries like "get all Ready handovers for user X"
-- Keep only if your UI has tabs/filters by state (Ready, InProgress, etc.)
CREATE INDEX IX_HO_TO_USER_STATE_TIME ON HANDOVERS(TO_USER_ID, CURRENT_STATE, WINDOW_START_AT DESC);

-- Index for sender's outbox by state + time (OPTIONAL - only if filtering by state)
-- Uses virtual column CURRENT_STATE (simpler than function-based index)
-- Optimizes queries like "get all my sent handovers by state"
-- Keep only if your UI has tabs/filters by state
CREATE INDEX IX_HO_FROM_USER_STATE_TIME ON HANDOVERS(FROM_USER_ID, CURRENT_STATE, WINDOW_START_AT DESC);

-- Unique index: only one active handover per patient/window
-- Active = not in any terminal state (Completed, Cancelled, Rejected, Expired)
-- Keep only if preventing duplicate active handovers is a core business rule
CREATE UNIQUE INDEX UQ_HO_ACTIVE_WINDOW ON HANDOVERS(
    PATIENT_ID,
    WINDOW_START_AT,
    FROM_SHIFT_ID,
    TO_SHIFT_ID,
    CASE
        WHEN COMPLETED_AT IS NULL
         AND CANCELLED_AT IS NULL
         AND REJECTED_AT  IS NULL
         AND EXPIRED_AT   IS NULL
        THEN 1 ELSE NULL
    END
);

-- OPTIONAL: Only if you have queries filtering by shift
-- CREATE INDEX IDX_HANDOVERS_FROM_SHIFT ON HANDOVERS(FROM_SHIFT_ID);
-- CREATE INDEX IDX_HANDOVERS_TO_SHIFT ON HANDOVERS(TO_SHIFT_ID);

-- ========================================
-- HANDOVER CHILD TABLES INDEXES
-- ========================================
-- Essential FK indexes for joins and to avoid full scans on parent operations

CREATE INDEX IDX_HACTION_HANDOVER_ID ON HANDOVER_ACTION_ITEMS(HANDOVER_ID);

-- Composite index covers both "participants by handover" and "active participants of a handover" queries
CREATE INDEX IX_PART_HO_STATUS ON HANDOVER_PARTICIPANTS(HANDOVER_ID, STATUS);
CREATE INDEX IDX_PARTICIPANTS_USER_ID ON HANDOVER_PARTICIPANTS(USER_ID);

-- Composite index covers both "contingencies by handover" and "contingencies by status for a handover" queries
CREATE INDEX IX_CONT_HO_STATUS ON HANDOVER_CONTINGENCY(HANDOVER_ID, STATUS);
CREATE INDEX IDX_CONTINGENCY_USER ON HANDOVER_CONTINGENCY(CREATED_BY);

-- Composite index for "messages of a handover ordered by time" (most common query pattern)
CREATE INDEX IX_MSG_HO_TIME ON HANDOVER_MESSAGES(HANDOVER_ID, CREATED_AT DESC);
CREATE INDEX IDX_MSG_USER ON HANDOVER_MESSAGES(USER_ID);
-- OPTIONAL: Only if you frequently filter messages by type
-- CREATE INDEX IDX_MSG_TYPE ON HANDOVER_MESSAGES(MESSAGE_TYPE);

CREATE INDEX IDX_MENTION_MESSAGE ON HANDOVER_MENTIONS(MESSAGE_ID);
-- OPTIONAL: Only if you have "notifications by mentions" queries per user
CREATE INDEX IDX_MENTION_USER ON HANDOVER_MENTIONS(MENTIONED_USER_ID);

CREATE INDEX IX_HAL_HO_TIME ON HANDOVER_ACTIVITY_LOG(HANDOVER_ID, CREATED_AT DESC);
CREATE INDEX IX_HAL_USER_TIME ON HANDOVER_ACTIVITY_LOG(USER_ID, CREATED_AT DESC);

-- ========================================
-- USERS INDEXES
-- ========================================

CREATE INDEX IDX_USERS_EMAIL ON USERS(EMAIL);
-- OPTIONAL: Only if you frequently query "all active doctors" etc.
-- Better as composite: CREATE INDEX IDX_USERS_ROLE_ACTIVE ON USERS(ROLE, IS_ACTIVE);
-- CREATE INDEX IDX_USERS_ROLE ON USERS(ROLE);
-- CREATE INDEX IDX_USERS_IS_ACTIVE ON USERS(IS_ACTIVE);

-- ========================================
-- HANDOVER_CONTENTS INDEXES
-- ========================================
-- OPTIONAL: Only if you have a feed of "recently edited handovers"
-- Since HANDOVER_CONTENTS is 1:1 with HANDOVERS (PK = HANDOVER_ID),
-- you usually access by HANDOVER_ID, so UPDATED_AT index may be redundant
-- CREATE INDEX IX_HC_UPDATED ON HANDOVER_CONTENTS(UPDATED_AT DESC);
