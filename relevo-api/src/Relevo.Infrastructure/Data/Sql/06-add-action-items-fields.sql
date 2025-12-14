-- Migration: Add PRIORITY, DUE_TIME, and CREATED_BY columns to HANDOVER_ACTION_ITEMS
-- Date: 2024-12-14
-- Description: Adds support for priority, due time, and creator tracking in action items

-- Add PRIORITY column
ALTER TABLE HANDOVER_ACTION_ITEMS 
ADD PRIORITY VARCHAR2(20) DEFAULT 'medium';

-- Add DUE_TIME column (nullable, stores time in HH:mm format)
ALTER TABLE HANDOVER_ACTION_ITEMS 
ADD DUE_TIME VARCHAR2(10);

-- Add CREATED_BY column (stores the name or ID of the user who created the item)
ALTER TABLE HANDOVER_ACTION_ITEMS 
ADD CREATED_BY VARCHAR2(200);

-- Update existing records to have default priority
UPDATE HANDOVER_ACTION_ITEMS 
SET PRIORITY = 'medium' 
WHERE PRIORITY IS NULL;

-- Add comment for documentation
COMMENT ON COLUMN HANDOVER_ACTION_ITEMS.PRIORITY IS 'Priority level: low, medium, high';
COMMENT ON COLUMN HANDOVER_ACTION_ITEMS.DUE_TIME IS 'Due time in HH:mm format (24-hour)';
COMMENT ON COLUMN HANDOVER_ACTION_ITEMS.CREATED_BY IS 'Name or ID of the user who created this action item';

