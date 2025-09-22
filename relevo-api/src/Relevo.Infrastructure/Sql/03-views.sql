-- ========================================
-- VISTAS
-- ========================================

-- Connect as RELEVO_APP user
CONNECT RELEVO_APP/TuPass123;

CREATE OR REPLACE VIEW VW_HANDOVERS_STATE AS
SELECT
  h.ID as HandoverId,
  CASE
    WHEN h.STATUS = 'Completed' AND h.COMPLETED_AT IS NOT NULL THEN 'Completed'
    WHEN h.CANCELLED_AT IS NOT NULL THEN 'Cancelled'
    WHEN h.REJECTED_AT  IS NOT NULL THEN 'Rejected'
    WHEN h.EXPIRED_AT   IS NOT NULL THEN 'Expired'
    WHEN h.ACCEPTED_AT  IS NOT NULL THEN 'Accepted'
    WHEN h.STARTED_AT   IS NOT NULL THEN 'InProgress'
    WHEN h.READY_AT     IS NOT NULL THEN 'Ready'
    ELSE 'Draft'
  END AS StateName
FROM HANDOVERS h;

