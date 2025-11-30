-- ========================================
-- INSERT SEED DATA
-- ========================================

-- Connect as RELEVO_APP user
CONNECT RELEVO_APP/TuPass123;

-- Insert Units
INSERT INTO UNITS (ID, NAME, DESCRIPTION) VALUES ('unit-1', 'UCI', 'Unidad de Cuidados Intensivos');
INSERT INTO UNITS (ID, NAME, DESCRIPTION) VALUES ('unit-2', 'Pediatría General', 'Unidad de Pediatría General');
INSERT INTO UNITS (ID, NAME, DESCRIPTION) VALUES ('unit-3', 'Pediatría Especializada', 'Unidad de Pediatría Especializada');

-- Insert Shifts
INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME) VALUES ('shift-day', 'Mañana', '07:00', '15:00');
INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME) VALUES ('shift-night', 'Noche', '19:00', '07:00');

-- Insert Patients for Unit 1 (ICU) - 12 patients
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-001', 'María García', 'unit-1', TO_DATE('2010-03-15', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '3' DAY, '201', 'Neumonía adquirida en comunidad', 'Penicilina', 'Amoxicilina, Oxígeno suplementario', 'Paciente estable, saturación de oxígeno 94%, requiere nebulizaciones cada 6 horas', 'MRN001234');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-002', 'Carlos Rodríguez', 'unit-1', TO_DATE('2008-07-22', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '5' DAY, '202', 'Sepsis secundaria a infección urinaria', 'Sulfonamidas', 'Meropenem, Vasopresores', 'Paciente crítico, requiere monitoreo continuo de signos vitales y soporte hemodinámico', 'MRN001235');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-003', 'Ana López', 'unit-1', TO_DATE('2012-11-08', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '2' DAY, '203', 'Estado asmático agudo', 'Ninguna', 'Salbutamol, Corticoides intravenosos', 'Paciente con mejoría progresiva, disminución en requerimiento de oxígeno', 'MRN001236');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-004', 'Miguel Hernández', 'unit-1', TO_DATE('2009-05-30', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '7' DAY, '204', 'Trauma craneoencefálico moderado', 'Látex', 'Manitol, Analgésicos', 'Glasgow 12/15, pupilas isocóricas, requiere monitoreo neurológico frecuente', 'MRN001237');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-005', 'Isabella González', 'unit-1', TO_DATE('2011-09-14', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '4' DAY, '205', 'Insuficiencia respiratoria aguda', 'Iodo', 'Ventilación mecánica, Sedantes', 'Paciente intubado, parámetros ventilatorios estables', 'MRN001238');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-006', 'David Pérez', 'unit-1', TO_DATE('2013-01-25', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '6' DAY, '206', 'Choque séptico', 'Ninguna', 'Antibióticos de amplio espectro, Fluidos', 'Paciente en shock distributivo, requiere soporte vasopresor', 'MRN001239');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-007', 'Sofia Martínez', 'unit-1', TO_DATE('2007-12-03', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '1' DAY, '207', 'Meningitis bacteriana', 'Penicilina', 'Ceftriaxona, Dexametasona', 'Paciente con mejoría clínica, cultivos pendientes', 'MRN001240');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-008', 'José Sánchez', 'unit-1', TO_DATE('2014-06-18', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '8' DAY, '208', 'Quemaduras de segundo grado', 'Ninguna', 'Analgésicos, Antibióticos tópicos', 'Quemaduras en 25% superficie corporal, requiere curas diarias', 'MRN001241');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-009', 'Carmen Díaz', 'unit-1', TO_DATE('2010-08-12', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '9' DAY, '209', 'Convulsiones febriles', 'Ninguna', 'Antiepilépticos, Antipiréticos', 'Paciente estable, sin recurrencia de convulsiones', 'MRN001242');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-010', 'Antonio Moreno', 'unit-1', TO_DATE('2006-04-07', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '12' DAY, '210', 'Intoxicación medicamentosa', 'Aspirina', 'Carbón activado, Soporte vital', 'Paciente estabilizado, requiere monitoreo de función hepática', 'MRN001243');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-011', 'Elena Jiménez', 'unit-1', TO_DATE('2012-02-28', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '3' DAY, '211', 'Hipoglucemia severa', 'Ninguna', 'Glucosa intravenosa, Insulina', 'Episodio resuelto, requiere educación diabética', 'MRN001244');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-012', 'Francisco Ruiz', 'unit-1', TO_DATE('2008-10-19', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '5' DAY, '212', 'Trauma abdominal', 'Ninguna', 'Analgésicos, Antibióticos profilácticos', 'Paciente estable, sin signos de peritonitis', 'MRN001245');

