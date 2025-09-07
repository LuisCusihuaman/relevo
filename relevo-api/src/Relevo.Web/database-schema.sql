-- ESQUEMA DE BASE DE DATOS RELEVO Y DATOS DE SEMILLA
-- Este archivo contiene todas las tablas y datos iniciales para la aplicación RELEVO
-- Ejecutar este script al configurar la base de datos Oracle por primera vez

-- ========================================
-- LIMPIAR TABLAS EXISTENTES (si existen)
-- ========================================

-- Eliminar tablas en orden inverso de dependencias (con manejo de errores)
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

-- Insertar Pacientes para Unidad 1 (UCI)
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-123', 'Juan Pérez', 'unit-1', TO_DATE('2010-05-15', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 5, '205', 'Exacerbación de Asma', 'Penicilina', 'Salbutamol, Prednisona', 'Paciente estable, requiere nebulizaciones cada 4 horas');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-456', 'María González', 'unit-1', TO_DATE('2008-03-22', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 3, '207', 'Neumonía', 'Sulfonamidas', 'Antibióticos, Oxígeno', 'Paciente con mejoría, saturación de oxígeno estable');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-789', 'Carlos Rodríguez', 'unit-1', TO_DATE('2012-11-08', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 7, '203', 'Cuidados postoperatorios', 'Látex', 'Analgésicos, Antibióticos', 'Paciente recuperándose bien de apendicectomía');

-- Insertar Pacientes para Unidad 2 (Pediatría General)
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-210', 'Sofía López', 'unit-2', TO_DATE('2015-07-12', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 2, '301', 'Gastroenteritis', 'Ninguna', 'Rehidratación oral, Antieméticos', 'Paciente responde bien al tratamiento, buena ingesta oral');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-220', 'Mateo Fernández', 'unit-2', TO_DATE('2011-09-30', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 4, '305', 'Brazo fracturado', 'Codeína', 'Analgésicos, Inmovilización', 'Paciente cómodo, buen control del dolor logrado');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-230', 'Valentina García', 'unit-2', TO_DATE('2013-01-18', 'YYYY-MM-DD'), 'Female', SYSTIMESTAMP - 1, '302', 'Infección respiratoria superior', 'Amoxicilina', 'Antihistamínicos, Descongestionantes', 'Síntomas mejorando, buena respuesta al medicamento');

-- Insertar Pacientes para Unidad 3 (Pediatría Especializada)
INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-310', 'Patricio Morales', 'unit-3', TO_DATE('2009-12-05', 'YYYY-MM-DD'), 'Other', SYSTIMESTAMP - 6, '401', 'Monitoreo de condición cardíaca', 'Yodo', 'Medicamentos cardíacos, Anticoagulantes', 'Ritmo cardíaco estable, requiere monitoreo regular');

INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES)
VALUES ('pat-320', 'Jordán Blanco', 'unit-3', TO_DATE('2014-06-25', 'YYYY-MM-DD'), 'Male', SYSTIMESTAMP - 8, '405', 'Evaluación neurológica', 'Ninguna', 'Anticonvulsivos, Sedantes', 'Estado neurológico estable, aguardando evaluación especialista');

-- Insertar colaboradores de ejemplo
INSERT INTO CONTRIBUTORS (NAME, EMAIL, PHONE_NUMBER) VALUES ('Dra. María García', 'maria.garcia@hospital.com.ar', '+54-11-555-0123');
INSERT INTO CONTRIBUTORS (NAME, EMAIL, PHONE_NUMBER) VALUES ('Dr. Carlos López', 'carlos.lopez@hospital.com.ar', '+54-11-555-0124');
INSERT INTO CONTRIBUTORS (NAME, EMAIL, PHONE_NUMBER) VALUES ('Dra. Ana Martínez', 'ana.martinez@hospital.com.ar', '+54-11-555-0125');

-- Insertar traspasos de ejemplo
INSERT INTO HANDOVERS (ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, CREATED_BY, ASSIGNED_TO, COMPLETED_AT)
VALUES ('hvo-001', 'pat-123', 'InProgress', 'Stable', 'Paciente estable postoperatorio con signos vitales buenos. No se observan complicaciones. Dolor bien controlado con el régimen actual.', 'hvo-001-sa', NULL, 'Mañana', 'demo-user', 'demo-user', NULL);

INSERT INTO HANDOVERS (ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, CREATED_BY, ASSIGNED_TO, COMPLETED_AT)
VALUES ('hvo-002', 'pat-456', 'Completed', 'Watcher', 'Paciente muestra signos de mejoría con requerimientos reducidos de oxígeno. Fisioterapia respiratoria efectiva.', 'hvo-002-sa', 'Paciente listo para cuidados de menor complejidad. Continuar monitoreo del estado respiratorio.', 'Noche', 'demo-user', 'demo-user', SYSTIMESTAMP - 2);

INSERT INTO HANDOVERS (ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, CREATED_BY, ASSIGNED_TO, COMPLETED_AT)
VALUES ('hvo-003', 'pat-789', 'InProgress', 'Stable', 'Recuperación postoperatoria procediendo según lo esperado. Cicatrización de herida buena sin signos de infección.', 'hvo-003-sa', NULL, 'Mañana', 'demo-user', 'demo-user', NULL);

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
SELECT 'CONTRIBUTORS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM CONTRIBUTORS
UNION ALL
SELECT 'HANDOVERS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM HANDOVERS
UNION ALL
SELECT 'HANDOVER_ACTION_ITEMS' AS TABLE_NAME, COUNT(*) AS RECORD_COUNT FROM HANDOVER_ACTION_ITEMS;
