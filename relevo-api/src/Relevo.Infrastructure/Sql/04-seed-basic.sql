-- ========================================
-- INSERTAR DATOS DE SEMILLA
-- ========================================

-- Insertar Unidades
INSERT INTO UNITS (ID, NAME, DESCRIPTION) VALUES ('unit-1', 'UCI', 'Unidad de Cuidados Intensivos');
INSERT INTO UNITS (ID, NAME, DESCRIPTION) VALUES ('unit-2', 'Pediatría General', 'Unidad de Pediatría General');
INSERT INTO UNITS (ID, NAME, DESCRIPTION) VALUES ('unit-3', 'Pediatría Especializada', 'Unidad de Pediatría Especializada');

-- Insertar Turnos
INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME) VALUES ('shift-day', 'Mañana', '07:00', '15:00');
INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME) VALUES ('shift-night', 'Noche', '19:00', '07:00');

-- Insertar Pacientes para Unidad 1 (UCI) - 12 pacientes
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-001', 'María García', 'unit-1', TO_DATE('2010-03-15', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 3, '201', 'Neumonía adquirida en comunidad', 'Penicilina', 'Amoxicilina, Oxígeno suplementario', 'Paciente estable, saturación de oxígeno 94%, requiere nebulizaciones cada 6 horas', 'MRN001234');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-002', 'Carlos Rodríguez', 'unit-1', TO_DATE('2008-07-22', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 5, '202', 'Sepsis secundaria a infección urinaria', 'Sulfonamidas', 'Meropenem, Vasopresores', 'Paciente crítico, requiere monitoreo continuo de signos vitales y soporte hemodinámico', 'MRN001235');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-003', 'Ana López', 'unit-1', TO_DATE('2012-11-08', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 2, '203', 'Estado asmático agudo', 'Ninguna', 'Salbutamol, Corticoides intravenosos', 'Paciente con mejoría progresiva, disminución en requerimiento de oxígeno', 'MRN001236');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-004', 'Miguel Hernández', 'unit-1', TO_DATE('2009-05-30', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 7, '204', 'Trauma craneoencefálico moderado', 'Látex', 'Manitol, Analgésicos', 'Glasgow 12/15, pupilas isocóricas, requiere monitoreo neurológico frecuente', 'MRN001237');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-005', 'Isabella González', 'unit-1', TO_DATE('2011-09-14', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 4, '205', 'Insuficiencia respiratoria aguda', 'Iodo', 'Ventilación mecánica, Sedantes', 'Paciente intubado, parámetros ventilatorios estables', 'MRN001238');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-006', 'David Pérez', 'unit-1', TO_DATE('2013-01-25', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 6, '206', 'Choque séptico', 'Ninguna', 'Antibióticos de amplio espectro, Fluidos', 'Paciente en shock distributivo, requiere soporte vasopresor', 'MRN001239');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-007', 'Sofia Martínez', 'unit-1', TO_DATE('2007-12-03', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 1, '207', 'Meningitis bacteriana', 'Penicilina', 'Ceftriaxona, Dexametasona', 'Paciente con mejoría clínica, cultivos pendientes', 'MRN001240');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-008', 'José Sánchez', 'unit-1', TO_DATE('2014-06-18', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 8, '208', 'Quemaduras de segundo grado', 'Ninguna', 'Analgésicos, Antibióticos tópicos', 'Quemaduras en 25% superficie corporal, requiere curas diarias', 'MRN001241');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-009', 'Carmen Díaz', 'unit-1', TO_DATE('2010-08-12', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 9, '209', 'Convulsiones febriles', 'Ninguna', 'Antiepilépticos, Antipiréticos', 'Paciente estable, sin recurrencia de convulsiones', 'MRN001242');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-010', 'Antonio Moreno', 'unit-1', TO_DATE('2006-04-07', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 12, '210', 'Intoxicación medicamentosa', 'Aspirina', 'Carbón activado, Soporte vital', 'Paciente estabilizado, requiere monitoreo de función hepática', 'MRN001243');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-011', 'Elena Jiménez', 'unit-1', TO_DATE('2012-02-28', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 3, '211', 'Hipoglucemia severa', 'Ninguna', 'Glucosa intravenosa, Insulina', 'Episodio resuelto, requiere educación diabética', 'MRN001244');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-012', 'Francisco Ruiz', 'unit-1', TO_DATE('2008-10-19', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 5, '212', 'Trauma abdominal', 'Ninguna', 'Analgésicos, Antibióticos profilácticos', 'Paciente estable, sin signos de peritonitis', 'MRN001245');
-- Insertar Pacientes para Unidad 2 (Pediatría General) - 12 pacientes
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-013', 'Lucía Álvarez', 'unit-2', TO_DATE('2015-03-12', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 2, '301', 'Bronquiolitis', 'Ninguna', 'Salbutamol, Hidratación', 'Paciente con mejoría respiratoria, buena respuesta al tratamiento', 'MRN001246');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-014', 'Pablo Romero', 'unit-2', TO_DATE('2011-07-08', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 1, '302', 'Gastroenteritis aguda', 'Ninguna', 'Rehidratación oral, Ondansetrón', 'Paciente con buena tolerancia oral, sin vómitos', 'MRN001247');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-015', 'Valentina Navarro', 'unit-2', TO_DATE('2013-11-25', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 3, '303', 'Otitis media aguda', 'Amoxicilina', 'Amoxicilina oral, Analgésicos', 'Paciente afebril, disminución del dolor otológico', 'MRN001248');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-016', 'Diego Torres', 'unit-2', TO_DATE('2009-09-14', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 4, '304', 'Neumonía adquirida en comunidad', 'Ninguna', 'Amoxicilina, Broncodilatadores', 'Paciente afebril, mejoría radiológica en progreso', 'MRN001249');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-017', 'Marta Ramírez', 'unit-2', TO_DATE('2014-05-30', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 2, '305', 'Infección urinaria', 'Sulfonamidas', 'Cefuroxima, Analgésicos', 'Urocultivo positivo, tratamiento dirigido iniciado', 'MRN001250');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-018', 'Adrián Gil', 'unit-2', TO_DATE('2010-12-03', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 5, '306', 'Fractura de antebrazo', 'Codeína', 'Ibuprofeno, Inmovilización', 'Fractura simple, alineación adecuada lograda', 'MRN001251');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-019', 'Clara Serrano', 'unit-2', TO_DATE('2012-08-17', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 1, '307', 'Varicela', 'Ninguna', 'Antihistamínicos, Antipiréticos', 'Lesiones en fase de costra, prurito controlado', 'MRN001252');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-020', 'Hugo Castro', 'unit-2', TO_DATE('2016-01-22', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 3, '308', 'Deshidratación moderada', 'Ninguna', 'Rehidratación intravenosa', 'Paciente hidratado, buena diuresis', 'MRN001253');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-021', 'Natalia Rubio', 'unit-2', TO_DATE('2013-04-09', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 6, '309', 'Apendicitis aguda', 'Ninguna', 'Antibióticos, Analgésicos', 'Paciente postoperatorio, evolución favorable', 'MRN001254');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-022', 'Iván Ortega', 'unit-2', TO_DATE('2008-11-14', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 2, '310', 'Asma agudizada', 'Ninguna', 'Salbutamol, Corticoides', 'Paciente con buen control sintomático', 'MRN001255');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-023', 'Paula Delgado', 'unit-2', TO_DATE('2011-06-27', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 7, '311', 'Faringoamigdalitis', 'Penicilina', 'Azitromicina, Analgésicos', 'Paciente afebril, mejoría de odinofagia', 'MRN001256');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-024', 'Mario Guerrero', 'unit-2', TO_DATE('2014-02-11', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 4, '312', 'Traumatismo craneoencefálico leve', 'Ninguna', 'Analgésicos, Observación', 'Glasgow 15/15, paciente asintomático', 'MRN001257');
-- Insertar Pacientes para Unidad 3 (Pediatría Especializada) - 11 pacientes
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-025', 'Laura Flores', 'unit-3', TO_DATE('2009-07-05', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 10, '401', 'Cardiopatía congénita', 'Ninguna', 'Digoxina, Diuréticos', 'Paciente compensado, requiere seguimiento cardiológico', 'MRN001258');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-026', 'Álvaro Vargas', 'unit-3', TO_DATE('2010-12-18', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 8, '402', 'Diabetes mellitus tipo 1', 'Ninguna', 'Insulina, Control glucémico', 'Buen control metabólico, educación diabética en progreso', 'MRN001259');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-027', 'Cristina Medina', 'unit-3', TO_DATE('2007-09-23', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 15, '403', 'Fibrosis quística', 'Ninguna', 'Antibióticos, Enzimas pancreáticas', 'Paciente estable, requiere fisioterapia respiratoria', 'MRN001260');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-028', 'Sergio Herrera', 'unit-3', TO_DATE('2012-04-30', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 6, '404', 'Trastorno del espectro autista', 'Ninguna', 'No farmacológico', 'Paciente en programa de intervención temprana', 'MRN001261');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-029', 'Alicia Castro', 'unit-3', TO_DATE('2011-08-14', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 12, '405', 'Epilepsia refractaria', 'Ninguna', 'Antiepilépticos múltiples', 'Paciente con control parcial de convulsiones', 'MRN001262');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-030', 'Roberto Vega', 'unit-3', TO_DATE('2008-03-07', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 20, '406', 'Leucemia linfoblástica aguda', 'Ninguna', 'Quimioterapia, Soporte', 'Paciente en protocolo de tratamiento oncológico', 'MRN001263');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-031', 'Beatriz León', 'unit-3', TO_DATE('2013-11-29', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 9, '407', 'Síndrome de Down con cardiopatía', 'Ninguna', 'Medicamentos cardíacos', 'Paciente compensado hemodinámicamente', 'MRN001264');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-032', 'Manuel Peña', 'unit-3', TO_DATE('2006-06-12', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 25, '408', 'Parálisis cerebral', 'Ninguna', 'Antiespásticos, Fisioterapia', 'Paciente en programa de rehabilitación intensiva', 'MRN001265');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-033', 'Silvia Cortés', 'unit-3', TO_DATE('2014-01-16', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 7, '409', 'Prematuridad extrema', 'Ninguna', 'Nutrición especializada, Soporte respiratorio', 'Paciente en seguimiento de desarrollo neurológico', 'MRN001266');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-034', 'Fernando Aguilar', 'unit-3', TO_DATE('2010-05-03', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 11, '410', 'Trastorno de déficit de atención', 'Ninguna', 'Estimulantes, Terapia conductual', 'Paciente con buena respuesta al tratamiento', 'MRN001267');
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES, MRN)
VALUES ('pat-035', 'Teresa Santana', 'unit-3', TO_DATE('2009-10-25', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 14, '411', 'Talasemia mayor', 'Ninguna', 'Transfusiones, Quelantes', 'Paciente en programa de trasfusiones crónicas', 'MRN001268');
-- Insertar asignaciones de usuario-paciente-turno (para crear handovers)
INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
VALUES ('assign-001', 'user_demo12345678901234567890123456', 'shift-day', 'pat-001', SYSTIMESTAMP);

INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
VALUES ('assign-002', 'user_demo12345678901234567890123456', 'shift-night', 'pat-002', SYSTIMESTAMP - 1);

INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
VALUES ('assign-003', 'user_demo12345678901234567890123456', 'shift-day', 'pat-003', SYSTIMESTAMP - 2);

INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
VALUES ('assign-004', 'user_demo12345678901234567890123456', 'shift-night', 'pat-004', SYSTIMESTAMP - 3);

INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
VALUES ('assign-005', 'user_demo12345678901234567890123456', 'shift-day', 'pat-005', SYSTIMESTAMP - 4);

-- Insertar colaboradores de ejemplo
INSERT INTO CONTRIBUTORS (ID, NAME, EMAIL, PHONE_NUMBER) VALUES (1, 'Dra. María García', 'maria.garcia@hospital.com.ar', '+54-11-555-0123');
INSERT INTO CONTRIBUTORS (ID, NAME, EMAIL, PHONE_NUMBER) VALUES (2, 'Dr. Carlos López', 'carlos.lopez@hospital.com.ar', '+54-11-555-0124');
INSERT INTO CONTRIBUTORS (ID, NAME, EMAIL, PHONE_NUMBER) VALUES (3, 'Dra. Ana Martínez', 'ana.martinez@hospital.com.ar', '+54-11-555-0125');

-- Insertar usuarios de ejemplo
INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, AVATAR_URL, ROLE, IS_ACTIVE, LAST_LOGIN)
VALUES ('user_demo12345678901234567890123456', 'dr.johnson@hospital.com', 'John', 'Johnson', 'Dr. John Johnson', 'https://example.com/avatar1.jpg', 'doctor', 1, SYSTIMESTAMP);

INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, AVATAR_URL, ROLE, IS_ACTIVE, LAST_LOGIN)
VALUES ('user_demo12345678901234567890123457', 'dr.patel@hospital.com', 'Priya', 'Patel', 'Dr. Priya Patel', 'https://example.com/avatar2.jpg', 'doctor', 1, SYSTIMESTAMP);

INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, AVATAR_URL, ROLE, IS_ACTIVE, LAST_LOGIN)
VALUES ('user_demo12345678901234567890123458', 'dr.martinez@hospital.com', 'Carlos', 'Martinez', 'Dr. Carlos Martinez', 'https://example.com/avatar3.jpg', 'doctor', 1, SYSTIMESTAMP);

-- Insertar preferencias de usuario
INSERT INTO USER_PREFERENCES (ID, USER_ID, THEME, LANGUAGE, TIMEZONE, NOTIFICATIONS_ENABLED, AUTO_SAVE_ENABLED)
VALUES ('pref-001', 'user_demo12345678901234567890123456', 'light', 'en', 'America/New_York', 1, 1);

INSERT INTO USER_PREFERENCES (ID, USER_ID, THEME, LANGUAGE, TIMEZONE, NOTIFICATIONS_ENABLED, AUTO_SAVE_ENABLED)
VALUES ('pref-002', 'user_demo12345678901234567890123457', 'dark', 'en', 'America/New_York', 1, 1);