-- Insert Patients for Unit 2 (General Pediatrics) - 12 patients
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-013', 'Lucía Álvarez', 'unit-2', TO_DATE('2015-03-12', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '2' DAY, '301', 'Bronquiolitis', 'Ninguna', 'Salbutamol, Hidratación', 'Paciente con mejoría respiratoria, buena respuesta al tratamiento', 'MRN001246');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-014', 'Pablo Romero', 'unit-2', TO_DATE('2011-07-08', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '1' DAY, '302', 'Gastroenteritis aguda', 'Ninguna', 'Rehidratación oral, Ondansetrón', 'Paciente con buena tolerancia oral, sin vómitos', 'MRN001247');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-015', 'Valentina Navarro', 'unit-2', TO_DATE('2013-11-25', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '3' DAY, '303', 'Otitis media aguda', 'Amoxicilina', 'Amoxicilina oral, Analgésicos', 'Paciente afebril, disminución del dolor otológico', 'MRN001248');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-016', 'Diego Torres', 'unit-2', TO_DATE('2009-09-14', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '4' DAY, '304', 'Neumonía adquirida en comunidad', 'Ninguna', 'Amoxicilina, Broncodilatadores', 'Paciente afebril, mejoría radiológica en progreso', 'MRN001249');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-017', 'Marta Ramírez', 'unit-2', TO_DATE('2014-05-30', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '2' DAY, '305', 'Infección urinaria', 'Sulfonamidas', 'Cefuroxima, Analgésicos', 'Urocultivo positivo, tratamiento dirigido iniciado', 'MRN001250');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-018', 'Adrián Gil', 'unit-2', TO_DATE('2010-12-03', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '5' DAY, '306', 'Fractura de antebrazo', 'Codeína', 'Ibuprofeno, Inmovilización', 'Fractura simple, alineación adecuada lograda', 'MRN001251');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-019', 'Clara Serrano', 'unit-2', TO_DATE('2012-08-17', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '1' DAY, '307', 'Varicela', 'Ninguna', 'Antihistamínicos, Antipiréticos', 'Lesiones en fase de costra, prurito controlado', 'MRN001252');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-020', 'Hugo Castro', 'unit-2', TO_DATE('2016-01-22', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '3' DAY, '308', 'Deshidratación moderada', 'Ninguna', 'Rehidratación intravenosa', 'Paciente hidratado, buena diuresis', 'MRN001253');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-021', 'Natalia Rubio', 'unit-2', TO_DATE('2013-04-09', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '6' DAY, '309', 'Apendicitis aguda', 'Ninguna', 'Antibióticos, Analgésicos', 'Paciente postoperatorio, evolución favorable', 'MRN001254');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-022', 'Iván Ortega', 'unit-2', TO_DATE('2008-11-14', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '2' DAY, '310', 'Asma agudizada', 'Ninguna', 'Salbutamol, Corticoides', 'Paciente con buen control sintomático', 'MRN001255');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-023', 'Paula Delgado', 'unit-2', TO_DATE('2011-06-27', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '7' DAY, '311', 'Faringoamigdalitis', 'Penicilina', 'Azitromicina, Analgésicos', 'Paciente afebril, mejoría de odinofagia', 'MRN001256');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-024', 'Mario Guerrero', 'unit-2', TO_DATE('2014-02-11', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '4' DAY, '312', 'Traumatismo craneoencefálico leve', 'Ninguna', 'Analgésicos, Observación', 'Glasgow 15/15, paciente asintomático', 'MRN001257');

