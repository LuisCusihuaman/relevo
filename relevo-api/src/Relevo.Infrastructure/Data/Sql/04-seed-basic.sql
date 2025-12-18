-- ========================================
-- INSERT SEED DATA
-- ========================================
-- Schema V3: Seed data for shift instances + windows model

-- Connect as RELEVO_APP user
CONNECT RELEVO_APP/TuPass123;

-- ========================================
-- BASE DATA
-- ========================================

-- Insert Units
INSERT INTO UNITS (ID, NAME, DESCRIPTION) VALUES ('unit-1', 'UCI', 'Unidad de Cuidados Intensivos');
INSERT INTO UNITS (ID, NAME, DESCRIPTION) VALUES ('unit-2', 'Pediatría General', 'Unidad de Pediatría General');
INSERT INTO UNITS (ID, NAME, DESCRIPTION) VALUES ('unit-3', 'Pediatría Especializada', 'Unidad de Pediatría Especializada');

-- Insert/Update Shifts (templates) - Using MERGE for idempotency
MERGE INTO SHIFTS s
USING (SELECT 'shift-day' AS ID FROM DUAL) src ON (s.ID = src.ID)
WHEN MATCHED THEN
    UPDATE SET NAME = 'Mañana', START_TIME = '08:00', END_TIME = '14:00'
WHEN NOT MATCHED THEN
    INSERT (ID, NAME, START_TIME, END_TIME) VALUES ('shift-day', 'Mañana', '08:00', '14:00');

MERGE INTO SHIFTS s
USING (SELECT 'shift-night' AS ID FROM DUAL) src ON (s.ID = src.ID)
WHEN MATCHED THEN
    UPDATE SET NAME = 'Noche', START_TIME = '14:00', END_TIME = '08:00'
WHEN NOT MATCHED THEN
    INSERT (ID, NAME, START_TIME, END_TIME) VALUES ('shift-night', 'Noche', '14:00', '08:00');

-- Insert Patients for Unit 1 (ICU) - 12 patients
-- Weight and Height values are age-appropriate pediatric ranges
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-001', 'María García', 'unit-1', TO_DATE('2010-03-15', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '3' DAY, '201', 'Neumonía adquirida en comunidad', 'Penicilina', 'Amoxicilina, Oxígeno suplementario', 'Paciente estable, saturación de oxígeno 94%, requiere nebulizaciones cada 6 horas', 'MRN001234', '54', '158');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-002', 'Carlos Rodríguez', 'unit-1', TO_DATE('2008-07-22', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '5' DAY, '202', 'Sepsis secundaria a infección urinaria', 'Sulfonamidas', 'Meropenem, Vasopresores', 'Paciente crítico, requiere monitoreo continuo de signos vitales y soporte hemodinámico', 'MRN001235', '63', '168');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-003', 'Ana López', 'unit-1', TO_DATE('2012-11-08', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '2' DAY, '203', 'Estado asmático agudo', 'Ninguna', 'Salbutamol, Corticoides intravenosos', 'Paciente con mejoría progresiva, disminución en requerimiento de oxígeno', 'MRN001236', '44', '148');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-004', 'Miguel Hernández', 'unit-1', TO_DATE('2009-05-30', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '7' DAY, '204', 'Trauma craneoencefálico moderado', 'Látex', 'Manitol, Analgésicos', 'Glasgow 12/15, pupilas isocóricas, requiere monitoreo neurológico frecuente', 'MRN001237', '58', '162');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-005', 'Isabella González', 'unit-1', TO_DATE('2011-09-14', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '4' DAY, '205', 'Insuficiencia respiratoria aguda', 'Iodo', 'Ventilación mecánica, Sedantes', 'Paciente intubado, parámetros ventilatorios estables', 'MRN001238', '48', '152');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-006', 'David Pérez', 'unit-1', TO_DATE('2013-01-25', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '6' DAY, '206', 'Choque séptico', 'Ninguna', 'Antibióticos de amplio espectro, Fluidos', 'Paciente en shock distributivo, requiere soporte vasopresor', 'MRN001239', '38', '142');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-007', 'Sofia Martínez', 'unit-1', TO_DATE('2007-12-03', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '1' DAY, '207', 'Meningitis bacteriana', 'Penicilina', 'Ceftriaxona, Dexametasona', 'Paciente con mejoría clínica, cultivos pendientes', 'MRN001240', '68', '172');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-008', 'José Sánchez', 'unit-1', TO_DATE('2014-06-18', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '8' DAY, '208', 'Quemaduras de segundo grado', 'Ninguna', 'Analgésicos, Antibióticos tópicos', 'Quemaduras en 25% superficie corporal, requiere curas diarias', 'MRN001241', '33', '138');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-009', 'Carmen Díaz', 'unit-1', TO_DATE('2010-08-12', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '9' DAY, '209', 'Convulsiones febriles', 'Ninguna', 'Antiepilépticos, Antipiréticos', 'Paciente estable, sin recurrencia de convulsiones', 'MRN001242', '52', '156');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-010', 'Antonio Moreno', 'unit-1', TO_DATE('2006-04-07', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '12' DAY, '210', 'Intoxicación medicamentosa', 'Aspirina', 'Carbón activado, Soporte vital', 'Paciente estabilizado, requiere monitoreo de función hepática', 'MRN001243', '72', '178');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-011', 'Elena Jiménez', 'unit-1', TO_DATE('2012-02-28', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '3' DAY, '211', 'Hipoglucemia severa', 'Ninguna', 'Glucosa intravenosa, Insulina', 'Episodio resuelto, requiere educación diabética', 'MRN001244', '43', '149');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-012', 'Francisco Ruiz', 'unit-1', TO_DATE('2008-10-19', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '5' DAY, '212', 'Trauma abdominal', 'Ninguna', 'Analgésicos, Antibióticos profilácticos', 'Paciente estable, sin signos de peritonitis', 'MRN001245', '61', '166');