INSERT INTO USER_PREFERENCES (ID, USER_ID, THEME, LANGUAGE, TIMEZONE, NOTIFICATIONS_ENABLED, AUTO_SAVE_ENABLED)
VALUES ('pref-003', 'user_demo12345678901234567890123458', 'light', 'es', 'America/New_York', 1, 1);

-- Insertar sesiones de usuario activas
INSERT INTO USER_SESSIONS (ID, USER_ID, IP_ADDRESS, USER_AGENT, IS_ACTIVE)
VALUES ('session-001', 'user_demo12345678901234567890123456', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', 1);

INSERT INTO USER_SESSIONS (ID, USER_ID, IP_ADDRESS, USER_AGENT, IS_ACTIVE)
VALUES ('session-002', 'user_demo12345678901234567890123457', '192.168.1.101', 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36', 1);

COMMIT;

-- Insertar handover de ejemplo
INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-001', 'assign-001', 'pat-001', 'Ready', 'Stable',
        'Paciente de 14 años con neumonía adquirida en comunidad. Estable, saturación de oxígeno 94%, requiere nebulizaciones cada 6 horas.',
        'doc-001',
        'Paciente estable, evolución favorable. Continuar tratamiento actual.',
        'Mañana → Noche', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 'user_demo12345678901234567890123456', TRUNC(SYSDATE), SYSTIMESTAMP - INTERVAL '30' MINUTE);

-- Insertar handovers adicionales para todos los pacientes
INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-002', 'assign-002', 'pat-002', 'Ready', 'Critical',
        'Paciente de 12 años con sepsis secundaria a infección urinaria. Paciente crítico, requiere monitoreo continuo.',
        'doc-002',
        'Paciente en estado crítico. Continuar soporte vasopresor y antibióticos.',
        'Noche → Mañana', 'shift-night', 'shift-day', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 1, SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-003', 'assign-003', 'pat-003', 'Ready', 'Unstable',
        'Paciente de 13 años con estado asmático agudo. Mejoría progresiva, disminución en requerimiento de oxígeno.',
        'doc-003',
        'Mejoría respiratoria evidente. Continuar tratamiento actual con reducción gradual.',
        'Mañana → Noche', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 2, SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-004', 'assign-004', 'pat-004', 'Ready', 'Unstable',
        'Paciente de 13 años con trauma craneoencefálico moderado. Glasgow 12/15, requiere monitoreo neurológico.',
        'doc-004',
        'Estable neurológicamente. Continuar monitoreo frecuente y evaluación neuroquirúrgica.',
        'Noche → Mañana', 'shift-night', 'shift-day', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 3, SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-005', 'assign-005', 'pat-005', 'Ready', 'Critical',
        'Paciente de 13 años con insuficiencia respiratoria aguda. Ventilación mecánica invasiva.',
        'doc-005',
        'Parámetros ventilatorios estables. Continuar sedación y monitoreo continuo.',
        'Mañana → Noche', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 4, SYSTIMESTAMP - INTERVAL '30' MINUTE);

