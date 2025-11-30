-- ========================================
-- CREATE VIEWS
-- ========================================

-- Connect as RELEVO_APP user
CONNECT RELEVO_APP/TuPass123;

-- View exposing handovers with explicit column selection for Dapper compatibility
-- CURRENT_STATE is now a virtual column in HANDOVERS table, so we can simply select it
-- This view maintains explicit column order/contract for Dapper mapping
CREATE OR REPLACE VIEW VW_HANDOVERS_WITH_STATE AS
SELECT
    h.ID,
    h.PATIENT_ID,
    h.PREVIOUS_HANDOVER_ID,
    h.FROM_SHIFT_ID,
    h.TO_SHIFT_ID,
    h.FROM_USER_ID,
    h.TO_USER_ID,
    h.WINDOW_START_AT,
    h.WINDOW_END_AT,
    h.CREATED_AT,
    h.UPDATED_AT,
    h.READY_AT,
    h.STARTED_AT,
    h.ACCEPTED_AT,
    h.COMPLETED_AT,
    h.CANCELLED_AT,
    h.REJECTED_AT,
    h.EXPIRED_AT,
    h.REJECTION_REASON,
    h.CURRENT_STATE
FROM HANDOVERS h;
