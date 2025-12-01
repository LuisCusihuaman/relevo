-- ========================================
-- CREATE INDEXES
-- ========================================
-- Schema V3: Indexes for shift instances + windows model

-- Connect as RELEVO_APP user
CONNECT RELEVO_APP/TuPass123;

-- ========================================
-- BASIC FOREIGN KEY INDEXES
-- ========================================
-- Essential for FK joins and to avoid full scans on parent table operations

-- Note: V3_PLAN.md doesn't explicitly list FK indexes, but they're standard practice
CREATE INDEX IDX_PATIENTS_UNIT_ID ON PATIENTS(UNIT_ID);

-- ========================================
-- SHIFT_WINDOWS INDEXES
-- ========================================
-- (From V3_PLAN.md section 2)

CREATE INDEX IX_SW_UNIT ON SHIFT_WINDOWS(UNIT_ID);
CREATE INDEX IX_SW_FROM ON SHIFT_WINDOWS(FROM_SHIFT_INSTANCE_ID);
CREATE INDEX IX_SW_TO ON SHIFT_WINDOWS(TO_SHIFT_INSTANCE_ID);

-- ========================================
-- SHIFT_COVERAGE INDEXES
-- ========================================
-- (From V3_PLAN.md section 3)

-- Un solo primary por paciente+shift_instance (sin triggers)
-- FIXED: Changed to use ID when IS_PRIMARY=0 to allow multiple non-primary coverages
-- When IS_PRIMARY=1: uses constant 'PRIMARY' (allows only one per patient+shift_instance)
-- When IS_PRIMARY=0: uses ID (unique per coverage, allows multiple)
CREATE UNIQUE INDEX UQ_SC_PRIMARY_ACTIVE ON SHIFT_COVERAGE (
    PATIENT_ID,
    SHIFT_INSTANCE_ID,
    CASE 
        WHEN IS_PRIMARY = 1 THEN 'PRIMARY'
        ELSE ID  -- Use coverage ID for non-primary, ensuring uniqueness
    END
);

CREATE INDEX IX_SC_USER_SI ON SHIFT_COVERAGE(RESPONSIBLE_USER_ID, SHIFT_INSTANCE_ID);
CREATE INDEX IX_SC_PAT_SI ON SHIFT_COVERAGE(PATIENT_ID, SHIFT_INSTANCE_ID);
CREATE INDEX IX_SC_SI_PAT ON SHIFT_COVERAGE(SHIFT_INSTANCE_ID, PATIENT_ID);
CREATE INDEX IX_SC_PRIMARY ON SHIFT_COVERAGE(PATIENT_ID, SHIFT_INSTANCE_ID, IS_PRIMARY);

-- ========================================
-- HANDOVERS INDEXES
-- ========================================
-- (From V3_PLAN.md section 4)

CREATE INDEX IX_HO_UNIT_STATE_TIME ON HANDOVERS(UNIT_ID, CURRENT_STATE, NVL(READY_AT, CREATED_AT) DESC);
CREATE INDEX IX_HO_SENDER ON HANDOVERS(SENDER_USER_ID);
CREATE INDEX IX_HO_COMPLETED_BY ON HANDOVERS(COMPLETED_BY_USER_ID);
CREATE INDEX IX_HO_STARTED_BY ON HANDOVERS(STARTED_BY_USER_ID);