-- Crear handovers adicionales para todos los pacientes restantes
INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-006', 'assign-001', 'pat-006', 'Ready', 'Critical',
        'Paciente de 11 años con choque séptico. Antibióticos de amplio espectro y soporte hemodinámico.',
        'doc-006',
        'Paciente en shock distributivo, requiere soporte vasopresor continuo.',
        'Mañana → Noche', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 5, SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-007', 'assign-002', 'pat-007', 'Ready', 'Unstable',
        'Paciente de 16 años con meningitis bacteriana. Ceftriaxona y dexametasona.',
        'doc-007',
        'Paciente con mejoría clínica, cultivos pendientes de resultado.',
        'Noche → Mañana', 'shift-night', 'shift-day', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 6, SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-008', 'assign-003', 'pat-008', 'Ready', 'Unstable',
        'Paciente de 10 años con quemaduras de segundo grado en 25% de superficie corporal.',
        'doc-008',
        'Quemaduras extensas requieren curas diarias y analgesia adecuada.',
        'Mañana → Noche', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 7, SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-009', 'assign-004', 'pat-009', 'Ready', 'Stable',
        'Paciente de 14 años con convulsiones febriles. Antiepilépticos y antipiréticos.',
        'doc-009',
        'Paciente estable, sin recurrencia de convulsiones.',
        'Noche → Mañana', 'shift-night', 'shift-day', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 8, SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-010', 'assign-005', 'pat-010', 'Ready', 'Unstable',
        'Paciente de 17 años con intoxicación medicamentosa. Carbón activado y soporte vital.',
        'doc-010',
        'Paciente estabilizado, requiere monitoreo de función hepática.',
        'Mañana → Noche', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 9, SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-011', 'assign-001', 'pat-011', 'Ready', 'Stable',
        'Paciente de 13 años con hipoglucemia severa. Glucosa intravenosa e insulina.',
        'doc-011',
        'Episodio resuelto, requiere educación diabética.',
        'Mañana → Noche', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 10, SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-012', 'assign-002', 'pat-012', 'Ready', 'Stable',
        'Paciente de 12 años con trauma abdominal. Analgésicos y antibióticos profilácticos.',
        'doc-012',
        'Paciente estable, sin signos de peritonitis.',
        'Noche → Mañana', 'shift-night', 'shift-day', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 11, SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-013', 'assign-003', 'pat-013', 'Ready', 'Stable',
        'Paciente de 8 años con bronquiolitis. Salbutamol y hidratación.',
        'doc-013',
        'Paciente con mejoría respiratoria, buena respuesta al tratamiento.',
        'Mañana → Noche', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 12, SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-014', 'assign-004', 'pat-014', 'Ready', 'Stable',
        'Paciente de 15 años con apendicitis aguda. Preparado para apendicectomía.',
        'doc-014',
        'Paciente en espera de cirugía, mantener en ayunas.',
        'Noche → Mañana', 'shift-night', 'shift-day', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 13, SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('handover-015', 'assign-005', 'pat-015', 'Ready', 'Stable',
        'Paciente de 9 años con fractura de fémur. Tracción esquelética.',
        'doc-015',
        'Paciente con buen control del dolor, requiere vigilancia neurovascular.',
        'Mañana → Noche', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 'user_demo12345678901234567890123456', TRUNC(SYSDATE) - 14, SYSTIMESTAMP - INTERVAL '30' MINUTE);

