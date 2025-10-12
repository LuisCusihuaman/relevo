-- ========================================
-- ADD VERSION COLUMN FOR OPTIMISTIC LOCKING
-- ========================================
-- This script adds a VERSION column to the HANDOVERS table for implementing optimistic concurrency control
-- The version is incremented on every UPDATE, allowing detection of concurrent modifications

-- Connect as RELEVO_APP user
CONNECT RELEVO_APP/TuPass123;

-- Add VERSION column with default value of 1
ALTER TABLE HANDOVERS ADD VERSION NUMBER DEFAULT 1 NOT NULL;

-- Create index on ID and VERSION for optimistic lock checks
CREATE INDEX IDX_HANDOVERS_VERSION ON HANDOVERS(ID, VERSION);

-- ========================================
-- VERIFICATION
-- ========================================
-- Verify column was added and has default value
SELECT COLUMN_NAME, DATA_TYPE, DATA_DEFAULT, NULLABLE
FROM USER_TAB_COLUMNS
WHERE TABLE_NAME = 'HANDOVERS' AND COLUMN_NAME = 'VERSION';

-- Verify index was created
SELECT INDEX_NAME, TABLE_NAME, COLUMN_NAME
FROM USER_IND_COLUMNS
WHERE TABLE_NAME = 'HANDOVERS' AND INDEX_NAME = 'IDX_HANDOVERS_VERSION'
ORDER BY COLUMN_POSITION;

-- Check existing handovers have version = 1
SELECT COUNT(*) AS HANDOVERS_WITH_VERSION_1
FROM HANDOVERS
WHERE VERSION = 1;