-- Insert Patients for Unit 3 (Specialized Pediatrics) - 11 patients
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-025', 'Laura Flores', 'unit-3', TO_DATE('2009-07-05', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '10' DAY, '401', 'Cardiopatía congénita', 'Ninguna', 'Digoxina, Diuréticos', 'Paciente compensado, requiere seguimiento cardiológico', 'MRN001258');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-026', 'Álvaro Vargas', 'unit-3', TO_DATE('2010-12-18', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '8' DAY, '402', 'Diabetes mellitus tipo 1', 'Ninguna', 'Insulina, Control glucémico', 'Buen control metabólico, educación diabética en progreso', 'MRN001259');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-027', 'Cristina Medina', 'unit-3', TO_DATE('2007-09-23', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '15' DAY, '403', 'Fibrosis quística', 'Ninguna', 'Antibióticos, Enzimas pancreáticas', 'Paciente estable, requiere fisioterapia respiratoria', 'MRN001260');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-028', 'Sergio Herrera', 'unit-3', TO_DATE('2012-04-30', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '6' DAY, '404', 'Trastorno del espectro autista', 'Ninguna', 'No farmacológico', 'Paciente en programa de intervención temprana', 'MRN001261');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-029', 'Alicia Castro', 'unit-3', TO_DATE('2011-08-14', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '12' DAY, '405', 'Epilepsia refractaria', 'Ninguna', 'Antiepilépticos múltiples', 'Paciente con control parcial de convulsiones', 'MRN001262');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-030', 'Roberto Vega', 'unit-3', TO_DATE('2008-03-07', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '20' DAY, '406', 'Leucemia linfoblástica aguda', 'Ninguna', 'Quimioterapia, Soporte', 'Paciente en protocolo de tratamiento oncológico', 'MRN001263');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-031', 'Beatriz León', 'unit-3', TO_DATE('2013-11-29', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '9' DAY, '407', 'Síndrome de Down con cardiopatía', 'Ninguna', 'Medicamentos cardíacos', 'Paciente compensado hemodinámicamente', 'MRN001264');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-032', 'Manuel Peña', 'unit-3', TO_DATE('2006-06-12', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '25' DAY, '408', 'Parálisis cerebral', 'Ninguna', 'Antiespásticos, Fisioterapia', 'Paciente en programa de rehabilitación intensiva', 'MRN001265');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-033', 'Silvia Cortés', 'unit-3', TO_DATE('2014-01-16', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '7' DAY, '409', 'Prematuridad extrema', 'Ninguna', 'Nutrición especializada, Soporte respiratorio', 'Paciente en seguimiento de desarrollo neurológico', 'MRN001266');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-034', 'Fernando Aguilar', 'unit-3', TO_DATE('2010-05-03', 'YYYY-MM-DD'), 'Male', LOCALTIMESTAMP - INTERVAL '11' DAY, '410', 'Trastorno de déficit de atención', 'Ninguna', 'Estimulantes, Terapia conductual', 'Paciente con buena respuesta al tratamiento', 'MRN001267');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-035', 'Teresa Santana', 'unit-3', TO_DATE('2009-10-25', 'YYYY-MM-DD'), 'Female', LOCALTIMESTAMP - INTERVAL '14' DAY, '411', 'Talasemia mayor', 'Ninguna', 'Transfusiones, Quelantes', 'Paciente en programa de trasfusiones crónicas', 'MRN001268');

-- Insert sample users (must be before USER_ASSIGNMENTS due to FK)
INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, AVATAR_URL, ROLE, IS_ACTIVE, LAST_LOGIN)
VALUES ('user_demo12345678901234567890123456', 'dr.johnson@hospital.com', 'John', 'Johnson', 'Dr. John Johnson', 'https://example.com/avatar1.jpg', 'doctor', 1, LOCALTIMESTAMP);

INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, AVATAR_URL, ROLE, IS_ACTIVE, LAST_LOGIN)
VALUES ('user_demo12345678901234567890123457', 'dr.patel@hospital.com', 'Priya', 'Patel', 'Dr. Priya Patel', 'https://example.com/avatar2.jpg', 'doctor', 1, LOCALTIMESTAMP);

INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, AVATAR_URL, ROLE, IS_ACTIVE, LAST_LOGIN)
VALUES ('user_demo12345678901234567890123458', 'dr.martinez@hospital.com', 'Carlos', 'Martinez', 'Dr. Carlos Martinez', 'https://example.com/avatar3.jpg', 'doctor', 1, LOCALTIMESTAMP);

-- Insert user-patient-shift assignments (after USERS due to FK)
INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
VALUES ('assign-001', 'user_demo12345678901234567890123456', 'shift-day', 'pat-001', LOCALTIMESTAMP);

INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
VALUES ('assign-002', 'user_demo12345678901234567890123456', 'shift-night', 'pat-002', LOCALTIMESTAMP - INTERVAL '1' DAY);

INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
VALUES ('assign-003', 'user_demo12345678901234567890123456', 'shift-day', 'pat-003', LOCALTIMESTAMP - INTERVAL '2' DAY);

INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
VALUES ('assign-004', 'user_demo12345678901234567890123456', 'shift-night', 'pat-004', LOCALTIMESTAMP - INTERVAL '3' DAY);

INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
VALUES ('assign-005', 'user_demo12345678901234567890123456', 'shift-day', 'pat-005', LOCALTIMESTAMP - INTERVAL '4' DAY);

-- Insert sample handovers (Schema v2)
-- NOTE: READY_AT must be >= CREATED_AT (constraint CHK_HO_RD_AFTER_CR)
-- We use explicit CREATED_AT to respect temporal order

-- handover-001: Ready (with complete content)
INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT, CREATED_AT, READY_AT)
VALUES ('handover-001', 'pat-001', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 
        LOCALTIMESTAMP - INTERVAL '2' HOUR, LOCALTIMESTAMP - INTERVAL '1' HOUR, LOCALTIMESTAMP - INTERVAL '30' MINUTE);

-- handover-002 to 015: Draft (without READY_AT) - to have variety of states
INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-002', 'pat-002', 'shift-night', 'shift-day', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '1' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-003', 'pat-003', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '2' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-004', 'pat-004', 'shift-night', 'shift-day', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '3' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-005', 'pat-005', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '4' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-006', 'pat-006', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '5' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-007', 'pat-007', 'shift-night', 'shift-day', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '6' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-008', 'pat-008', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '7' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-009', 'pat-009', 'shift-night', 'shift-day', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '8' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-010', 'pat-010', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '9' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-011', 'pat-011', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '10' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-012', 'pat-012', 'shift-night', 'shift-day', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '11' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-013', 'pat-013', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '12' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-014', 'pat-014', 'shift-night', 'shift-day', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '13' DAY);

INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT)
VALUES ('handover-015', 'pat-015', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '14' DAY);

-- Additional handovers with different states
-- Handover handover-016 (Ready) - timestamps respect temporal order
-- CREATED_AT -> READY_AT (READY_AT >= CREATED_AT)
INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT, CREATED_AT, READY_AT)
VALUES ('handover-016', 'pat-001', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 
        LOCALTIMESTAMP - INTERVAL '3' HOUR, LOCALTIMESTAMP - INTERVAL '2' HOUR, LOCALTIMESTAMP - INTERVAL '1' HOUR);

-- Handover handover-017 (InProgress) - timestamps respect temporal order
-- CREATED_AT -> READY_AT -> STARTED_AT
INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT, CREATED_AT, READY_AT, STARTED_AT)
VALUES ('handover-017', 'pat-002', 'shift-night', 'shift-day', 'user_demo12345678901234567890123458', 'user_demo12345678901234567890123456', 
        LOCALTIMESTAMP - INTERVAL '4' HOUR, LOCALTIMESTAMP - INTERVAL '3' HOUR, LOCALTIMESTAMP - INTERVAL '2' HOUR, LOCALTIMESTAMP - INTERVAL '1' HOUR);

-- Seed data for HANDOVER_CONTENTS (replaces singleton tables)
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
    'Paciente estable, sin complicaciones. Buena respuesta al tratamiento antibiótico.', 
    'Continuar tratamiento actual. Alta probable en 48-72 horas si evolución favorable.',
    'Completed',
    'Completed',
    'Draft',
    'user_demo12345678901234567890123456'
);

-- Content for handover-016
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
    'handover-016', 
    'Unstable',
    'John Doe, M, 68. Antecedentes de HTA, DM2, y cardiopatia isquemica con FEVI 35%. Ingreso por SCA sin elevacion del ST hace 3 dias, manejado con AAS, clopidogrel, enoxaparina y estatinas. Coronariografia ayer mostro enfermedad de 3 vasos no revascularizable percutaneamente. Se discutio con cirugia cardiaca y se acepto para CRM programada. Durante la noche, presento episodio de DPN que respondio a furosemida IV. Actualmente con dolor toracico leve, intermitente.',
    'Paciente con enfermedad coronaria severa en espera de cirugia de revascularizacion. Requiere monitorizacion hemodinamica estricta y manejo de insuficiencia cardiaca congestiva. Alto riesgo de isquemia recurrente y arritmias ventriculares.',
    'Continuar tratamiento actual. Alta probable en 48-72 horas si evolucion favorable.',
    'Completed', 'Completed', 'Draft',
    'user_demo12345678901234567890123456'
);