COMMIT;

-- Insertar action items del handover
INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('action-001', 'handover-001', 'Realizar nebulizaciones cada 6 horas', 0);

INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('action-002', 'handover-001', 'Monitorear saturación de oxígeno', 0);

INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('action-003', 'handover-001', 'Control de temperatura cada 4 horas', 1);

-- Insertar participantes del handover
INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, USER_NAME, USER_ROLE, STATUS)
VALUES ('participant-001', 'handover-001', 'user_demo12345678901234567890123456', 'Dr. John Johnson', 'Day Attending', 'active');

INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, USER_NAME, USER_ROLE, STATUS)
VALUES ('participant-002', 'handover-001', 'user_demo12345678901234567890123457', 'Dr. Priya Patel', 'Evening Attending', 'active');

INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, USER_NAME, USER_ROLE, STATUS)
VALUES ('participant-003', 'handover-001', 'user_demo12345678901234567890123458', 'Dr. Carlos Martinez', 'Resident', 'active');

-- Insertar secciones I-PASS del handover
INSERT INTO HANDOVER_SECTIONS (ID, HANDOVER_ID, SECTION_TYPE, CONTENT, STATUS, LAST_EDITED_BY)
VALUES ('section-001', 'handover-001', 'illness_severity', 'Stable - Paciente con evolución favorable', 'completed', 'user_demo12345678901234567890123456');

