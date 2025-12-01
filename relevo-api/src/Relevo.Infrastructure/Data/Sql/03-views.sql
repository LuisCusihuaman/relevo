-- ========================================
-- CREATE VIEWS
-- ========================================
-- Schema V3: Views for shift instances + windows model

-- Connect as RELEVO_APP user
CONNECT RELEVO_APP/TuPass123;

-- View exposing handovers with explicit column selection for Dapper compatibility
-- CURRENT_STATE is now a virtual column in HANDOVERS table, so we can simply select it
-- This view maintains explicit column order/contract for Dapper mapping
CREATE OR REPLACE VIEW VW_HANDOVERS_WITH_STATE AS
SELECT
    h.ID,
    h.PATIENT_ID,
    h.SHIFT_WINDOW_ID,
    h.UNIT_ID,
    h.PREVIOUS_HANDOVER_ID,
    h.SENDER_USER_ID,
    h.RECEIVER_USER_ID,
    h.CREATED_BY_USER_ID,
    h.CREATED_AT,
    h.UPDATED_AT,
    h.READY_AT,
    h.READY_BY_USER_ID,
    h.STARTED_AT,
    h.STARTED_BY_USER_ID,
    h.COMPLETED_AT,
    h.COMPLETED_BY_USER_ID,
    h.CANCELLED_AT,
    h.CANCELLED_BY_USER_ID,
    h.CANCEL_REASON,
    h.CURRENT_STATE
FROM HANDOVERS h;
