-- ESQUEMA DE BASE DE DATOS RELEVO Y DATOS DE SEMILLA
-- Este archivo contiene todas las tablas y datos iniciales para la aplicación RELEVO
-- Ejecutar este script al configurar la base de datos Oracle por primera vez

-- ========================================
-- LIMPIAR TABLAS EXISTENTES (si existen)
-- ========================================

-- Eliminar tablas en orden inverso de dependencias (con manejo de errores)
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE USER_ASSIGNMENTS CASCADE CONSTRAINTS';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE HANDOVER_ACTION_ITEMS CASCADE CONSTRAINTS';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE HANDOVERS CASCADE CONSTRAINTS';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE CONTRIBUTORS CASCADE CONSTRAINTS';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP SEQUENCE CONTRIBUTORS_SEQ';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -2289 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE PATIENTS CASCADE CONSTRAINTS';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE SHIFTS CASCADE CONSTRAINTS';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE UNITS CASCADE CONSTRAINTS';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

-- ========================================
-- CREAR TABLAS
-- ========================================

-- Tabla UNITS
CREATE TABLE UNITS (
    ID VARCHAR2(50) PRIMARY KEY,
    NAME VARCHAR2(100) NOT NULL,
    DESCRIPTION VARCHAR2(500),
    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP
);

-- Tabla SHIFTS
CREATE TABLE SHIFTS (
    ID VARCHAR2(50) PRIMARY KEY,
    NAME VARCHAR2(100) NOT NULL,
    START_TIME VARCHAR2(5) NOT NULL, -- Format: HH:MM
    END_TIME VARCHAR2(5) NOT NULL,   -- Format: HH:MM
    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP
);

-- Tabla PATIENTS
CREATE TABLE PATIENTS (
    ID VARCHAR2(50) PRIMARY KEY,
    NAME VARCHAR2(200) NOT NULL,
    UNIT_ID VARCHAR2(50) NOT NULL,
    DATE_OF_BIRTH DATE,
    GENDER VARCHAR2(20), -- Male, Female, Other, Unknown
    ADMISSION_DATE TIMESTAMP,
    ROOM_NUMBER VARCHAR2(20),
    DIAGNOSIS VARCHAR2(500),
    ALLERGIES VARCHAR2(1000),
    MEDICATIONS VARCHAR2(1000),
    NOTES VARCHAR2(1000),
    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    CONSTRAINT FK_PATIENTS_UNIT FOREIGN KEY (UNIT_ID) REFERENCES UNITS(ID)
);

-- Tabla USER_ASSIGNMENTS (para asignaciones de pacientes a usuarios)
CREATE TABLE USER_ASSIGNMENTS (
    USER_ID VARCHAR2(255) NOT NULL,
    SHIFT_ID VARCHAR2(50) NOT NULL,
    PATIENT_ID VARCHAR2(50) NOT NULL,
    ASSIGNED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    CONSTRAINT PK_USER_ASSIGNMENTS PRIMARY KEY (USER_ID, PATIENT_ID),
    CONSTRAINT FK_ASSIGNMENTS_PATIENT FOREIGN KEY (PATIENT_ID) REFERENCES PATIENTS(ID),
    CONSTRAINT FK_ASSIGNMENTS_SHIFT FOREIGN KEY (SHIFT_ID) REFERENCES SHIFTS(ID)
);

-- Tabla CONTRIBUTORS (para funcionalidad existente)
CREATE TABLE CONTRIBUTORS (
    ID NUMBER PRIMARY KEY,
    NAME VARCHAR2(200) NOT NULL,
    EMAIL VARCHAR2(200) NOT NULL,
    PHONE_NUMBER VARCHAR2(20),
    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP
);

-- Sequence para CONTRIBUTORS (Oracle 11g compatible)
CREATE SEQUENCE CONTRIBUTORS_SEQ START WITH 1 INCREMENT BY 1;