INSERT INTO HANDOVER_SECTIONS (ID, HANDOVER_ID, SECTION_TYPE, CONTENT, STATUS, LAST_EDITED_BY)
VALUES ('section-002', 'handover-001', 'patient_summary',
        'María García, 14 años, neumonía adquirida en comunidad. Ingreso hace 3 días. Tratamiento con Amoxicilina y oxígeno suplementario.',
        'completed', 'user_demo12345678901234567890123456');

INSERT INTO HANDOVER_SECTIONS (ID, HANDOVER_ID, SECTION_TYPE, CONTENT, STATUS, LAST_EDITED_BY)
VALUES ('section-003', 'handover-001', 'action_items',
        'Nebulizaciones cada 6 horas, monitoreo de saturación de oxígeno, control de temperatura cada 4 horas',
        'in_progress', 'user_demo12345678901234567890123456');

INSERT INTO HANDOVER_SECTIONS (ID, HANDOVER_ID, SECTION_TYPE, CONTENT, STATUS, LAST_EDITED_BY)
VALUES ('section-004', 'handover-001', 'situation_awareness',
        'Paciente estable, sin complicaciones. Buena respuesta al tratamiento antibiótico.',
        'completed', 'user_demo12345678901234567890123456');

INSERT INTO HANDOVER_SECTIONS (ID, HANDOVER_ID, SECTION_TYPE, CONTENT, STATUS, LAST_EDITED_BY)
VALUES ('section-005', 'handover-001', 'synthesis',
        'Continuar tratamiento actual. Alta probable en 48-72 horas si evolución favorable.',
        'draft', 'user_demo12345678901234567890123456');

-- Insertar estado de sincronización
INSERT INTO HANDOVER_SYNC_STATUS (ID, HANDOVER_ID, USER_ID, SYNC_STATUS, VERSION)
VALUES ('sync-001', 'handover-001', 'user_demo12345678901234567890123456', 'synced', 1);

