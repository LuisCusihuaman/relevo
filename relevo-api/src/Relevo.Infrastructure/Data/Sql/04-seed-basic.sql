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

-- Insert Shifts (templates)
INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME) VALUES ('shift-day', 'Mañana', '07:00', '15:00');
INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME) VALUES ('shift-night', 'Noche', '19:00', '07:00');

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
        TRUNC(LOCALTIMESTAMP) + INTERVAL '7' HOUR, 
        TRUNC(LOCALTIMESTAMP) + INTERVAL '15' HOUR);

INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit1-day-2', 'unit-1', 'shift-day', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '1' DAY + INTERVAL '7' HOUR, 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '1' DAY + INTERVAL '15' HOUR);

INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit1-day-3', 'unit-1', 'shift-day', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '2' DAY + INTERVAL '7' HOUR, 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '2' DAY + INTERVAL '15' HOUR);

-- Unit 1: Night shift instances (last 3 nights)
INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit1-night-1', 'unit-1', 'shift-night', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '1' DAY + INTERVAL '19' HOUR, 
        TRUNC(LOCALTIMESTAMP) + INTERVAL '7' HOUR);

INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit1-night-2', 'unit-1', 'shift-night', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '2' DAY + INTERVAL '19' HOUR, 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '1' DAY + INTERVAL '7' HOUR);

INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT)
VALUES ('si-unit1-night-3', 'unit-1', 'shift-night', 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '3' DAY + INTERVAL '19' HOUR, 
        TRUNC(LOCALTIMESTAMP) - INTERVAL '2' DAY + INTERVAL '7' HOUR);

-- ========================================
-- SHIFT WINDOWS (V3: Windows between shift instances)
-- ========================================
-- Chronological order (oldest to newest):
-- si-unit1-night-3: 3 days ago 19:00 -> 2 days ago 07:00
-- si-unit1-day-3:   2 days ago 07:00 -> 2 days ago 15:00
-- si-unit1-night-2: 2 days ago 19:00 -> yesterday 07:00
-- si-unit1-day-2:   yesterday 07:00 -> yesterday 15:00
-- si-unit1-night-1: yesterday 19:00 -> today 07:00
-- si-unit1-day-1:   today 07:00 -> today 15:00

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

-- ========================================
-- SHIFT COVERAGE (V3: Replaces USER_ASSIGNMENTS)
-- ========================================

-- Coverage for today's day shift (primary assignments)
INSERT INTO SHIFT_COVERAGE (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, IS_PRIMARY)
VALUES ('sc-001', 'user_demo12345678901234567890123456', 'pat-001', 'si-unit1-day-1', 'unit-1', 1);

INSERT INTO SHIFT_COVERAGE (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, IS_PRIMARY)
VALUES ('sc-002', 'user_demo12345678901234567890123456', 'pat-002', 'si-unit1-day-1', 'unit-1', 1);

INSERT INTO SHIFT_COVERAGE (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, IS_PRIMARY)
VALUES ('sc-003', 'user_demo12345678901234567890123456', 'pat-003', 'si-unit1-day-1', 'unit-1', 1);

-- Coverage for today's night shift
INSERT INTO SHIFT_COVERAGE (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, IS_PRIMARY)
VALUES ('sc-004', 'user_demo12345678901234567890123457', 'pat-001', 'si-unit1-night-1', 'unit-1', 1);

INSERT INTO SHIFT_COVERAGE (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, IS_PRIMARY)
VALUES ('sc-005', 'user_demo12345678901234567890123457', 'pat-002', 'si-unit1-night-1', 'unit-1', 1);

-- ========================================
-- HANDOVERS (V3: Uses SHIFT_WINDOW_ID)
-- ========================================

-- handover-001: Ready (yesterday: day-2 -> night-1)
INSERT INTO HANDOVERS (ID, PATIENT_ID, SHIFT_WINDOW_ID, UNIT_ID, SENDER_USER_ID, RECEIVER_USER_ID, CREATED_BY_USER_ID, CREATED_AT, READY_AT, READY_BY_USER_ID)
VALUES ('handover-001', 'pat-001', 'sw-unit1-day-night-1', 'unit-1', 
        'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 
        'user_demo12345678901234567890123456',
        LOCALTIMESTAMP - INTERVAL '1' HOUR, LOCALTIMESTAMP - INTERVAL '30' MINUTE, 'user_demo12345678901234567890123456');

