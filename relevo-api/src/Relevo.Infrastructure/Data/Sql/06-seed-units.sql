-- Quick seed script for UNITS table only
-- Execute with: docker exec -i xe11 sqlplus RELEVO_APP/TuPass123@localhost:1521/XE @/container-entrypoint-initdb.d/06-seed-units.sql

-- Insert Units (with MERGE to avoid duplicates)
MERGE INTO UNITS u
USING (
  SELECT 'unit-1' AS ID, 'UCI' AS NAME, 'Unidad de Cuidados Intensivos' AS DESCRIPTION FROM DUAL
  UNION ALL
  SELECT 'unit-2' AS ID, 'Pediatría General' AS NAME, 'Unidad de Pediatría General' AS DESCRIPTION FROM DUAL
  UNION ALL
  SELECT 'unit-3' AS ID, 'Pediatría Especializada' AS NAME, 'Unidad de Pediatría Especializada' AS DESCRIPTION FROM DUAL
) s
ON (u.ID = s.ID)
WHEN NOT MATCHED THEN
  INSERT (ID, NAME, DESCRIPTION) VALUES (s.ID, s.NAME, s.DESCRIPTION);

COMMIT;