INSERT INTO HANDOVER_SYNC_STATUS (ID, HANDOVER_ID, USER_ID, SYNC_STATUS, VERSION)
VALUES ('sync-002', 'handover-001', 'user_demo12345678901234567890123457', 'syncing', 1);

-- Insertar plantillas I-PASS
INSERT INTO IPASS_TEMPLATES (ID, TEMPLATE_ID, SECTION, TITLE, TEMPLATE_CONTENT, IS_ACTIVE)
VALUES ('template-001', 'illness-update', 'illness', 'Illness Severity Update', 'Patient condition: [condition]\nVital signs: [vitals]\nRecent changes: [changes]\nConcerns: [concerns]', 1);

INSERT INTO IPASS_TEMPLATES (ID, TEMPLATE_ID, SECTION, TITLE, TEMPLATE_CONTENT, IS_ACTIVE)
VALUES ('template-002', 'patient-summary', 'patient', 'Patient Summary Template', 'Patient: [name], [age] years old\nAdmission: [date]\nDiagnosis: [diagnosis]\nCurrent status: [status]', 1);

INSERT INTO IPASS_TEMPLATES (ID, TEMPLATE_ID, SECTION, TITLE, TEMPLATE_CONTENT, IS_ACTIVE)
VALUES ('template-003', 'action-item', 'actions', 'Action Item Template', 'Action: [action]\nPriority: [priority]\nDue: [due_date]\nResponsible: [person]', 1);

INSERT INTO IPASS_TEMPLATES (ID, TEMPLATE_ID, SECTION, TITLE, TEMPLATE_CONTENT, IS_ACTIVE)
VALUES ('template-004', 'situation-awareness', 'awareness', 'Situation Awareness Template', 'Current situation: [situation]\nTeam awareness: [awareness]\nContingency plans: [plans]', 1);

INSERT INTO IPASS_TEMPLATES (ID, TEMPLATE_ID, SECTION, TITLE, TEMPLATE_CONTENT, IS_ACTIVE)
VALUES ('template-005', 'synthesis-note', 'synthesis', 'Synthesis Note Template', 'Key takeaways: [takeaways]\nHandover to: [next_team]\nCritical information: [critical]', 1);