-- Insert Patients for Unit 2 (Pediatría General) - 10 patients
-- Weight and Height values are age-appropriate pediatric ranges
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-013', 'Lucía Fernández', 'unit-2', TO_DATE('2015-04-12', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '2' DAY, '301', 'Gastroenteritis aguda', 'Ninguna', 'Solución de rehidratación oral, Probióticos', 'Paciente con vómitos y diarrea, mejoría con hidratación oral, tolerando dieta blanda', 'MRN001246', '28', '125');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-014', 'Diego Torres', 'unit-2', TO_DATE('2016-08-25', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '1' DAY, '302', 'Infección respiratoria alta', 'Penicilina', 'Amoxicilina, Antitusivos', 'Rinorrea y tos productiva, fiebre controlada, buen estado general', 'MRN001247', '24', '118');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-015', 'Valentina Morales', 'unit-2', TO_DATE('2014-11-30', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '3' DAY, '303', 'Otitis media aguda', 'Ninguna', 'Amoxicilina-ácido clavulánico, Analgésicos', 'Dolor de oído derecho, otoscopia con tímpano abombado, mejoría con tratamiento', 'MRN001248', '32', '135');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-016', 'Mateo Castro', 'unit-2', TO_DATE('2017-02-14', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '4' DAY, '304', 'Amigdalitis bacteriana', 'Penicilina', 'Azitromicina, Antipiréticos', 'Faringoamigdalitis con exudado, cultivo positivo para estreptococo, tratamiento antibiótico iniciado', 'MRN001249', '20', '110');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-017', 'Camila Vargas', 'unit-2', TO_DATE('2015-09-08', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '2' DAY, '305', 'Bronquitis aguda', 'Ninguna', 'Salbutamol inhalado, Expectorantes', 'Tos persistente con sibilancias leves, respuesta favorable a broncodilatadores', 'MRN001250', '26', '122');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-018', 'Santiago Ramírez', 'unit-2', TO_DATE('2016-12-20', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '1' DAY, '306', 'Fiebre sin foco aparente', 'Ibuprofeno', 'Paracetamol, Observación', 'Fiebre de 38.5°C sin foco evidente, estudios complementarios en curso, estado general conservado', 'MRN001251', '22', '115');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-019', 'Emma Herrera', 'unit-2', TO_DATE('2014-05-03', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '5' DAY, '307', 'Deshidratación leve', 'Ninguna', 'Solución salina intravenosa, Rehidratación oral', 'Deshidratación secundaria a gastroenteritis, mejoría con hidratación parenteral, tolerando vía oral', 'MRN001252', '30', '130');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-020', 'Sebastián Jiménez', 'unit-2', TO_DATE('2015-07-17', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '3' DAY, '308', 'Infección urinaria', 'Sulfonamidas', 'Cefalexina, Analgésicos', 'Cistitis con urocultivo positivo, tratamiento antibiótico, mejoría de síntomas', 'MRN001253', '27', '128');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-021', 'Isabella Rojas', 'unit-2', TO_DATE('2016-10-29', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '2' DAY, '309', 'Conjuntivitis bacteriana', 'Ninguna', 'Colirio antibiótico, Higiene ocular', 'Conjuntivitis bilateral con secreción purulenta, tratamiento tópico iniciado, mejoría progresiva', 'MRN001254', '23', '120');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN, WEIGHT, HEIGHT)
VALUES ('pat-022', 'Lucas Mendoza', 'unit-2', TO_DATE('2014-01-22', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '4' DAY, '310', 'Dermatitis atópica exacerbada', 'Látex', 'Corticoides tópicos, Antihistamínicos', 'Dermatitis con lesiones eccematosas en pliegues, tratamiento tópico y medidas de cuidado de la piel', 'MRN001255', '31', '132');

-- Insert sample users
INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, AVATAR_URL, ROLE, IS_ACTIVE, LAST_LOGIN)
VALUES ('user_demo12345678901234567890123456', 'dr.johnson@hospital.com', 'John', 'Johnson', 'Dr. John Johnson', 'https://example.com/avatar1.jpg', 'doctor', 1, LOCALTIMESTAMP);

INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, AVATAR_URL, ROLE, IS_ACTIVE, LAST_LOGIN)
VALUES ('user_demo12345678901234567890123457', 'dr.patel@hospital.com', 'Priya', 'Patel', 'Dr. Priya Patel', 'https://example.com/avatar2.jpg', 'doctor', 1, LOCALTIMESTAMP);

INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, AVATAR_URL, ROLE, IS_ACTIVE, LAST_LOGIN)
VALUES ('user_demo12345678901234567890123458', 'dr.martinez@hospital.com', 'Carlos', 'Martinez', 'Dr. Carlos Martinez', 'https://example.com/avatar3.jpg', 'doctor', 1, LOCALTIMESTAMP);

-- System user for automated cancellations
INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, AVATAR_URL, ROLE, IS_ACTIVE, LAST_LOGIN)
VALUES ('system', 'system@relevo.app', 'System', 'Bot', 'System Bot', NULL, 'system', 1, NULL);

-- ========================================
-- SHIFT INSTANCES (V3: Real occurrences with dates/times)
-- ========================================

-- Unit 1: Day shift instances (last 3 days)
INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit1-day-1', 'unit-1', 'shift-day', 
        TRUNC(LOCALTIMESTAMP) + INTERVAL '8' HOUR, 
        TRUNC(LOCALTIMESTAMP) + INTERVAL '14' HOUR);

INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit1-day-2', 'unit-1', 'shift-day', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '1' DAY + INTERVAL '8' HOUR, 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '1' DAY + INTERVAL '14' HOUR);

INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit1-day-3', 'unit-1', 'shift-day', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '2' DAY + INTERVAL '8' HOUR, 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '2' DAY + INTERVAL '14' HOUR);

-- Unit 1: Night shift instances (last 3 nights)
INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit1-night-1', 'unit-1', 'shift-night', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '1' DAY + INTERVAL '14' HOUR, 
        TRUNC(LOCALTIMESTAMP) + INTERVAL '8' HOUR);

INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit1-night-2', 'unit-1', 'shift-night', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '2' DAY + INTERVAL '14' HOUR, 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '1' DAY + INTERVAL '8' HOUR);

INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit1-night-3', 'unit-1', 'shift-night', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '3' DAY + INTERVAL '14' HOUR, 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '2' DAY + INTERVAL '8' HOUR);

-- Unit 2: Day shift instances (last 3 days)
INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit2-day-1', 'unit-2', 'shift-day', 
        TRUNC(LOCALTIMESTAMP) + INTERVAL '8' HOUR, 
        TRUNC(LOCALTIMESTAMP) + INTERVAL '14' HOUR);

INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit2-day-2', 'unit-2', 'shift-day', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '1' DAY + INTERVAL '8' HOUR, 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '1' DAY + INTERVAL '14' HOUR);

INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit2-day-3', 'unit-2', 'shift-day', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '2' DAY + INTERVAL '8' HOUR, 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '2' DAY + INTERVAL '14' HOUR);

-- Unit 2: Night shift instances (last 3 nights)
INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit2-night-1', 'unit-2', 'shift-night', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '1' DAY + INTERVAL '14' HOUR, 
        TRUNC(LOCALTIMESTAMP) + INTERVAL '8' HOUR);

INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit2-night-2', 'unit-2', 'shift-night', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '2' DAY + INTERVAL '14' HOUR, 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '1' DAY + INTERVAL '8' HOUR);

INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit2-night-3', 'unit-2', 'shift-night', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '3' DAY + INTERVAL '14' HOUR, 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '2' DAY + INTERVAL '8' HOUR);

-- ========================================
-- SHIFT WINDOWS (V3: Windows between shift instances)
-- ========================================
-- Chronological order (oldest to newest):
-- si-unit1-night-3: 3 days ago 14:00 -> 2 days ago 08:00
-- si-unit1-day-3:   2 days ago 08:00 -> 2 days ago 14:00
-- si-unit1-night-2: 2 days ago 14:00 -> yesterday 08:00
-- si-unit1-day-2:   yesterday 08:00 -> yesterday 14:00
-- si-unit1-night-1: yesterday 14:00 -> today 08:00
-- si-unit1-day-1:   today 08:00 -> today 14:00

