-- ========================================
-- ADD ADDITIONAL INDEXES FOR PERFORMANCE
-- ========================================
-- This script adds indexes on timestamp columns used for state-based queries
-- These indexes improve performance for queries filtering by handover state

-- Connect as RELEVO_APP user
CONNECT RELEVO_APP/TuPass123;

-- ========================================
-- TIMESTAMP INDEXES
-- ========================================
-- Add indexes on all timestamp columns used in state determination

-- Index for querying handovers by Ready state
CREATE INDEX IDX_HANDOVERS_READY_AT ON HANDOVERS(READY_AT);

-- Index for querying handovers by Started/InProgress state
CREATE INDEX IDX_HANDOVERS_STARTED_AT ON HANDOVERS(STARTED_AT);

-- Index for querying handovers by Accepted state
CREATE INDEX IDX_HANDOVERS_ACCEPTED_AT ON HANDOVERS(ACCEPTED_AT);

-- Index for querying handovers by Cancelled state
CREATE INDEX IDX_HANDOVERS_CANCELLED_AT ON HANDOVERS(CANCELLED_AT);

-- Index for querying handovers by Rejected state
CREATE INDEX IDX_HANDOVERS_REJECTED_AT ON HANDOVERS(REJECTED_AT);

-- Index for querying handovers by Expired state
CREATE INDEX IDX_HANDOVERS_EXPIRED_AT ON HANDOVERS(EXPIRED_AT);

-- Note: IDX_HANDOVERS_COMPLETED_AT already exists in 02-indexes.sql

-- ========================================
-- COMPOSITE INDEXES FOR COMMON QUERIES
-- ========================================

-- Composite index for finding active handovers by patient and date
-- Active = not in any terminal state (Completed, Cancelled, Rejected, Expired)
-- Note: Oracle 11g doesn't support partial indexes with WHERE clause
-- Instead, we create a function-based index that evaluates to NULL for terminal states
CREATE INDEX IDX_HANDOVERS_ACTIVE_PATIENT ON HANDOVERS(
  PATIENT_ID, 
  HANDOVER_WINDOW_DATE,
  CASE
    WHEN COMPLETED_AT IS NULL
     AND CANCELLED_AT IS NULL
     AND REJECTED_AT IS NULL
     AND EXPIRED_AT IS NULL
    THEN 1
    ELSE NULL
  END
);

-- Composite index for finding handovers by doctor and state
CREATE INDEX IDX_HANDOVERS_FROM_DOCTOR ON HANDOVERS(FROM_DOCTOR_ID, CREATED_AT);
CREATE INDEX IDX_HANDOVERS_TO_DOCTOR ON HANDOVERS(TO_DOCTOR_ID, CREATED_AT);

-- ========================================
-- VERIFICATION
-- ========================================
-- Verify indexes were created successfully
SELECT INDEX_NAME, TABLE_NAME, COLUMN_NAME, COLUMN_POSITION
FROM USER_IND_COLUMNS
WHERE TABLE_NAME = 'HANDOVERS'
  AND INDEX_NAME LIKE 'IDX_HANDOVERS_%'
ORDER BY INDEX_NAME, COLUMN_POSITION;