-- Content for handover-017
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
    'handover-017', 
    'Watcher',
    'Jane Smith, F, 45. Antecedentes de cancer de mama estadio III, actualmente en quimioterapia adyuvante con paclitaxel. Presenta neutropenia grado 2, ultimo recuento de neutrofilos 1.2 x 10^9/L. Recibe GCSF profilactico. Durante la noche presento episodio de nauseas que respondio a ondansetron. Actualmente estable, sin fiebre.',
    'Paciente oncologica en tratamiento quimioterapico con riesgo de neutropenia febril. Requiere monitorizacion estrecha de signos vitales y recuentos hematologicos. Mantener precauciones de aislamiento por neutropenia.',
    'Continuar quimioterapia segun protocolo. Monitoreo hematologico estrecho.',
    'Completed', 'Completed', 'Draft',
    'user_demo12345678901234567890123458'
);

-- Insert handover action items
INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('action-001', 'handover-001', 'Realizar nebulizaciones cada 6 horas', 0);

INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('action-002', 'handover-001', 'Monitorear saturación de oxígeno', 0);

INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('action-003', 'handover-001', 'Control de temperatura cada 4 horas', 1);

-- Insert handover participants
INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, STATUS)
VALUES ('participant-001', 'handover-001', 'user_demo12345678901234567890123456', 'active');

INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, STATUS)
VALUES ('participant-002', 'handover-001', 'user_demo12345678901234567890123457', 'active');

INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, STATUS)
VALUES ('participant-003', 'handover-001', 'user_demo12345678901234567890123458', 'active');

-- Insert some sample discussion messages
INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT)
VALUES ('message-001', 'handover-001', 'user_demo12345678901234567890123456', 'Just reviewed the case. The heart failure seems stable today. Any concerns about the fluid balance?', 'message', LOCALTIMESTAMP - INTERVAL '5' MINUTE);

INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT)
VALUES ('message-002', 'handover-001', 'user_demo12345678901234567890123457', 'Patient has been net negative 500ml today. Responded well to the lasix adjustment this morning. Current weight is down 2kg from admission.', 'message', LOCALTIMESTAMP - INTERVAL '3' MINUTE);

INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT)
VALUES ('message-003', 'handover-001', 'user_demo12345678901234567890123458', 'Should we continue the current diuretic dose overnight? BUN/Cr stable at 1.2.', 'message', LOCALTIMESTAMP - INTERVAL '1' MINUTE);

INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT)
VALUES ('message-004', 'handover-001', 'user_demo12345678901234567890123456', 'Agreed, let''s maintain current dose and recheck labs tomorrow morning.', 'message', LOCALTIMESTAMP - INTERVAL '30' SECOND);

INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT)
VALUES ('message-005', 'handover-001', 'user_demo12345678901234567890123457', 'Family called - they want to discuss the discharge plan. Should I arrange a meeting for tomorrow?', 'message', LOCALTIMESTAMP - INTERVAL '15' SECOND);

-- ========================================
-- CONTINGENCY PLANNING FOR ALL PATIENTS
-- ========================================

-- Contingency planning for handover-001 (Patient with pneumonia - pat-001)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-001', 'handover-001', 'Si el paciente desarrolla dificultad respiratoria aguda o saturación de oxígeno < 92%', 'Administrar oxígeno suplementario, llamar a terapia respiratoria, considerar BIPAP, contactar al médico tratante inmediatamente', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-002', 'handover-001', 'Si la temperatura axilar supera los 38.5°C', 'Administrar antipiréticos según protocolo, evaluar foco infeccioso, contactar médico si persiste fiebre', 'high', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-003', 'handover-001', 'Si aparecen signos de insuficiencia respiratoria (taquipnea > 30/min, tiraje)', 'Aumentar FiO2, preparar para posible intubación, llamar a intensivista, contactar médico tratante', 'high', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-004', 'handover-001', 'Si el paciente presenta reacción alérgica a penicilina', 'Suspender antibiótico inmediatamente, administrar epinefrina si anafilaxia, contactar alergólogo', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '15' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-005', 'handover-001', 'Si no hay mejoría radiológica en 48-72 horas', 'Repetir radiografía de tórax, considerar cambio de antibiótico, consultar infectólogo', 'medium', 'planned', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '10' MINUTE);