-- Trigger para auto-incrementar ID en CONTRIBUTORS
CREATE OR REPLACE TRIGGER CONTRIBUTORS_TRG
BEFORE INSERT ON CONTRIBUTORS
FOR EACH ROW
WHEN (NEW.ID IS NULL)
BEGIN
    SELECT CONTRIBUTORS_SEQ.NEXTVAL INTO :NEW.ID FROM DUAL;
END;
/

-- Tabla HANDOVERS
CREATE TABLE HANDOVERS (
    ID VARCHAR2(50) PRIMARY KEY,
    PATIENT_ID VARCHAR2(50) NOT NULL,
    STATUS VARCHAR2(20) NOT NULL, -- InProgress, Completed
    ILLNESS_SEVERITY VARCHAR2(20), -- Stable, Watcher, Unstable
    PATIENT_SUMMARY CLOB,
    SITUATION_AWARENESS_DOC_ID VARCHAR2(100),
    SYNTHESIS CLOB,
    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    COMPLETED_AT TIMESTAMP,
    SHIFT_NAME VARCHAR2(100),
    CREATED_BY VARCHAR2(100),
    ASSIGNED_TO VARCHAR2(100),
    CONSTRAINT FK_HANDOVERS_PATIENT FOREIGN KEY (PATIENT_ID) REFERENCES PATIENTS(ID)
);

-- Tabla HANDOVER_ACTION_ITEMS
CREATE TABLE HANDOVER_ACTION_ITEMS (
    ID VARCHAR2(50) PRIMARY KEY,
    HANDOVER_ID VARCHAR2(50) NOT NULL,
    DESCRIPTION VARCHAR2(500) NOT NULL,
    IS_COMPLETED NUMBER(1) DEFAULT 0, -- 0 = false, 1 = true
    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    COMPLETED_AT TIMESTAMP,
    CONSTRAINT FK_ACTION_ITEMS_HANDOVER FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID)
);

-- ========================================
-- CREAR ÍNDICES
-- ========================================

