-- Migration: Add WEIGHT and HEIGHT columns to PATIENTS table
-- Date: 2024-12-14
-- Description: Adds support for patient weight and height in the PATIENTS table

-- Connect as RELEVO_APP user
CONNECT RELEVO_APP/TuPass123;

-- Add WEIGHT column (nullable, stores weight in kg)
ALTER TABLE PATIENTS 
ADD WEIGHT VARCHAR2(20);

-- Add HEIGHT column (nullable, stores height in cm)
ALTER TABLE PATIENTS 
ADD HEIGHT VARCHAR2(20);

-- Add comments for documentation
COMMENT ON COLUMN PATIENTS.WEIGHT IS 'Patient weight in kilograms (kg)';
COMMENT ON COLUMN PATIENTS.HEIGHT IS 'Patient height in centimeters (cm)';