-- handover-002: Draft (yesterday: day-2 -> night-1)
INSERT INTO HANDOVERS (ID, PATIENT_ID, SHIFT_WINDOW_ID, UNIT_ID, CREATED_BY_USER_ID)
VALUES ('handover-002', 'pat-002', 'sw-unit1-day-night-1', 'unit-1', 'user_demo12345678901234567890123456');

-- handover-003: InProgress (yesterday: day-2 -> night-1)
INSERT INTO HANDOVERS (ID, PATIENT_ID, SHIFT_WINDOW_ID, UNIT_ID, SENDER_USER_ID, RECEIVER_USER_ID, CREATED_BY_USER_ID, 
                       CREATED_AT, READY_AT, READY_BY_USER_ID, STARTED_AT, STARTED_BY_USER_ID)
VALUES ('handover-003', 'pat-003', 'sw-unit1-day-night-1', 'unit-1', 
        'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 
        'user_demo12345678901234567890123456',
        LOCALTIMESTAMP - INTERVAL '2' HOUR, LOCALTIMESTAMP - INTERVAL '1' HOUR, 'user_demo12345678901234567890123456',
        LOCALTIMESTAMP - INTERVAL '30' MINUTE, 'user_demo12345678901234567890123457');

-- ========================================
-- HANDOVER CONTENT
-- ========================================

-- Content for handover-001
INSERT INTO HANDOVER_CONTENTS(
    HANDOVER_ID, 
    ILLNESS_SEVERITY, 
    PATIENT_SUMMARY, 
    SITUATION_AWARENESS, 
    SYNTHESIS, 
    PATIENT_SUMMARY_STATUS,
    SA_STATUS,
    SYNTHESIS_STATUS,
    LAST_EDITED_BY
)
VALUES (
    'handover-001', 
    'Stable', 
    'Paciente de 14 años con neumonía adquirida en comunidad. Ingreso hace 3 días. Tratamiento con Amoxicilina y oxígeno suplementario.', 
    'Paciente estable, saturación de oxígeno 94%, requiere nebulizaciones cada 6 horas. Sin complicaciones.', 
    'Paciente estable, sin complicaciones. Buena respuesta al tratamiento antibiótico.', 
    'Completed',
    'Completed',
    'Draft',
    'user_demo12345678901234567890123456'
);

-- Content for handover-003
INSERT INTO HANDOVER_CONTENTS(
    HANDOVER_ID, 
    ILLNESS_SEVERITY, 
    PATIENT_SUMMARY, 
    SITUATION_AWARENESS, 
    SYNTHESIS, 
    PATIENT_SUMMARY_STATUS,
    SA_STATUS,
    SYNTHESIS_STATUS,
    LAST_EDITED_BY
)
VALUES (
    'handover-003', 
    'Watcher', 
    'Paciente con estado asmático agudo. Mejoría progresiva.', 
    'Requiere monitoreo de función respiratoria.', 
    'Continuar tratamiento actual.', 
    'Completed', 
    'Completed', 
    'Draft',
    'user_demo12345678901234567890123457'
);

-- ========================================
-- HANDOVER ACTION ITEMS
-- ========================================

INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('action-001', 'handover-001', 'Realizar nebulizaciones cada 6 horas', 0);

INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('action-002', 'handover-001', 'Monitorear saturación de oxígeno', 0);

INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('action-003', 'handover-001', 'Control de temperatura cada 4 horas', 1);

-- ========================================
-- HANDOVER MESSAGES
-- ========================================

INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT)
VALUES ('message-001', 'handover-001', 'user_demo12345678901234567890123456', 'Just reviewed the case. The patient seems stable today. Any concerns?', 'message', LOCALTIMESTAMP - INTERVAL '5' MINUTE);

INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT)
VALUES ('message-002', 'handover-001', 'user_demo12345678901234567890123457', 'Patient has been stable. Responded well to the treatment this morning.', 'message', LOCALTIMESTAMP - INTERVAL '3' MINUTE);

-- ========================================
-- HANDOVER CONTINGENCY
-- ========================================

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-001', 'handover-001', 'Si el paciente desarrolla dificultad respiratoria aguda o saturación de oxígeno < 92%', 'Administrar oxígeno suplementario, llamar a terapia respiratoria, considerar BIPAP, contactar al médico tratante inmediatamente', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);