-- Planificación de Contingencia para handover-002 (Paciente con sepsis - pat-002)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-006', 'handover-002', 'Si la presión arterial sistólica < 90 mmHg o requiere aumento de vasopresores', 'Aumentar fluidos IV, incrementar dosis de vasopresores, llamar a intensivista, monitoreo continuo', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-007', 'handover-002', 'Si el paciente presenta oliguria (< 1ml/kg/hora) o anuria', 'Evaluar estado de hidratación, ecografía renal, considerar catéter vesical, ajustar fluidos', 'high', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-008', 'handover-002', 'Si aparecen nuevos focos infecciosos o empeoramiento clínico', 'Cultivos adicionales, evaluación por imágenes, escalar antibióticos, consultar infectólogo', 'high', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-009', 'handover-002', 'Si el paciente presenta alteración del estado mental', 'Evaluar causas metabólicas, infección SNC, ajustar sedación, consultar neurología', 'medium', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '15' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-010', 'handover-002', 'Si persiste fiebre > 48 horas con tratamiento antibiótico', 'Repetir hemocultivos, evaluación de foco oculto, cambio empírico de antibióticos', 'medium', 'planned', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '10' MINUTE);

-- Planificación de Contingencia para handover-003 (Paciente con asma - pat-003)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-011', 'handover-003', 'Si el paciente presenta crisis asmática severa con pobre respuesta a tratamiento', 'Administrar adrenalina subcutánea, preparar para intubación, llamar a intensivista', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-012', 'handover-003', 'Si requiere oxígeno suplementario > 2L/min persistentemente', 'Aumentar dosis de corticoides, considerar terapia inhalatoria adicional, evaluación intensiva', 'high', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-013', 'handover-003', 'Si aparecen signos de fatiga muscular respiratoria', 'Preparar para ventilación no invasiva, evaluar necesidad de intubación, monitoreo continuo', 'high', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-014', 'handover-003', 'Si el paciente presenta taquicardia > 150/min persistente', 'Evaluar causa (hipoxemia, ansiedad, efectos medicamentosos), ajustar tratamiento', 'medium', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '15' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-015', 'handover-003', 'Si no hay mejoría clínica en 24 horas', 'Reevaluar tratamiento, considerar inmunomoduladores, consultar alergólogo/inmunólogo', 'medium', 'planned', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '10' MINUTE);

-- Planificación de Contingencia para handover-004 (Paciente con trauma craneoencefálico - pat-004)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-016', 'handover-004', 'Si disminución del nivel de conciencia (Glasgow < 12)', 'Evaluar urgencia neuroquirúrgica, tomografía cerebral inmediata, preparar para cirugía', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-017', 'handover-004', 'Si anisocoria o midriasis unilateral', 'Medición de presión intraocular, evaluación neuroquirúrgica urgente, preparar para cirugía', 'high', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-018', 'handover-004', 'Si convulsiones o movimientos anormales', 'Administrar anticonvulsivantes, evaluación neurológica, monitoreo EEG continuo', 'high', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-019', 'handover-004', 'Si hipertensión endocraneana (presión > 20 mmHg)', 'Aumentar dosis de manitol, hiperventilación, consultar neurocirugía para drenaje', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '15' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-020', 'handover-004', 'Si náuseas o vómitos incoercibles', 'Administrar antieméticos, evaluar aumento de presión endocraneana, monitoreo neurológico', 'medium', 'planned', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '10' MINUTE);