CREATE INDEX IDX_PATIENTS_UNIT_ID ON PATIENTS(UNIT_ID);
CREATE INDEX IDX_USER_ASSIGNMENTS_USER ON USER_ASSIGNMENTS(USER_ID);
CREATE INDEX IDX_USER_ASSIGNMENTS_PATIENT ON USER_ASSIGNMENTS(PATIENT_ID);
CREATE INDEX IDX_USER_ASSIGNMENTS_SHIFT ON USER_ASSIGNMENTS(SHIFT_ID);
CREATE INDEX IDX_HANDOVERS_PATIENT_ID ON HANDOVERS(PATIENT_ID);
CREATE INDEX IDX_HANDOVERS_STATUS ON HANDOVERS(STATUS);
CREATE INDEX IDX_HACTION_HANDOVER_ID ON HANDOVER_ACTION_ITEMS(HANDOVER_ID);

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
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-001', 'María García', 'unit-1', TO_DATE('2010-03-15', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 3, '201', 'Neumonía adquirida en comunidad', 'Penicilina', 'Amoxicilina, Oxígeno suplementario', 'Paciente estable, saturación de oxígeno 94%, requiere nebulizaciones cada 6 horas');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-002', 'Carlos Rodríguez', 'unit-1', TO_DATE('2008-07-22', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 5, '202', 'Sepsis secundaria a infección urinaria', 'Sulfonamidas', 'Meropenem, Vasopresores', 'Paciente crítico, requiere monitoreo continuo de signos vitales y soporte hemodinámico');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-003', 'Ana López', 'unit-1', TO_DATE('2012-11-08', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 2, '203', 'Estado asmático agudo', 'Ninguna', 'Salbutamol, Corticoides intravenosos', 'Paciente con mejoría progresiva, disminución en requerimiento de oxígeno');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-004', 'Miguel Hernández', 'unit-1', TO_DATE('2009-05-30', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 7, '204', 'Trauma craneoencefálico moderado', 'Látex', 'Manitol, Analgésicos', 'Glasgow 12/15, pupilas isocóricas, requiere monitoreo neurológico frecuente');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-005', 'Isabella González', 'unit-1', TO_DATE('2011-09-14', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 4, '205', 'Insuficiencia respiratoria aguda', 'Iodo', 'Ventilación mecánica, Sedantes', 'Paciente intubado, parámetros ventilatorios estables');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-006', 'David Pérez', 'unit-1', TO_DATE('2013-01-25', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 6, '206', 'Choque séptico', 'Ninguna', 'Antibióticos de amplio espectro, Fluidos', 'Paciente en shock distributivo, requiere soporte vasopresor');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-007', 'Sofia Martínez', 'unit-1', TO_DATE('2007-12-03', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 1, '207', 'Meningitis bacteriana', 'Penicilina', 'Ceftriaxona, Dexametasona', 'Paciente con mejoría clínica, cultivos pendientes');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-008', 'José Sánchez', 'unit-1', TO_DATE('2014-06-18', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 8, '208', 'Quemaduras de segundo grado', 'Ninguna', 'Analgésicos, Antibióticos tópicos', 'Quemaduras en 25% superficie corporal, requiere curas diarias');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-009', 'Carmen Díaz', 'unit-1', TO_DATE('2010-08-12', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 9, '209', 'Convulsiones febriles', 'Ninguna', 'Antiepilépticos, Antipiréticos', 'Paciente estable, sin recurrencia de convulsiones');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-010', 'Antonio Moreno', 'unit-1', TO_DATE('2006-04-07', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 12, '210', 'Intoxicación medicamentosa', 'Aspirina', 'Carbón activado, Soporte vital', 'Paciente estabilizado, requiere monitoreo de función hepática');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-011', 'Elena Jiménez', 'unit-1', TO_DATE('2012-02-28', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 3, '211', 'Hipoglucemia severa', 'Ninguna', 'Glucosa intravenosa, Insulina', 'Episodio resuelto, requiere educación diabética');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-012', 'Francisco Ruiz', 'unit-1', TO_DATE('2008-10-19', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 5, '212', 'Trauma abdominal', 'Ninguna', 'Analgésicos, Antibióticos profilácticos', 'Paciente estable, sin signos de peritonitis');

-- Insertar Pacientes para Unidad 2 (Pediatría General) - 12 pacientes
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-013', 'Lucía Álvarez', 'unit-2', TO_DATE('2015-03-12', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 2, '301', 'Bronquiolitis', 'Ninguna', 'Salbutamol, Hidratación', 'Paciente con mejoría respiratoria, buena respuesta al tratamiento');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-014', 'Pablo Romero', 'unit-2', TO_DATE('2011-07-08', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 1, '302', 'Gastroenteritis aguda', 'Ninguna', 'Rehidratación oral, Ondansetrón', 'Paciente con buena tolerancia oral, sin vómitos');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-015', 'Valentina Navarro', 'unit-2', TO_DATE('2013-11-25', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 3, '303', 'Otitis media aguda', 'Amoxicilina', 'Amoxicilina oral, Analgésicos', 'Paciente afebril, disminución del dolor otológico');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-016', 'Diego Torres', 'unit-2', TO_DATE('2009-09-14', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 4, '304', 'Neumonía adquirida en comunidad', 'Ninguna', 'Amoxicilina, Broncodilatadores', 'Paciente afebril, mejoría radiológica en progreso');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-017', 'Marta Ramírez', 'unit-2', TO_DATE('2014-05-30', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 2, '305', 'Infección urinaria', 'Sulfonamidas', 'Cefuroxima, Analgésicos', 'Urocultivo positivo, tratamiento dirigido iniciado');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-018', 'Adrián Gil', 'unit-2', TO_DATE('2010-12-03', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 5, '306', 'Fractura de antebrazo', 'Codeína', 'Ibuprofeno, Inmovilización', 'Fractura simple, alineación adecuada lograda');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-019', 'Clara Serrano', 'unit-2', TO_DATE('2012-08-17', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 1, '307', 'Varicela', 'Ninguna', 'Antihistamínicos, Antipiréticos', 'Lesiones en fase de costra, prurito controlado');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-020', 'Hugo Castro', 'unit-2', TO_DATE('2016-01-22', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 3, '308', 'Deshidratación moderada', 'Ninguna', 'Rehidratación intravenosa', 'Paciente hidratado, buena diuresis');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-021', 'Natalia Rubio', 'unit-2', TO_DATE('2013-04-09', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 6, '309', 'Apendicitis aguda', 'Ninguna', 'Antibióticos, Analgésicos', 'Paciente postoperatorio, evolución favorable');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-022', 'Iván Ortega', 'unit-2', TO_DATE('2008-11-14', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 2, '310', 'Asma agudizada', 'Ninguna', 'Salbutamol, Corticoides', 'Paciente con buen control sintomático');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-023', 'Paula Delgado', 'unit-2', TO_DATE('2011-06-27', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 7, '311', 'Faringoamigdalitis', 'Penicilina', 'Azitromicina, Analgésicos', 'Paciente afebril, mejoría de odinofagia');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-024', 'Mario Guerrero', 'unit-2', TO_DATE('2014-02-11', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 4, '312', 'Traumatismo craneoencefálico leve', 'Ninguna', 'Analgésicos, Observación', 'Glasgow 15/15, paciente asintomático');