-- Insertar plantillas de secciones
INSERT INTO SECTION_TEMPLATES (ID, SECTION_TYPE, TEMPLATE_NAME, TEMPLATE_CONTENT, IS_DEFAULT)
VALUES ('section-template-001', 'patient_summary', 'Standard Patient Summary',
'Patient Summary:
• Name: [patient_name]
• Age: [patient_age]
• Diagnosis: [diagnosis]
• Current medications: [medications]
• Allergies: [allergies]
• Recent vital signs: [vitals]
• Key concerns: [concerns]', 1);

INSERT INTO SECTION_TEMPLATES (ID, SECTION_TYPE, TEMPLATE_NAME, TEMPLATE_CONTENT, IS_DEFAULT)
VALUES ('section-template-002', 'situation_awareness', 'Standard Situation Awareness',
'Current Situation:
• Patient location: [location]
• Current interventions: [interventions]
• Response to treatment: [response]
• Team communication: [communication]
• Equipment needs: [equipment]
• Family involvement: [family]', 1);

INSERT INTO SECTION_TEMPLATES (ID, SECTION_TYPE, TEMPLATE_NAME, TEMPLATE_CONTENT, IS_DEFAULT)
VALUES ('section-template-003', 'synthesis', 'Standard Synthesis',
'Handover Synthesis:
• Key clinical decisions: [decisions]
• Outstanding tasks: [tasks]
• Follow-up requirements: [followup]
• Communication with family: [family_comm]
• Discharge planning: [discharge]
• Next steps: [next_steps]', 1);

-- Insertar elementos de lista de verificación de confirmación por defecto
INSERT INTO HANDOVER_CHECKLISTS (ID, HANDOVER_ID, USER_ID, ITEM_ID, ITEM_CATEGORY, ITEM_LABEL, ITEM_DESCRIPTION, IS_REQUIRED, IS_CHECKED)
VALUES ('checklist-001', 'handover-001', 'user_demo12345678901234567890123456', 'clinical-status', 'Clinical Status', 'Review clinical data', 'I have reviewed all clinical data and understand the patient''s current condition', 1, 0);

INSERT INTO HANDOVER_CHECKLISTS (ID, HANDOVER_ID, USER_ID, ITEM_ID, ITEM_CATEGORY, ITEM_LABEL, ITEM_DESCRIPTION, IS_REQUIRED, IS_CHECKED)
VALUES ('checklist-002', 'handover-001', 'user_demo12345678901234567890123456', 'medications', 'Medications', 'Understand medications', 'I understand all current medications, dosages, and administration schedules', 1, 0);

INSERT INTO HANDOVER_CHECKLISTS (ID, HANDOVER_ID, USER_ID, ITEM_ID, ITEM_CATEGORY, ITEM_LABEL, ITEM_DESCRIPTION, IS_REQUIRED, IS_CHECKED)
VALUES ('checklist-003', 'handover-001', 'user_demo12345678901234567890123456', 'allergies', 'Safety', 'Note allergies', 'I am aware of all patient allergies and have reviewed allergy protocols', 1, 0);

INSERT INTO HANDOVER_CHECKLISTS (ID, HANDOVER_ID, USER_ID, ITEM_ID, ITEM_CATEGORY, ITEM_LABEL, ITEM_DESCRIPTION, IS_REQUIRED, IS_CHECKED)
VALUES ('checklist-004', 'handover-001', 'user_demo12345678901234567890123456', 'priorities', 'Care Plan', 'Understand priorities', 'I understand the care priorities and treatment plan for this shift', 1, 0);

INSERT INTO HANDOVER_CHECKLISTS (ID, HANDOVER_ID, USER_ID, ITEM_ID, ITEM_CATEGORY, ITEM_LABEL, ITEM_DESCRIPTION, IS_REQUIRED, IS_CHECKED)
VALUES ('checklist-005', 'handover-001', 'user_demo12345678901234567890123456', 'contingency', 'Contingency', 'Aware of contingency plans', 'I am aware of all contingency plans and emergency protocols', 1, 0);

INSERT INTO HANDOVER_CHECKLISTS (ID, HANDOVER_ID, USER_ID, ITEM_ID, ITEM_CATEGORY, ITEM_LABEL, ITEM_DESCRIPTION, IS_REQUIRED, IS_CHECKED)
VALUES ('checklist-006', 'handover-001', 'user_demo12345678901234567890123456', 'communication', 'Communication', 'Know who to contact', 'I know who to contact for questions or concerns during this shift', 1, 0);

-- Insertar algunos mensajes de discusión de ejemplo
INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT)
VALUES ('message-001', 'handover-001', 'user_demo12345678901234567890123456', 'Just reviewed the case. The heart failure seems stable today. Any concerns about the fluid balance?', 'message', SYSTIMESTAMP - INTERVAL '5' MINUTE);

INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT)
VALUES ('message-002', 'handover-001', 'user_demo12345678901234567890123457', 'Patient has been net negative 500ml today. Responded well to the lasix adjustment this morning. Current weight is down 2kg from admission.', 'message', SYSTIMESTAMP - INTERVAL '3' MINUTE);

INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT)
VALUES ('message-003', 'handover-001', 'user_demo12345678901234567890123458', 'Should we continue the current diuretic dose overnight? BUN/Cr stable at 1.2.', 'message', SYSTIMESTAMP - INTERVAL '1' MINUTE);

INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT)
VALUES ('message-004', 'handover-001', 'user_demo12345678901234567890123456', 'Agreed, let''s maintain current dose and recheck labs tomorrow morning.', 'message', SYSTIMESTAMP - INTERVAL '30' SECOND);

INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT)
VALUES ('message-005', 'handover-001', 'user_demo12345678901234567890123457', 'Family called - they want to discuss the discharge plan. Should I arrange a meeting for tomorrow?', 'message', SYSTIMESTAMP - INTERVAL '15' SECOND);