-- Planificación de Contingencia para handover-005 (Paciente con insuficiencia respiratoria - pat-005)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-021', 'handover-005', 'Si desaturación < 88% o desconexión accidental del ventilador', 'Reintubar inmediatamente, verificar parámetros ventilatorios, llamar a intensivista', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-022', 'handover-005', 'Si neumotórax o deterioro respiratorio agudo', 'Drenaje pleural urgente, ajuste ventilatorio, radiografía de tórax inmediata', 'high', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-023', 'handover-005', 'Si hemorragia por vía aérea artificial', 'Verificar posición del tubo, aspirar, evaluar sangrado, preparar cambio de tubo', 'high', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-024', 'handover-005', 'Si agitación o dolor no controlado con sedación actual', 'Aumentar dosis de sedantes, evaluar causa (neumotórax, etc.), consulta anestésica', 'medium', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '15' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-025', 'handover-005', 'Si infección nosocomial o neumonía asociada a ventilador', 'Cultivos bronquiales, antibióticos de amplio espectro, consultar infectólogo', 'medium', 'planned', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '10' MINUTE);

-- Planificación de Contingencia para handover-006 (Paciente con choque séptico - pat-006)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-026', 'handover-006', 'Si hipotensión refractaria (< 65 mmHg) a pesar de vasopresores', 'Escalar vasopresores, evaluar necesidad de balón intraaórtico, consultar cardiología', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-027', 'handover-006', 'Si anuria persistente > 6 horas', 'Iniciar hemodiálisis urgente, evaluar necesidad de catéter venoso central', 'high', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-028', 'handover-006', 'Si acidosis láctica > 4 mmol/L', 'Aumentar soporte ventilatorio, evaluar perfusión tisular, considerar bicarbonato', 'high', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-007 (Paciente con meningitis - pat-007)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-029', 'handover-007', 'Si deterioro neurológico agudo (disminución Glasgow > 2 puntos)', 'Repetir tomografía cerebral urgente, preparar para intervención neuroquirúrgica', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-030', 'handover-007', 'Si convulsiones recurrentes a pesar de tratamiento', 'Administrar fenitoína en carga, monitoreo EEG continuo, consultar neurología', 'high', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-031', 'handover-007', 'Si hipertensión endocraneana (presión > 20 mmHg)', 'Administrar manitol, hiperventilación controlada, preparar para drenaje ventricular', 'high', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-008 (Paciente con quemaduras - pat-008)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-032', 'handover-008', 'Si signos de infección en quemaduras (fiebre, eritema, secreción purulenta)', 'Cultivos locales, antibióticos intravenosos, evaluación quirúrgica urgente', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-033', 'handover-008', 'Si dolor incontrolable a pesar de analgesia multimodal', 'Consultar servicio de dolor, evaluar necesidad de sedación profunda', 'medium', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-034', 'handover-008', 'Si hipovolemia por pérdidas insensibles', 'Aumentar fluidos IV según fórmula de Parkland, monitoreo hemodinámico', 'medium', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-009 (Paciente con convulsiones - pat-009)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-035', 'handover-009', 'Si convulsión prolongada (> 5 minutos)', 'Administrar diazepam IV, preparar para intubación, monitoreo post-ictal', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-036', 'handover-009', 'Si estado post-ictal prolongado (> 30 minutos)', 'Evaluar causa metabólica, tomografía cerebral, consulta neurológica', 'medium', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-037', 'handover-009', 'Si recurrencia de convulsiones febriles', 'Repetir estudios etiológicos, considerar profilaxis antiepiléptica', 'medium', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-010 (Paciente con intoxicación - pat-010)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-038', 'handover-010', 'Si deterioro del nivel de conciencia', 'Evaluar Glasgow, soporte ventilatorio, antídoto específico según tóxico', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-039', 'handover-010', 'Si insuficiencia hepática aguda', 'Factor VII recombinante, plasma fresco congelado, consulta hepatología', 'high', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-040', 'handover-010', 'Si arritmias cardíacas por toxicidad', 'Antiarrítmicos específicos, monitoreo continuo, consulta cardiología', 'high', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-011 (Paciente con hipoglucemia - pat-011)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-041', 'handover-011', 'Si hipoglucemia recurrente (< 60 mg/dL)', 'Ajustar esquema insulínico, evaluar adherencia, consulta endocrinología', 'medium', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-042', 'handover-011', 'Si cetosis o cetoacidosis', 'Insulina intravenosa continua, fluidos, monitoreo gasométrico frecuente', 'high', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-043', 'handover-011', 'Si convulsiones por hipoglucemia', 'Glucosa hipertónica IV, benzodiazepinas si convulsiones, evaluación neurológica', 'high', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-012 (Paciente con trauma abdominal - pat-012)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-044', 'handover-012', 'Si signos de peritonitis (rigidez abdominal, rebote)', 'Cirugía urgente, antibióticos de amplio espectro, soporte hemodinámico', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-045', 'handover-012', 'Si hipotensión por sangrado interno', 'Fluidos cristaloides, sangre tipo específico, evaluación radiológica urgente', 'high', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-046', 'handover-012', 'Si dolor abdominal incontrolable', 'Analgesia multimodal, evaluación por dolor agudo, consulta cirugía', 'medium', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-013 (Paciente con bronquiolitis - pat-013)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-047', 'handover-013', 'Si insuficiencia respiratoria progresiva', 'Oxígeno suplementario, preparar para ventilación no invasiva, consulta intensivista', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-048', 'handover-013', 'Si deshidratación por dificultad respiratoria', 'Fluidos IV, monitoreo electrolitos, ajuste respiratorio para minimizar trabajo', 'medium', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-049', 'handover-013', 'Si apnea o bradicardia', 'Estimulación, ventilación con ambú, preparar para intubación', 'high', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-014 (Paciente con gastroenteritis - pat-014)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-050', 'handover-014', 'Si deshidratación severa (pérdida > 10% peso)', 'Fluidos IV isotónicos, monitoreo electrolitos, evaluación cardíaca', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-051', 'handover-014', 'Si vómitos incoercibles a pesar de ondansetrón', 'Antieméticos parenterales, evaluación causa, posible sonda nasogástrica', 'medium', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-052', 'handover-014', 'Si sangre en deposiciones', 'Evaluación endoscópica urgente, antibióticos, monitoreo hemodinámico', 'high', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-015 (Paciente con otitis media - pat-015)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-053', 'handover-015', 'Si complicación intracraneal (meningitis, absceso)', 'Antibióticos intravenosos de amplio espectro, evaluación neurológica urgente', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-054', 'handover-015', 'Si mastoiditis aguda', 'Antibióticos intravenosos, evaluación otorrinolaringológica urgente', 'high', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-055', 'handover-015', 'Si dolor persistente a pesar de analgesia', 'Reevaluar causa, posible paracentesis, consulta otorrinolaringología', 'medium', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificacion de Contingencia para handover-016
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-092', 'handover-016', 'Si el paciente desarrolla dificultad respiratoria aguda o saturacion de oxigeno < 92%', 'Administrar oxigeno suplementario, llamar a terapia respiratoria, considerar BIPAP, contactar al medico tratante inmediatamente', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-093', 'handover-016', 'Si la temperatura axilar supera los 38.5C', 'Administrar antipireticos segun protocolo, evaluar foco infeccioso, contactar medico si persiste fiebre', 'high', 'active', 'user_demo12345678901234567890123457', LOCALTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-094', 'handover-016', 'Si aparecen signos de insuficiencia respiratoria (taquipnea > 30/min, tiraje)', 'Aumentar FiO2, preparar para posible intubacion, llamar a intensivista, contactar medico tratante', 'high', 'planned', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '20' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-095', 'handover-016', 'Si el paciente presenta dolor toracico > 5/10 no controlado con NTG', 'Realizar EKG de 12 derivaciones, administrar morfina, y notificar a cardiologia de guardia inmediatamente.', 'high', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '15' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-096', 'handover-017', 'Si la paciente presenta fiebre neutropenica (T > 38.3C o > 38.0C por 1h)', 'Iniciar protocolo de sepsis, tomar hemocultivos y urocultivo, administrar antibioticos de amplio espectro en la primera hora', 'high', 'active', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '10' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-097', 'handover-017', 'Si el recuento de plaquetas cae por debajo de 20,000/uL', 'Preparar para transfusion de plaquetas, evitar procedimientos invasivos, monitorizar signos de sangrado', 'high', 'active', 'user_demo12345678901234567890123458', LOCALTIMESTAMP - INTERVAL '5' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-098', 'handover-017', 'Si la paciente presenta nauseas y vomitos incontrolables (>3 episodios en 1h)', 'Administrar antiemeticos de rescate (e.g., olanzapina), asegurar hidratacion IV, consultar con farmacia clinica', 'medium', 'active', 'user_demo12345678901234567890123456', LOCALTIMESTAMP - INTERVAL '3' MINUTE);

COMMIT;
