-- ========================================
-- ADD DATABASE CONSTRAINTS
-- ========================================
-- This script adds check constraints to enforce handover state machine integrity
-- Run this AFTER verifying existing data is clean (see migration queries below)

-- Connect as RELEVO_APP user
CONNECT RELEVO_APP/TuPass123;

-- ========================================
-- PRE-MIGRATION: Check for violations
-- ========================================
-- Run these queries BEFORE adding constraints to find and fix bad data

-- Check for completed handovers without accepted_at
-- SELECT ID, COMPLETED_AT, ACCEPTED_AT, STARTED_AT, READY_AT
-- FROM HANDOVERS
-- WHERE COMPLETED_AT IS NOT NULL AND ACCEPTED_AT IS NULL;

-- Check for accepted handovers without started_at
-- SELECT ID, ACCEPTED_AT, STARTED_AT, READY_AT
-- FROM HANDOVERS
-- WHERE ACCEPTED_AT IS NOT NULL AND STARTED_AT IS NULL;

-- Check for started handovers without ready_at
-- SELECT ID, STARTED_AT, READY_AT
-- FROM HANDOVERS
-- WHERE STARTED_AT IS NOT NULL AND READY_AT IS NULL;

-- Check for multiple terminal states
-- SELECT ID, COMPLETED_AT, CANCELLED_AT, REJECTED_AT, EXPIRED_AT
-- FROM HANDOVERS
-- WHERE (CASE WHEN COMPLETED_AT IS NOT NULL THEN 1 ELSE 0 END +
--        CASE WHEN CANCELLED_AT IS NOT NULL THEN 1 ELSE 0 END +
--        CASE WHEN REJECTED_AT IS NOT NULL THEN 1 ELSE 0 END +
--        CASE WHEN EXPIRED_AT IS NOT NULL THEN 1 ELSE 0 END) > 1;

-- ========================================
-- ADD CONSTRAINTS
-- ========================================

-- Ensure state transitions follow correct sequence
-- Completed requires Accepted (shortened to fit Oracle 30-char limit)
ALTER TABLE HANDOVERS ADD CONSTRAINT CHK_COMPLETED_REQ_ACCEPTED
CHECK (COMPLETED_AT IS NULL OR ACCEPTED_AT IS NOT NULL);

-- Accepted requires Started (shortened to fit Oracle 30-char limit)
ALTER TABLE HANDOVERS ADD CONSTRAINT CHK_ACCEPTED_REQ_STARTED
CHECK (ACCEPTED_AT IS NULL OR STARTED_AT IS NOT NULL);

-- Started requires Ready
ALTER TABLE HANDOVERS ADD CONSTRAINT CHK_STARTED_REQUIRES_READY
CHECK (STARTED_AT IS NULL OR READY_AT IS NOT NULL);

-- Ensure only ONE terminal state is set at a time
-- A handover can be Completed OR Cancelled OR Rejected OR Expired, but not multiple
ALTER TABLE HANDOVERS ADD CONSTRAINT CHK_SINGLE_TERMINAL_STATE
CHECK (
  (CASE WHEN COMPLETED_AT IS NOT NULL THEN 1 ELSE 0 END +
   CASE WHEN CANCELLED_AT IS NOT NULL THEN 1 ELSE 0 END +
   CASE WHEN REJECTED_AT IS NOT NULL THEN 1 ELSE 0 END +
   CASE WHEN EXPIRED_AT IS NOT NULL THEN 1 ELSE 0 END) <= 1
);

-- ========================================
-- VERIFICATION
-- ========================================
-- Verify constraints were created successfully
SELECT CONSTRAINT_NAME, CONSTRAINT_TYPE, SEARCH_CONDITION
FROM USER_CONSTRAINTS
WHERE TABLE_NAME = 'HANDOVERS' 
  AND CONSTRAINT_TYPE = 'C'
  AND CONSTRAINT_NAME LIKE 'CHK_%'
ORDER BY CONSTRAINT_NAME;