-- Insertar Pacientes para Unidad 3 (Pediatría Especializada) - 11 pacientes
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-025', 'Laura Flores', 'unit-3', TO_DATE('2009-07-05', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 10, '401', 'Cardiopatía congénita', 'Ninguna', 'Digoxina, Diuréticos', 'Paciente compensado, requiere seguimiento cardiológico');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-026', 'Álvaro Vargas', 'unit-3', TO_DATE('2010-12-18', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 8, '402', 'Diabetes mellitus tipo 1', 'Ninguna', 'Insulina, Control glucémico', 'Buen control metabólico, educación diabética en progreso');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-027', 'Cristina Medina', 'unit-3', TO_DATE('2007-09-23', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 15, '403', 'Fibrosis quística', 'Ninguna', 'Antibióticos, Enzimas pancreáticas', 'Paciente estable, requiere fisioterapia respiratoria');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-028', 'Sergio Herrera', 'unit-3', TO_DATE('2012-04-30', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 6, '404', 'Trastorno del espectro autista', 'Ninguna', 'No farmacológico', 'Paciente en programa de intervención temprana');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-029', 'Alicia Castro', 'unit-3', TO_DATE('2011-08-14', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 12, '405', 'Epilepsia refractaria', 'Ninguna', 'Antiepilépticos múltiples', 'Paciente con control parcial de convulsiones');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-030', 'Roberto Vega', 'unit-3', TO_DATE('2008-03-07', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 20, '406', 'Leucemia linfoblástica aguda', 'Ninguna', 'Quimioterapia, Soporte', 'Paciente en protocolo de tratamiento oncológico');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-031', 'Beatriz León', 'unit-3', TO_DATE('2013-11-29', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 9, '407', 'Síndrome de Down con cardiopatía', 'Ninguna', 'Medicamentos cardíacos', 'Paciente compensado hemodinámicamente');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-032', 'Manuel Peña', 'unit-3', TO_DATE('2006-06-12', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 25, '408', 'Parálisis cerebral', 'Ninguna', 'Antiespásticos, Fisioterapia', 'Paciente en programa de rehabilitación intensiva');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-033', 'Silvia Cortés', 'unit-3', TO_DATE('2014-01-16', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 7, '409', 'Prematuridad extrema', 'Ninguna', 'Nutrición especializada, Soporte respiratorio', 'Paciente en seguimiento de desarrollo neurológico');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-034', 'Fernando Aguilar', 'unit-3', TO_DATE('2010-05-03', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 11, '410', 'Trastorno de déficit de atención', 'Ninguna', 'Estimulantes, Terapia conductual', 'Paciente con buena respuesta al tratamiento');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-035', 'Teresa Santana', 'unit-3', TO_DATE('2009-10-25', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 14, '411', 'Talasemia mayor', 'Ninguna', 'Transfusiones, Quelantes', 'Paciente en programa de trasfusiones crónicas');