-- Window: Night -> Day (2 days ago: night-3 -> day-3)
INSERT INTO SHIFT_WINDOWS (ID, UNIT_ID, FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID)
VALUES ('sw-unit1-night-day-2', 'unit-1', 'si-unit1-night-3', 'si-unit1-day-3');

-- Window: Day -> Night (2 days ago: day-3 -> night-2)
INSERT INTO SHIFT_WINDOWS (ID, UNIT_ID, FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID)
VALUES ('sw-unit1-day-night-2', 'unit-1', 'si-unit1-day-3', 'si-unit1-night-2');

-- Window: Night -> Day (yesterday: night-2 -> day-2)
INSERT INTO SHIFT_WINDOWS (ID, UNIT_ID, FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID)
VALUES ('sw-unit1-night-day-1', 'unit-1', 'si-unit1-night-2', 'si-unit1-day-2');

-- Window: Day -> Night (yesterday: day-2 -> night-1)
INSERT INTO SHIFT_WINDOWS (ID, UNIT_ID, FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID)
VALUES ('sw-unit1-day-night-1', 'unit-1', 'si-unit1-day-2', 'si-unit1-night-1');

-- Window: Night -> Day (today: night-1 -> day-1)
INSERT INTO SHIFT_WINDOWS (ID, UNIT_ID, FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID)
VALUES ('sw-unit1-night-day-0', 'unit-1', 'si-unit1-night-1', 'si-unit1-day-1');

-- Unit 2: Shift Windows
-- Window: Night -> Day (2 days ago: night-3 -> day-3)
INSERT INTO SHIFT_WINDOWS (ID, UNIT_ID, FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID)
VALUES ('sw-unit2-night-day-2', 'unit-2', 'si-unit2-night-3', 'si-unit2-day-3');

-- Window: Day -> Night (2 days ago: day-3 -> night-2)
INSERT INTO SHIFT_WINDOWS (ID, UNIT_ID, FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID)
VALUES ('sw-unit2-day-night-2', 'unit-2', 'si-unit2-day-3', 'si-unit2-night-2');

-- Window: Night -> Day (yesterday: night-2 -> day-2)
INSERT INTO SHIFT_WINDOWS (ID, UNIT_ID, FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID)
VALUES ('sw-unit2-night-day-1', 'unit-2', 'si-unit2-night-2', 'si-unit2-day-2');

-- Window: Day -> Night (yesterday: day-2 -> night-1)
INSERT INTO SHIFT_WINDOWS (ID, UNIT_ID, FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID)
VALUES ('sw-unit2-day-night-1', 'unit-2', 'si-unit2-day-2', 'si-unit2-night-1');

-- Window: Night -> Day (today: night-1 -> day-1)
INSERT INTO SHIFT_WINDOWS (ID, UNIT_ID, FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID)
VALUES ('sw-unit2-night-day-0', 'unit-2', 'si-unit2-night-1', 'si-unit2-day-1');

-- ========================================
-- SHIFT COVERAGE (V3: Replaces USER_ASSIGNMENTS)
-- ========================================
-- NO pre-created coverage - use the app to assign patients and test automatic handover creation
-- 
-- Test flow:
-- 1. Login as Dr. Johnson (user_demo...56) 
-- 2. Assign yourself to pat-001 in day shift (FROM) → Handover created automatically (Draft)
-- 3. Login as Dr. Patel (user_demo...57)
-- 4. Assign yourself to pat-001 in night shift (TO) → Now you can receive
-- 5. Dr. Johnson marks as Ready → State: Ready
-- 6. Dr. Patel starts handover → State: InProgress  
-- 7. Dr. Patel completes handover → State: Completed
--
-- Available users:
-- - user_demo12345678901234567890123456 (Dr. John Johnson)
-- - user_demo12345678901234567890123457 (Dr. Priya Patel)
-- - user_demo12345678901234567890123458 (Dr. Carlos Martinez)
--
-- Available patients: 
--   - pat-001 to pat-012 (unit-1: UCI, no assignments)
--   - pat-013 to pat-022 (unit-2: Pediatría General, no assignments)
-- Available shifts: shift-day (08:00-14:00), shift-night (14:00-08:00)

-- ========================================
-- NO HANDOVERS - Created automatically when assigning patients
-- ========================================
-- Handovers are created as side effects of domain commands (Rule 24)
-- The PatientAssignedToShiftHandler creates handovers when:
-- - A patient is assigned to a shift (coverage created)
-- - The assignment is primary (IS_PRIMARY=1)