-- Insertar colaboradores de ejemplo
INSERT INTO CONTRIBUTORS (NAME, EMAIL, PHONE_NUMBER) VALUES ('Dra. María García', 'maria.garcia@hospital.com.ar', '+54-11-555-0123');
INSERT INTO CONTRIBUTORS (NAME, EMAIL, PHONE_NUMBER) VALUES ('Dr. Carlos López', 'carlos.lopez@hospital.com.ar', '+54-11-555-0124');
INSERT INTO CONTRIBUTORS (NAME, EMAIL, PHONE_NUMBER) VALUES ('Dra. Ana Martínez', 'ana.martinez@hospital.com.ar', '+54-11-555-0125');

-- Insertar traspasos de ejemplo
INSERT INTO HANDOVERS (ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, CREATED_BY, ASSIGNED_TO, COMPLETED_AT)
VALUES ('hvo-001', 'pat-001', 'InProgress', 'Stable', 'Paciente estable postoperatorio con signos vitales buenos. No se observan complicaciones. Dolor bien controlado con el régimen actual.', 'hvo-001-sa', NULL, 'Mañana', 'demo-user', 'demo-user', NULL);

INSERT INTO HANDOVERS (ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, CREATED_BY, ASSIGNED_TO, COMPLETED_AT)
VALUES ('hvo-002', 'pat-002', 'Completed', 'Watcher', 'Paciente muestra signos de mejoría con requerimientos reducidos de oxígeno. Fisioterapia respiratoria efectiva.', 'hvo-002-sa', 'Paciente listo para cuidados de menor complejidad. Continuar monitoreo del estado respiratorio.', 'Noche', 'demo-user', 'demo-user', SYSTIMESTAMP - 2);

INSERT INTO HANDOVERS (ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, CREATED_BY, ASSIGNED_TO, COMPLETED_AT)
VALUES ('hvo-003', 'pat-003', 'InProgress', 'Stable', 'Recuperación postoperatoria procediendo según lo esperado. Cicatrización de herida buena sin signos de infección.', 'hvo-003-sa', NULL, 'Mañana', 'demo-user', 'demo-user', NULL);

-- Insertar elementos de acción de traspasos de ejemplo
INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('act-001', 'hvo-001', 'Monitorear signos vitales cada 4 horas', 0);

INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('act-002', 'hvo-001', 'Administrar medicación para el dolor según necesidad', 1);

INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('act-003', 'hvo-002', 'Retirar soporte de oxígeno gradualmente', 1);

INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('act-004', 'hvo-002', 'Continuar fisioterapia respiratoria', 1);

INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('act-005', 'hvo-003', 'Revisar herida quirúrgica diariamente', 0);

INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED)
VALUES ('act-006', 'hvo-003', 'Monitorear signos de infección', 0);

COMMIT;

-- ========================================
-- CONSULTAS DE VERIFICACIÓN
-- ========================================

-- Verificar conteos de tablas
SELECT 'UNITS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM UNITS
UNION ALL
SELECT 'SHIFTS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM SHIFTS
UNION ALL
SELECT 'PATIENTS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM PATIENTS
UNION ALL
SELECT 'USER_ASSIGNMENTS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM USER_ASSIGNMENTS
UNION ALL
SELECT 'CONTRIBUTORS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM CONTRIBUTORS
UNION ALL
SELECT 'HANDOVERS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM HANDOVERS
UNION ALL
SELECT 'HANDOVER_ACTION_ITEMS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM HANDOVER_ACTION_ITEMS;
