
## 1) Tablas base

```sql
-- UNITS
CREATE TABLE UNITS (
  ID          VARCHAR2(50) PRIMARY KEY,
  NAME        VARCHAR2(100) NOT NULL,
  DESCRIPTION VARCHAR2(500),
  CREATED_AT  TIMESTAMP DEFAULT LOCALTIMESTAMP,
  UPDATED_AT  TIMESTAMP DEFAULT LOCALTIMESTAMP
);

-- SHIFTS (plantilla: Day/Night)
CREATE TABLE SHIFTS (
  ID         VARCHAR2(50) PRIMARY KEY,
  NAME       VARCHAR2(100) NOT NULL,
  START_TIME VARCHAR2(5) NOT NULL,  -- HH:MM
  END_TIME   VARCHAR2(5) NOT NULL,  -- HH:MM
  CREATED_AT TIMESTAMP DEFAULT LOCALTIMESTAMP,
  UPDATED_AT TIMESTAMP DEFAULT LOCALTIMESTAMP
);

-- PATIENTS
CREATE TABLE PATIENTS (
  ID            VARCHAR2(50) PRIMARY KEY,
  NAME          VARCHAR2(200) NOT NULL,
  UNIT_ID       VARCHAR2(50) NOT NULL,
  DATE_OF_BIRTH DATE,
  GENDER        VARCHAR2(20),
  ADMISSION_DATE TIMESTAMP,
  ROOM_NUMBER   VARCHAR2(20),
  DIAGNOSIS     VARCHAR2(500),
  ALLERGIES     VARCHAR2(1000),
  MEDICATIONS   VARCHAR2(1000),
  NOTES         VARCHAR2(1000),
  MRN           VARCHAR2(20),
  CREATED_AT    TIMESTAMP DEFAULT LOCALTIMESTAMP,
  UPDATED_AT    TIMESTAMP DEFAULT LOCALTIMESTAMP,
  CONSTRAINT FK_PATIENTS_UNIT FOREIGN KEY (UNIT_ID) REFERENCES UNITS(ID)
);

-- USERS
CREATE TABLE USERS (
  ID         VARCHAR2(255) PRIMARY KEY,
  EMAIL      VARCHAR2(200) NOT NULL,
  FIRST_NAME VARCHAR2(100),
  LAST_NAME  VARCHAR2(100),
  FULL_NAME  VARCHAR2(200),
  AVATAR_URL VARCHAR2(500),
  ROLE       VARCHAR2(50) DEFAULT 'user',
  IS_ACTIVE  NUMBER(1) DEFAULT 1,
  LAST_LOGIN TIMESTAMP,
  CREATED_AT TIMESTAMP DEFAULT LOCALTIMESTAMP,
  UPDATED_AT TIMESTAMP DEFAULT LOCALTIMESTAMP
);
```

> **Nota sobre usuario system:** Se recomienda crear un usuario especial `USERS.ID='system'` (o `'handover-bot'`) para representar acciones automáticas del sistema (ej: `AutoVoid_NoCoverage`). Este usuario se usa como `CANCELLED_BY_USER_ID` en cancelaciones automáticas, manteniendo auditoría completa sin permitir NULLs.
>
> ✅ **IMPLEMENTADO:** El usuario system existe en `04-seed-basic.sql` con `ID='system'`, `EMAIL='system@relevo.app'`, `FULL_NAME='System Bot'`, `ROLE='system'`.

---

## 2) Shift occurrences + transitions

```sql
-- SHIFT_INSTANCES: ocurrencia real de un shift (con fecha/hora) por unidad
CREATE TABLE SHIFT_INSTANCES (
  ID         VARCHAR2(50) PRIMARY KEY,
  UNIT_ID    VARCHAR2(50) NOT NULL,
  SHIFT_ID   VARCHAR2(50) NOT NULL,
  START_AT   TIMESTAMP NOT NULL,
  END_AT     TIMESTAMP NOT NULL,
  CREATED_AT TIMESTAMP DEFAULT LOCALTIMESTAMP NOT NULL,
  UPDATED_AT TIMESTAMP DEFAULT LOCALTIMESTAMP NOT NULL,
  CONSTRAINT FK_SI_UNIT  FOREIGN KEY (UNIT_ID) REFERENCES UNITS(ID),
  CONSTRAINT FK_SI_SHIFT FOREIGN KEY (SHIFT_ID) REFERENCES SHIFTS(ID),
  CONSTRAINT CHK_SI_ORDER CHECK (END_AT > START_AT),
  CONSTRAINT UQ_SI_SHIFT_START UNIQUE (UNIT_ID, SHIFT_ID, START_AT),
  CONSTRAINT UQ_SI_ID_UNIT UNIQUE (ID, UNIT_ID) -- Para FK compuesta en SHIFT_COVERAGE
);

-- SHIFT_WINDOWS: ventana concreta entre dos instancias de turno (representa el "cuándo")
CREATE TABLE SHIFT_WINDOWS (
  ID                     VARCHAR2(50) PRIMARY KEY,
  UNIT_ID                VARCHAR2(50) NOT NULL, -- DB-enforced: unidad de ambas instancias
  FROM_SHIFT_INSTANCE_ID VARCHAR2(50) NOT NULL,
  TO_SHIFT_INSTANCE_ID   VARCHAR2(50) NOT NULL,
  CREATED_AT             TIMESTAMP DEFAULT LOCALTIMESTAMP NOT NULL,
  UPDATED_AT             TIMESTAMP DEFAULT LOCALTIMESTAMP NOT NULL,

  CONSTRAINT FK_SW_UNIT FOREIGN KEY (UNIT_ID) REFERENCES UNITS(ID),

  -- DB-enforced: ambas instancias deben ser de la misma unidad (sin triggers)
  CONSTRAINT FK_SW_FROM_SI_UNIT FOREIGN KEY (FROM_SHIFT_INSTANCE_ID, UNIT_ID)
    REFERENCES SHIFT_INSTANCES(ID, UNIT_ID),
  CONSTRAINT FK_SW_TO_SI_UNIT FOREIGN KEY (TO_SHIFT_INSTANCE_ID, UNIT_ID)
    REFERENCES SHIFT_INSTANCES(ID, UNIT_ID),

  CONSTRAINT CHK_SW_NOT_SAME CHECK (FROM_SHIFT_INSTANCE_ID <> TO_SHIFT_INSTANCE_ID),
  CONSTRAINT UQ_SW_PAIR UNIQUE (FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID),
  -- Unique necesario para FK compuesta desde HANDOVERS
  CONSTRAINT UQ_SW_ID_UNIT UNIQUE (ID, UNIT_ID)
);

-- Índices útiles
CREATE INDEX IX_SW_UNIT ON SHIFT_WINDOWS(UNIT_ID);
CREATE INDEX IX_SW_FROM ON SHIFT_WINDOWS(FROM_SHIFT_INSTANCE_ID);
CREATE INDEX IX_SW_TO   ON SHIFT_WINDOWS(TO_SHIFT_INSTANCE_ID);
```

> **Nota sobre scope:** Los turnos (`SHIFT_INSTANCES`) son por unidad. Cada unidad tiene sus propios turnos y ventanas. Las constraints `FK_SW_FROM_SI_UNIT` y `FK_SW_TO_SI_UNIT` garantizan **DB-enforced** que ambas instancias en `SHIFT_WINDOWS` son de la **misma unidad** (sin necesidad de triggers ni validación en app).

> **Nota conceptual:** `SHIFT_WINDOWS` representa una **ventana temporal concreta** (FROM instancia → TO instancia). El "cuándo" está implícito en las fechas/horas de las instancias referenciadas. Si en el futuro se necesitan reglas conceptuales de transición (ej: "Día→Noche como plantilla"), se podría agregar una tabla separada `SHIFT_RULE_TRANSITIONS`.

---

## 3) Shift coverage (cobertura de turnos)

```sql
CREATE TABLE SHIFT_COVERAGE (
  ID                   VARCHAR2(50) PRIMARY KEY,
  RESPONSIBLE_USER_ID   VARCHAR2(255) NOT NULL,
  PATIENT_ID            VARCHAR2(50) NOT NULL,
  SHIFT_INSTANCE_ID     VARCHAR2(50) NOT NULL,
  UNIT_ID               VARCHAR2(50) NOT NULL, -- snapshot: unit del shift_instance
  ASSIGNED_AT           TIMESTAMP DEFAULT LOCALTIMESTAMP NOT NULL,
  IS_PRIMARY            NUMBER(1) DEFAULT 0 NOT NULL, -- 1 = primary (primero asignado)

  CONSTRAINT FK_SC_USER FOREIGN KEY (RESPONSIBLE_USER_ID) REFERENCES USERS(ID),
  CONSTRAINT FK_SC_PAT  FOREIGN KEY (PATIENT_ID) REFERENCES PATIENTS(ID),
  CONSTRAINT FK_SC_UNIT FOREIGN KEY (UNIT_ID) REFERENCES UNITS(ID),

  -- DB-enforced: el shift_instance debe ser de ESA unit_id
  CONSTRAINT FK_SC_SI_UNIT FOREIGN KEY (SHIFT_INSTANCE_ID, UNIT_ID)
    REFERENCES SHIFT_INSTANCES(ID, UNIT_ID),

  CONSTRAINT UQ_SC UNIQUE (RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID)
);

-- Un solo primary por paciente+shift_instance (sin triggers)
-- Implementación: usa constante 'PRIMARY' cuando IS_PRIMARY=1, ID cuando IS_PRIMARY=0
-- Esto permite múltiples non-primary coverages mientras garantiza un solo primary
CREATE UNIQUE INDEX UQ_SC_PRIMARY_ACTIVE ON SHIFT_COVERAGE (
  PATIENT_ID,
  SHIFT_INSTANCE_ID,
  CASE 
    WHEN IS_PRIMARY = 1 THEN 'PRIMARY'
    ELSE ID  -- Use coverage ID for non-primary, ensuring uniqueness
  END
);

CREATE INDEX IX_SC_USER_SI ON SHIFT_COVERAGE(RESPONSIBLE_USER_ID, SHIFT_INSTANCE_ID);
CREATE INDEX IX_SC_PAT_SI  ON SHIFT_COVERAGE(PATIENT_ID, SHIFT_INSTANCE_ID);
CREATE INDEX IX_SC_SI_PAT ON SHIFT_COVERAGE(SHIFT_INSTANCE_ID, PATIENT_ID);
CREATE INDEX IX_SC_PRIMARY ON SHIFT_COVERAGE(PATIENT_ID, SHIFT_INSTANCE_ID, IS_PRIMARY);
```

> **Nota:** El constraint `FK_SC_SI_UNIT` evita cruces de unidad (coverage apunta a shift_instance de otra unidad). Que `PATIENT_ID` sea de esa unidad sigue siendo mejor **app-enforced**, porque si mañana el paciente cambia de unidad, no querés que se rompa el historial.

> **Nota sobre primary:** `IS_PRIMARY=1` identifica al responsable "primario" (el primero asignado). Regla de app: al insertar coverage, si no existe primary para (PATIENT_ID, SHIFT_INSTANCE_ID), setear `IS_PRIMARY=1`. Si el primary se desasigna (DELETE de la fila), la app debe promover al siguiente (el más antiguo por `ASSIGNED_AT`) y setear `IS_PRIMARY=1`. El índice `UQ_SC_PRIMARY_ACTIVE` garantiza un solo primary por paciente+shift_instance usando una constante `'PRIMARY'` cuando `IS_PRIMARY=1`, y el `ID` del coverage cuando `IS_PRIMARY=0`. Esto permite múltiples non-primary coverages mientras mantiene la unicidad del primary.
>
> ✅ **IMPLEMENTADO:** La lógica de promoción de primary está implementada en `AssignmentRepository.RemoveCoverageWithPrimaryPromotionAsync()`. Cuando se elimina un coverage que es primary, se promueve automáticamente al siguiente coverage más antiguo (por `ASSIGNED_AT ASC`) y se setea `IS_PRIMARY=1`. Esta lógica se usa en `UnassignPatientAsync` y `AssignPatientsAsync` (cuando se remueve coverage existente).

> **Nota sobre modelo "estado actual" vs "audit-friendly":** Este modelo es **"estado actual"** (MVP simple): cuando alguien se desasigna, se hace DELETE de la fila. Si en el futuro necesitás auditoría de reasignaciones dentro del mismo turno, podés agregar `STATUS` ('active'|'inactive') y `UNASSIGNED_AT`, pero entonces deberías cambiar `UQ_SC` por un índice único parcial que solo aplique a activos: `CREATE UNIQUE INDEX UQ_SC_ACTIVE ON SHIFT_COVERAGE(RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, CASE WHEN STATUS='active' THEN 1 ELSE NULL END);` y agregar `CONSTRAINT CHK_SC_UNASSIGNED CHECK ((STATUS='active' AND UNASSIGNED_AT IS NULL) OR (STATUS='inactive' AND UNASSIGNED_AT IS NOT NULL));`.

---

## 4) HANDOVERS (por paciente + ventana)

```sql
CREATE TABLE HANDOVERS (
  ID                   VARCHAR2(50) PRIMARY KEY,
  PATIENT_ID           VARCHAR2(50) NOT NULL,
  SHIFT_WINDOW_ID      VARCHAR2(50) NOT NULL,
  UNIT_ID              VARCHAR2(50) NOT NULL, -- snapshot para scoping rápido
  PREVIOUS_HANDOVER_ID VARCHAR2(50) NULL,

  SENDER_USER_ID       VARCHAR2(255) NULL, -- el emisor responsable único (From) = primary del FROM shift
  RECEIVER_USER_ID     VARCHAR2(255) NULL, -- receptor esperado (opcional, para referencia); receiver-of-record = quien completa
  CREATED_BY_USER_ID   VARCHAR2(255) NULL, -- quién creó el handover (puede ser distinto del sender)

  CREATED_AT           TIMESTAMP DEFAULT LOCALTIMESTAMP NOT NULL,
  UPDATED_AT           TIMESTAMP DEFAULT LOCALTIMESTAMP NOT NULL,

  READY_AT             TIMESTAMP NULL,
  READY_BY_USER_ID     VARCHAR2(255),
  STARTED_AT           TIMESTAMP NULL,
  STARTED_BY_USER_ID   VARCHAR2(255),

  COMPLETED_AT         TIMESTAMP NULL,
  COMPLETED_BY_USER_ID VARCHAR2(255),

  CANCELLED_AT         TIMESTAMP NULL,
  CANCELLED_BY_USER_ID VARCHAR2(255),
  CANCEL_REASON        VARCHAR2(200) NULL, -- e.g. 'AutoVoid_NoCoverage', 'Duplicate', 'ReceiverRefused', ...

  CURRENT_STATE VARCHAR2(20) GENERATED ALWAYS AS (
    CASE
      WHEN CANCELLED_AT IS NOT NULL THEN 'Cancelled'
      WHEN COMPLETED_AT IS NOT NULL THEN 'Completed'
      WHEN STARTED_AT   IS NOT NULL THEN 'InProgress'
      WHEN READY_AT     IS NOT NULL THEN 'Ready'
      ELSE 'Draft'
    END
  ) VIRTUAL,

  CONSTRAINT FK_HO_PATIENT    FOREIGN KEY (PATIENT_ID) REFERENCES PATIENTS(ID),
  CONSTRAINT FK_HO_WINDOW     FOREIGN KEY (SHIFT_WINDOW_ID) REFERENCES SHIFT_WINDOWS(ID),
  CONSTRAINT FK_HO_UNIT       FOREIGN KEY (UNIT_ID) REFERENCES UNITS(ID),
  -- DB-enforced: UNIT_ID debe coincidir con la unidad de la ventana
  CONSTRAINT FK_HO_WINDOW_UNIT FOREIGN KEY (SHIFT_WINDOW_ID, UNIT_ID)
    REFERENCES SHIFT_WINDOWS(ID, UNIT_ID),
  -- DB-enforced: PREVIOUS_HANDOVER_ID debe ser del mismo paciente (FK compuesta)
  CONSTRAINT FK_HO_PREV_SAME_PAT FOREIGN KEY (PREVIOUS_HANDOVER_ID, PATIENT_ID)
    REFERENCES HANDOVERS(ID, PATIENT_ID),
  CONSTRAINT FK_HO_SENDER     FOREIGN KEY (SENDER_USER_ID) REFERENCES USERS(ID),
  CONSTRAINT FK_HO_RECEIVER   FOREIGN KEY (RECEIVER_USER_ID) REFERENCES USERS(ID),
  CONSTRAINT FK_HO_CREATED_BY FOREIGN KEY (CREATED_BY_USER_ID) REFERENCES USERS(ID),
  CONSTRAINT FK_HO_READY_BY   FOREIGN KEY (READY_BY_USER_ID) REFERENCES USERS(ID),
  CONSTRAINT FK_HO_STARTED_BY FOREIGN KEY (STARTED_BY_USER_ID) REFERENCES USERS(ID),
  CONSTRAINT FK_HO_COMPLETED_BY FOREIGN KEY (COMPLETED_BY_USER_ID) REFERENCES USERS(ID),
  CONSTRAINT FK_HO_CANCELLED_BY FOREIGN KEY (CANCELLED_BY_USER_ID) REFERENCES USERS(ID),

  CONSTRAINT CHK_HO_RD_BY_REQ CHECK (READY_AT IS NULL OR READY_BY_USER_ID IS NOT NULL),
  CONSTRAINT CHK_HO_ST_BY_REQ CHECK (STARTED_AT IS NULL OR STARTED_BY_USER_ID IS NOT NULL),
  CONSTRAINT CHK_HO_CO_BY_REQ CHECK (COMPLETED_AT IS NULL OR COMPLETED_BY_USER_ID IS NOT NULL),
  CONSTRAINT CHK_HO_CO_REQ_ST CHECK (COMPLETED_AT IS NULL OR STARTED_AT IS NOT NULL),

  -- Consistencia inversa: *_BY implica *_AT (evita filas raras)
  CONSTRAINT CHK_HO_RD_BY_IMPLIES_RD_AT CHECK (READY_BY_USER_ID IS NULL OR READY_AT IS NOT NULL),
  CONSTRAINT CHK_HO_ST_BY_IMPLIES_ST_AT CHECK (STARTED_BY_USER_ID IS NULL OR STARTED_AT IS NOT NULL),
  CONSTRAINT CHK_HO_CO_BY_IMPLIES_CO_AT CHECK (COMPLETED_BY_USER_ID IS NULL OR COMPLETED_AT IS NOT NULL),
  CONSTRAINT CHK_HO_CAN_BY_IMPLIES_CAN_AT CHECK (CANCELLED_BY_USER_ID IS NULL OR CANCELLED_AT IS NOT NULL),

  -- Ready requiere sender seteado (receiver-of-record se define al completar)
  CONSTRAINT CHK_HO_READY_REQ_SENDER CHECK (READY_AT IS NULL OR SENDER_USER_ID IS NOT NULL),

  -- DB-enforced: el que start/complete NO puede ser el sender (mismo doctor no puede ser emisor y receptor)
  CONSTRAINT CHK_HO_STARTED_NE_SENDER CHECK (
    SENDER_USER_ID IS NULL OR STARTED_BY_USER_ID IS NULL OR SENDER_USER_ID <> STARTED_BY_USER_ID
  ),
  CONSTRAINT CHK_HO_COMPLETED_NE_SENDER CHECK (
    SENDER_USER_ID IS NULL OR COMPLETED_BY_USER_ID IS NULL OR SENDER_USER_ID <> COMPLETED_BY_USER_ID
  ),

  -- Cancel: permitido incluso en Draft, pero auditado
  CONSTRAINT CHK_HO_CAN_BY_REQ CHECK (CANCELLED_AT IS NULL OR CANCELLED_BY_USER_ID IS NOT NULL),
  CONSTRAINT CHK_HO_CAN_RSN_REQ CHECK (CANCELLED_AT IS NULL OR CANCEL_REASON IS NOT NULL),

  -- Terminales mutuamente excluyentes: Completed vs Cancelled
  CONSTRAINT CHK_HO_ONE_TERM CHECK (
    (CASE WHEN COMPLETED_AT IS NOT NULL THEN 1 ELSE 0 END +
     CASE WHEN CANCELLED_AT IS NOT NULL THEN 1 ELSE 0 END) <= 1
  ),

  CONSTRAINT CHK_HO_RD_AFTER_CR CHECK (READY_AT IS NULL OR READY_AT >= CREATED_AT),
  CONSTRAINT CHK_HO_ST_REQ_RD   CHECK (STARTED_AT IS NULL OR READY_AT IS NOT NULL),
  CONSTRAINT CHK_HO_ST_AFTER_RD CHECK (STARTED_AT IS NULL OR STARTED_AT >= READY_AT),

  -- Reglas temporales para Cancel (evita datos imposibles)
  CONSTRAINT CHK_HO_CAN_AFTER_CR CHECK (CANCELLED_AT IS NULL OR CANCELLED_AT >= CREATED_AT),
  CONSTRAINT CHK_HO_CAN_AFTER_RD CHECK (CANCELLED_AT IS NULL OR READY_AT IS NULL OR CANCELLED_AT >= READY_AT),
  CONSTRAINT CHK_HO_CAN_AFTER_ST CHECK (CANCELLED_AT IS NULL OR STARTED_AT IS NULL OR CANCELLED_AT >= STARTED_AT),

  CONSTRAINT UQ_HO_PAT_WINDOW UNIQUE (PATIENT_ID, SHIFT_WINDOW_ID),
  -- DB-enforced: permite FK compuesta para PREVIOUS_HANDOVER_ID del mismo paciente
  CONSTRAINT UQ_HO_ID_PAT UNIQUE (ID, PATIENT_ID)
);

-- Índices útiles
CREATE INDEX IX_HO_UNIT_STATE_TIME ON HANDOVERS(UNIT_ID, CURRENT_STATE, NVL(READY_AT, CREATED_AT) DESC);
CREATE INDEX IX_HO_SENDER ON HANDOVERS(SENDER_USER_ID);
CREATE INDEX IX_HO_COMPLETED_BY ON HANDOVERS(COMPLETED_BY_USER_ID);
CREATE INDEX IX_HO_STARTED_BY ON HANDOVERS(STARTED_BY_USER_ID);
```

> **Nota:** `UNIT_ID` se setea en app (sin triggers) como la unidad del `FROM_SHIFT_INSTANCE` (o del `TO`, deberían coincidir por regla de app). Esto permite scoping rápido sin joins.

> **Nota sobre responsables:** El handover tiene **exactamente 1 emisor responsable** (`SENDER_USER_ID` = primary del FROM shift). El **receiver-of-record** es quien completa (`COMPLETED_BY_USER_ID`), no está fijado de antemano. `RECEIVER_USER_ID` es opcional (para referencia/UI) pero no se usa como constraint fuerte. `SENDER_USER_ID` puede ser NULL en estado `Draft`, pero `CHK_HO_READY_REQ_SENDER` garantiza que no puede haber `Ready` sin sender. Al pasar a `Ready`, se setea `SENDER_USER_ID` desde `SHIFT_COVERAGE` del `FROM_SHIFT_INSTANCE` (primary o primero por `ASSIGNED_AT`). Los constraints `CHK_HO_STARTED_NE_SENDER` y `CHK_HO_COMPLETED_NE_SENDER` aseguran que el mismo doctor no puede ser emisor y receptor (start/complete no pueden ser el sender).

> **Nota sobre presencia vs responsabilidad:** **Presence** (quién está mirando/colaborando ahora) es **efímero** y **NO se persiste en DB**. **Responsabilidad/firma** (quién responde por el pase) es **exactamente 1 emisor + 1 receptor** y **SÍ va a DB** como columnas en `HANDOVERS`. Otros usuarios pueden editar/colaborar (registrado en `HANDOVER_CONTENTS.LAST_EDITED_BY` y logs), pero no son responsables. No se necesita tabla `HANDOVER_MEMBERS`.

> **Nota sobre unidad DB-enforced:** El constraint `FK_HO_WINDOW_UNIT` garantiza que `HANDOVERS.UNIT_ID` coincide con `SHIFT_WINDOWS.UNIT_ID` (DB-enforced, sin necesidad de validación en app). Las constraints `FK_SW_FROM_SI_UNIT` y `FK_SW_TO_SI_UNIT` garantizan que ambas instancias en `SHIFT_WINDOWS` son de la misma unidad.

> **Nota sobre PREVIOUS_HANDOVER_ID:** El constraint `FK_HO_PREV_SAME_PAT` (FK compuesta) garantiza que `PREVIOUS_HANDOVER_ID` siempre apunta a un handover del mismo paciente (DB-enforced, sin triggers). Requiere el unique `UQ_HO_ID_PAT` para poder referenciar `(ID, PATIENT_ID)`.

> **Nota sobre READY_BY_USER_ID:** Campo de auditoría que registra quién marcó el handover como Ready. Los constraints `CHK_HO_RD_BY_REQ` y `CHK_HO_RD_BY_IMPLIES_RD_AT` aseguran consistencia simétrica con Start/Complete/Cancel (si hay `READY_AT` entonces hay `READY_BY_USER_ID`, y viceversa).

> **Nota sobre selección del sender:** El sender es el **primary** del FROM shift (el primero asignado). Regla de app: al insertar coverage, si no existe primary para (PATIENT_ID, SHIFT_INSTANCE_ID), setear `IS_PRIMARY=1`. Si el primary se desasigna (DELETE de la fila), promover al siguiente (el más antiguo por `ASSIGNED_AT`) y setear `IS_PRIMARY=1`. Los constraints `CHK_HO_ST_BY_IMPLIES_ST_AT`, `CHK_HO_CO_BY_IMPLIES_CO_AT` y `CHK_HO_CAN_BY_IMPLIES_CAN_AT` aseguran consistencia: si hay `*_BY_USER_ID` entonces debe haber `*_AT` correspondiente (evita filas raras).
>
> ✅ **IMPLEMENTADO:** La selección del sender está implementada en:
> - `HandoverRepository.CreateHandoverAsync()`: Selecciona el primary del FROM shift (o el primero por `ASSIGNED_AT` si no hay primary) y lo setea como `SENDER_USER_ID` al crear el handover.
> - `HandoverRepository.MarkAsReadyAsync()`: Si `SENDER_USER_ID` no está seteado, lo selecciona del primary del FROM shift (o el primero por `ASSIGNED_AT`) antes de setear `READY_AT`. Valida que existe coverage >= 1 antes de permitir Ready.

> **Nota sobre estados:** El estado máquina solo incluye estados **mecánicos**: Draft → Ready → InProgress → Completed → Cancelled. No hay estados "humanos" como Rejected o Expired. El rechazo verdadero (receptor se niega) se modela como **Cancel con `CANCEL_REASON='ReceiverRefused'`**. El rechazo blando (faltan cosas) se modela como **ReturnForChanges** (regla de app: vuelve a Draft limpiando `READY_AT`). El receiver-of-record se define al completar: `COMPLETED_BY_USER_ID` debe tener coverage en el TO shift (validación de app).

> **Nota sobre cancelación por sistema:** Para cancelaciones automáticas (ej: `AutoVoid_NoCoverage`), se recomienda crear un usuario especial `USERS.ID='system'` (o `'handover-bot'`) y usar ese ID como `CANCELLED_BY_USER_ID`. Esto mantiene la auditoría completa sin permitir NULLs.
>
> ✅ **IMPLEMENTADO:** El usuario system existe y está disponible para cancelaciones automáticas. Ver nota en sección 1.

---

## 5) Contenidos y tablas hijas

```sql
CREATE TABLE HANDOVER_CONTENTS (
  HANDOVER_ID           VARCHAR2(50) PRIMARY KEY,
  ILLNESS_SEVERITY      VARCHAR2(20),
  PATIENT_SUMMARY       VARCHAR2(4000),
  SITUATION_AWARENESS   VARCHAR2(4000),
  SYNTHESIS             VARCHAR2(4000),
  PATIENT_SUMMARY_STATUS VARCHAR2(20),
  SA_STATUS             VARCHAR2(20),
  SYNTHESIS_STATUS      VARCHAR2(20),
  LAST_EDITED_BY        VARCHAR2(255),
  UPDATED_AT            TIMESTAMP DEFAULT LOCALTIMESTAMP NOT NULL,

  CONSTRAINT FK_HC_HANDOVER FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID) ON DELETE CASCADE,
  CONSTRAINT FK_HC_LAST_EDITED FOREIGN KEY (LAST_EDITED_BY) REFERENCES USERS(ID),

  CONSTRAINT CHK_HC_SUM_ST CHECK (PATIENT_SUMMARY_STATUS IS NULL OR PATIENT_SUMMARY_STATUS IN ('Draft','Completed')),
  CONSTRAINT CHK_HC_SA_ST  CHECK (SA_STATUS IS NULL OR SA_STATUS IN ('Draft','Completed')),
  CONSTRAINT CHK_HC_SYN_ST CHECK (SYNTHESIS_STATUS IS NULL OR SYNTHESIS_STATUS IN ('Draft','Completed'))
);

CREATE TABLE HANDOVER_ACTION_ITEMS (
  ID           VARCHAR2(50) PRIMARY KEY,
  HANDOVER_ID  VARCHAR2(50) NOT NULL,
  DESCRIPTION  VARCHAR2(500) NOT NULL,
  IS_COMPLETED NUMBER(1) DEFAULT 0,
  CREATED_AT   TIMESTAMP DEFAULT LOCALTIMESTAMP,
  UPDATED_AT   TIMESTAMP DEFAULT LOCALTIMESTAMP,
  COMPLETED_AT TIMESTAMP,
  CONSTRAINT FK_ACTION_ITEMS_HO FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID) ON DELETE CASCADE
);

CREATE TABLE HANDOVER_CONTINGENCY (
  ID           VARCHAR2(50) PRIMARY KEY,
  HANDOVER_ID  VARCHAR2(50) NOT NULL,
  CONDITION_TEXT VARCHAR2(1000) NOT NULL,
  ACTION_TEXT    VARCHAR2(1000) NOT NULL,
  PRIORITY       VARCHAR2(20) DEFAULT 'medium',
  STATUS         VARCHAR2(20) DEFAULT 'active',
  CREATED_BY     VARCHAR2(255) NOT NULL,
  CREATED_AT     TIMESTAMP DEFAULT LOCALTIMESTAMP,
  UPDATED_AT     TIMESTAMP DEFAULT LOCALTIMESTAMP,

  CONSTRAINT FK_CONT_HO FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID) ON DELETE CASCADE,
  CONSTRAINT FK_CONT_USER FOREIGN KEY (CREATED_BY) REFERENCES USERS(ID),
  CONSTRAINT CHK_CONT_PRIORITY CHECK (PRIORITY IN ('low','medium','high')),
  CONSTRAINT CHK_CONT_STATUS   CHECK (STATUS IN ('active','planned','completed'))
);

CREATE TABLE HANDOVER_MESSAGES (
  ID           VARCHAR2(50) PRIMARY KEY,
  HANDOVER_ID  VARCHAR2(50) NOT NULL,
  USER_ID      VARCHAR2(255) NOT NULL,
  MESSAGE_TEXT VARCHAR2(4000) NOT NULL,
  MESSAGE_TYPE VARCHAR2(20) DEFAULT 'message',
  CREATED_AT   TIMESTAMP DEFAULT LOCALTIMESTAMP,
  UPDATED_AT   TIMESTAMP DEFAULT LOCALTIMESTAMP,

  CONSTRAINT FK_MSG_HO FOREIGN KEY (HANDOVER_ID) REFERENCES HANDOVERS(ID) ON DELETE CASCADE,
  CONSTRAINT FK_MSG_USER FOREIGN KEY (USER_ID) REFERENCES USERS(ID),
  CONSTRAINT CHK_MSG_TYPE CHECK (MESSAGE_TYPE IN ('message','system','notification'))
);

CREATE TABLE HANDOVER_MENTIONS (
  ID                VARCHAR2(50) PRIMARY KEY,
  MESSAGE_ID        VARCHAR2(50) NOT NULL,
  MENTIONED_USER_ID VARCHAR2(255) NOT NULL,
  CREATED_AT        TIMESTAMP DEFAULT LOCALTIMESTAMP,

  CONSTRAINT FK_MENTION_MSG FOREIGN KEY (MESSAGE_ID) REFERENCES HANDOVER_MESSAGES(ID) ON DELETE CASCADE,
  CONSTRAINT FK_MENTION_USER FOREIGN KEY (MENTIONED_USER_ID) REFERENCES USERS(ID)
);
```

---

## 6) Reglas de negocio

```
1. Un **handover** representa un **traspaso de responsabilidad (transfer-of-care)** entre un turno emisor y un turno receptor, **por paciente**.

2. El pase “real” ocurre en una **sesión/reunión presencial** donde se revisan **varios pacientes**, pero el **handover persistido** es **por paciente** (cada paciente tiene “su doc”).

3. **Responsabilidad/firma:** Un handover tiene **exactamente 1 emisor responsable** (`SENDER_USER_ID` = primary del FROM shift, el primero asignado). El **receiver-of-record** es quien completa (`COMPLETED_BY_USER_ID`), no está fijado de antemano. Los "firmantes" se persisten en DB como columnas en `HANDOVERS`. No se necesita tabla `HANDOVER_MEMBERS`.

4. **Presencia (presence):** "Quién está mirando/colaborando ahora" es **efímero** y **NO se persiste en DB**. Se maneja por RT/cliente. Otros usuarios pueden editar/colaborar (registrado en `HANDOVER_CONTENTS.LAST_EDITED_BY` y logs), pero no son responsables del pase.

5. Para que exista un pase válido, se requiere **exactamente 1 emisor responsable** (regla fuerte). El sender debe estar seteado para pasar a `Ready`. El receiver-of-record se define al completar (quien completa debe tener coverage en el TO shift, validación de app).

6. Para el MVP hay **solo 2 plantillas de turno**: **Día** y **Noche**.

7. Los turnos “plantilla” (`SHIFTS`) se instancian como ocurrencias reales (`SHIFT_INSTANCES`) con fecha/hora (para evitar ambigüedad).

8. `SHIFT_WINDOWS` representa una **ventana temporal concreta** (`FROM_SHIFT_INSTANCE → TO_SHIFT_INSTANCE`). El "cuándo" está implícito en las fechas/horas de las instancias referenciadas. Ambas instancias deben ser de la **misma unidad** (**DB-enforced** con `FK_SW_FROM_SI_UNIT` y `FK_SW_TO_SI_UNIT`, sin triggers). Si en el futuro se necesitan reglas conceptuales de transición (ej: "Día→Noche como plantilla"), se podría agregar una tabla separada `SHIFT_RULE_TRANSITIONS`.

9. Regla core: **máximo 1 handover activo por paciente por ventana** (unicidad por `PATIENT_ID + SHIFT_WINDOW_ID`).

10. No puede haber handover "en el aire": **no puede existir handover sin coverage** (si no hay responsables asignados, no debería crearse). **DB no puede forzar esta regla** sin triggers (requiere mirar otras filas), así que es **precondición del comando**. El comando que pasa a `Ready` debe hacer atómico: (1) verificar `SHIFT_COVERAGE` del `FROM_SHIFT_INSTANCE` → debe haber `>=1`, **elegir el primary** (o el primero por `ASSIGNED_AT` si no hay primary) como `SENDER_USER_ID`, (2) recién ahí setear `READY_AT`. El constraint `CHK_HO_READY_REQ_SENDER` garantiza que el sender esté seteado. **Importante:** La regla de selección del sender es "el primero que se asigna" (primary), estable y explicable ("figura como emisor porque fue el primero asignado en ese turno"). El receiver-of-record se define al completar: quien completa debe tener coverage en el TO shift (validación de app).
>
> ✅ **IMPLEMENTADO:** 
> - `HandoverRepository.CreateHandoverAsync()`: Valida que existe coverage >= 1 antes de crear el handover. Si no existe, lanza `InvalidOperationException`. Selecciona el primary (o primero por `ASSIGNED_AT`) como `SENDER_USER_ID` al crear.
> - `HandoverRepository.MarkAsReadyAsync()`: Valida coverage >= 1 antes de setear `READY_AT`. Si no existe coverage, retorna `false`. Selecciona el sender si no está seteado.

11. "Coverage" significa: **"este doctor está a cargo de este paciente en este turno"**.

12. Un paciente puede tener **varios responsables en coverage** (entran/salen; multi-doctor).

13. Para tu app, **solo una persona puede desasignar** (decisión de UX), aunque el modelo puede soportar múltiples.

14. Los médicos **no crean handovers manualmente**: el handover se crea como **efecto secundario** de comandos del dominio (ej: asignar responsable / asegurar transición del día).
>
> ✅ **IMPLEMENTADO:** Los handovers se crean automáticamente mediante el evento de dominio `PatientAssignedToShiftEvent`. El handler `PatientAssignedToShiftHandler` crea handovers cuando se asignan pacientes a turnos. Ver `AUTO_HANDOVER_CREATION.md` para detalles.

15. Regla de arquitectura: **un GET no debe crear** nada (evitar `GetOrCreate...` en lecturas).

16. La creación debe ser **idempotente** y “race-safe” usando **constraints + insert/merge + retry** desde la app (sin triggers).

17. Cuando un receptor se asigna pacientes para el próximo turno, el handover debería poder **estar disponible** (Draft) para completarse antes de la reunión.

18. **Presencia (presence) NO va a DB:** Los "participants/members tipo Google Docs" (presencia: quién mira/tipea ahora) es **efímero** y **NO se persiste** en DB. Se maneja por RT/cliente. La lista de "quién está en la conversación" es volátil y no requiere persistencia.

19. **Colaboración tipo Google Docs sin members:** La colaboración se modela con `HANDOVER_MESSAGES` (discusión), `HANDOVER_CONTENTS.LAST_EDITED_BY` (último editor), y logs de auditoría. No se necesita tabla `HANDOVER_MEMBERS` porque: (a) presence es efímero, (b) responsabilidad es exactamente 1+1 (ya en `HANDOVERS` como columnas).

20. Máquina de estados MVP (por timestamps): **Draft → Ready → InProgress → Completed**, con terminal **Cancelled**. Solo incluye estados **mecánicos**; no hay estados "humanos" como Rejected o Expired en el header. El rechazo verdadero (receptor se niega) se modela como **Cancel con `CANCEL_REASON='ReceiverRefused'`**. El rechazo blando (faltan cosas) se modela como **ReturnForChanges** (regla de app, no estado).

21. **Ready** significa "listo para pasar" (semántica exacta aún no 100% cerrada, pero existe como etapa). Requiere que `SENDER_USER_ID` esté seteado (emisor responsable). El constraint `CHK_HO_READY_REQ_SENDER` lo garantiza. El receiver-of-record se define al completar.

22. Para pasar a **InProgress**: cualquier usuario con coverage en el TO shift puede iniciar. **DB lo fuerza** con constraint: `STARTED_BY_USER_ID` NO puede ser el `SENDER_USER_ID` (mismo doctor no puede ser emisor y receptor). La validación de que quien start tiene coverage en TO shift es **app-enforced**.
>
> ✅ **IMPLEMENTADO:** `HandoverStateMachineHandlers.Handle(StartHandoverCommand)` valida: (1) que el usuario tiene coverage en el TO shift (`HasCoverageInToShiftAsync`), (2) que el usuario NO es el sender (verifica `SENDER_USER_ID`). Si alguna validación falla, retorna error. Si pasa, llama a `HandoverRepository.StartHandoverAsync()`.
>
> ✅ **IMPLEMENTADO:** `HandoverStateMachineHandlers.Handle(StartHandoverCommand)` valida: (1) que el usuario tiene coverage en el TO shift (`HasCoverageInToShiftAsync`), (2) que el usuario NO es el sender (verifica `SENDER_USER_ID`). Si alguna validación falla, retorna error. Si pasa, llama a `HandoverRepository.StartHandoverAsync()`.

23. "InProgress" se interpreta como "**en la sala se empezó a tratar ese paciente**" (no como "hay gente conectada", porque presencia no se persiste).

24. **Completed**: cualquier usuario con coverage en el TO shift puede completar. **DB lo fuerza** con constraint: `COMPLETED_BY_USER_ID` NO puede ser el `SENDER_USER_ID` (mismo doctor no puede ser emisor y receptor). La validación de que quien completa tiene coverage en TO shift es **app-enforced**. El `COMPLETED_BY_USER_ID` es el receiver-of-record.

25. **Cancelled**: puede cancelarse desde cualquier estado (incluso Draft). Si hay `CANCELLED_AT`, debe existir `CANCELLED_BY_USER_ID` y `CANCEL_REASON` (DB enforced). El `CANCEL_REASON` puede ser: `'AutoVoid_NoCoverage'`, `'Duplicate'`, `'ReceiverRefused'` (rechazo verdadero), u otros según reglas de negocio. Para cancelaciones automáticas (sistema), usar `CANCELLED_BY_USER_ID='system'` (usuario especial creado para este propósito). Los constraints `CHK_HO_CAN_AFTER_CR`, `CHK_HO_CAN_AFTER_RD` y `CHK_HO_CAN_AFTER_ST` garantizan que `CANCELLED_AT` es posterior a `CREATED_AT`, `READY_AT` y `STARTED_AT` (si existen).

26. **ReturnForChanges** (rechazo blando): no es un estado, es una **regla de app**. Si estaba `Ready` y alguien "devuelve para cambios": `READY_AT = NULL` (vuelve a Draft). Ventaja: no agrega estado nuevo, no complica constraints.
>
> ✅ **IMPLEMENTADO:** `ReturnForChangesHandler` llama a `HandoverRepository.ReturnForChangesAsync()` que actualiza el handover: setea `READY_AT = NULL` y `READY_BY_USER_ID = NULL` solo si el handover está en estado `Ready` (tiene `READY_AT` no nulo) y no está completado ni cancelado. Esto efectivamente vuelve el handover a estado `Draft`.

27. **ChangeReceiver**: cambio de receptor esperado. `RECEIVER_USER_ID` es opcional y puede actualizarse, pero no se usa como constraint fuerte. El receiver-of-record real es quien completa (`COMPLETED_BY_USER_ID`). No es un estado.

28. Para histórico/auditoría: se quiere saber **quién marcó Ready** (`READY_BY_USER_ID`), **quién dio Start** (`STARTED_BY_USER_ID`), **quién dio Complete** (`COMPLETED_BY_USER_ID` = receiver-of-record), y **quién canceló** (`CANCELLED_BY_USER_ID`). Start y Complete NO pueden ser el sender (DB enforced con `CHK_HO_STARTED_NE_SENDER` y `CHK_HO_COMPLETED_NE_SENDER`).

29. Si se guardan `READY_BY_USER_ID` / `STARTED_BY_USER_ID` / `COMPLETED_BY_USER_ID` / `CANCELLED_BY_USER_ID`, debe ser consistente: si hay `READY_AT` entonces hay `READY_BY_USER_ID` (y análogo para started/completed/cancelled). DB lo fuerza con constraints. Además, `COMPLETED_AT` requiere `STARTED_AT` (consistencia fuerte).

30. Estados terminales deben ser **mutuamente excluyentes** (si se marca uno terminal no se pueden marcar otros a la vez). Los estados terminales globales son: **Completed** y **Cancelled**. Se puede cancelar desde cualquier estado, incluso Draft (DB enforced con `CHK_HO_CAN_RSN_REQ`).

31. Regla temporal: `READY_AT >= CREATED_AT` y `STARTED_AT` requiere `READY_AT` (y típicamente `STARTED_AT >= READY_AT`).

32. El pase se realiza **paciente por paciente**: cada paciente tiene su "start" al ser tratado.

33. No se soporta cobertura parcial tipo **"me cubrís 2 horas"** (explícitamente fuera de scope).

34. "Mis pacientes": un usuario ve su lista y al click entra al handover en curso (modo doc).

35. También existe una vista global donde se pueden ver handovers de otros turnos/unidades (al menos lectura).

36. El **patient summary** es el bloque más estable y típicamente se **copia/arrastra** del handover previo al nuevo.

37. El handover es la **fuente de verdad** del pase (no se espera "otra fuente" paralela para el mismo paciente/transición).

38. No hay triggers en DB: cualquier regla que requiera "mirar otras filas" o "validación cruzada" se hace en la **app** (commands/workflow).

39. **Denormalización de UNIT_ID:** `HANDOVERS.UNIT_ID` es un snapshot para scoping rápido. Debe coincidir con la unidad de la ventana (DB enforced con `FK_HO_WINDOW_UNIT`). `SHIFT_COVERAGE.UNIT_ID` evita cruces de unidad vía FK compuesta a `SHIFT_INSTANCES(ID, UNIT_ID)`.

40. **Validaciones DB-enforced:** `SHIFT_WINDOWS` une instancias de la **misma unidad** (DB enforced con `FK_SW_FROM_SI_UNIT` y `FK_SW_TO_SI_UNIT`). `HANDOVERS.UNIT_ID` coincide con la unidad de la ventana (DB enforced con `FK_HO_WINDOW_UNIT`). **Qué queda app-enforced (sin triggers):** Para pasar a `Ready`: verificar `SHIFT_COVERAGE` del `FROM_SHIFT_INSTANCE` (`>=1`, elegir primary o primero por `ASSIGNED_AT` como `SENDER_USER_ID`), luego setear `READY_AT`. Al iniciar/completar: verificar que quien start/complete tiene coverage en el TO shift. Los constraints `CHK_HO_STARTED_NE_SENDER` y `CHK_HO_COMPLETED_NE_SENDER` garantizan que el mismo doctor no puede ser emisor y receptor (DB enforced).

41. **Responsables:** `HANDOVERS.SENDER_USER_ID` identifica al emisor responsable único (primary del FROM shift, el primero asignado). El **receiver-of-record** es quien completa (`COMPLETED_BY_USER_ID`), no está fijado de antemano. `RECEIVER_USER_ID` es opcional (para referencia/UI) pero no se usa como constraint fuerte. Al pasar a `Ready`: se setea `SENDER_USER_ID` desde `SHIFT_COVERAGE` del `FROM_SHIFT_INSTANCE` (primary o primero por `ASSIGNED_AT`). Al iniciar/completar: quien start/complete debe tener coverage en el TO shift (validación de app). Los constraints `CHK_HO_STARTED_NE_SENDER` y `CHK_HO_COMPLETED_NE_SENDER` garantizan que el mismo doctor no puede ser emisor y receptor (DB enforced). Otros usuarios pueden colaborar/editando pero no son responsables.

42. Regla encadenada: al completar, el receptor "toma" el pase y el próximo handover lo tendrá como emisor (conceptualmente; implementación por transiciones/turnos).

---

## 7) Estado de implementación

> **Nota:** Esta sección documenta qué partes del plan V3 están implementadas en el código actual.

### ✅ Implementado y verificado

1. **Esquema de base de datos:** Todas las tablas, constraints, índices y vistas están implementadas en `relevo-api/src/Relevo.Infrastructure/Data/Sql/`:
   - `01-tables.sql`: Todas las tablas base, shift instances/windows, coverage, handovers y contenido
   - `02-indexes.sql`: Todos los índices incluyendo `UQ_SC_PRIMARY_ACTIVE` con la lógica correcta (usa `ID` cuando `IS_PRIMARY=0`)
   - `03-views.sql`: Vista `VW_HANDOVERS_WITH_STATE` para compatibilidad con Dapper

2. **Máquina de estados:** Implementada correctamente:
   - Estados: `Draft` → `Ready` → `InProgress` → `Completed` (terminal: `Cancelled`)
   - Columna virtual `CURRENT_STATE` calcula el estado desde timestamps
   - Todos los constraints DB-enforced están implementados
   - No hay estados `Accepted`, `Rejected`, `Expired` (como especifica V3_PLAN.md)

3. **Selección de sender:** Implementada en:
   - `HandoverRepository.CreateHandoverAsync()`: Selecciona primary (o primero por `ASSIGNED_AT`) al crear
   - `HandoverRepository.MarkAsReadyAsync()`: Selecciona sender si no está seteado, valida coverage >= 1

4. **Validación de coverage:** Implementada:
   - No se puede crear handover sin coverage (lanza excepción)
   - No se puede pasar a Ready sin coverage (retorna `false`)
   - Validación atómica antes de setear `READY_AT`

5. **Promoción de primary:** Implementada en `AssignmentRepository.RemoveCoverageWithPrimaryPromotionAsync()`:
   - Cuando se elimina un coverage que es primary, promueve al siguiente (más antiguo por `ASSIGNED_AT`)
   - Se usa en `UnassignPatientAsync` y `AssignPatientsAsync`

6. **Transiciones de estado:** Implementadas en `HandoverStateMachineHandlers`:
   - `StartHandoverCommand`: Valida coverage en TO shift, valida que no sea sender
   - `CompleteHandoverCommand`: Valida coverage en TO shift, valida que no sea sender
   - `RejectHandoverCommand`: Usa `CancelHandoverCommand` con `CANCEL_REASON='ReceiverRefused'`
   - `CancelHandoverCommand`: Permite cancelar desde cualquier estado

7. **ReturnForChanges:** Implementado en `ReturnForChangesHandler`:
   - Limpia `READY_AT` y `READY_BY_USER_ID` (vuelve a Draft)
   - Solo permite si está en estado `Ready` y no está completado/cancelado

8. **Usuario system:** Existe en seed data (`04-seed-basic.sql`):
   - `ID='system'`, `EMAIL='system@relevo.app'`, `FULL_NAME='System Bot'`, `ROLE='system'`
   - Disponible para cancelaciones automáticas

9. **Auto-creación de handovers:** Implementada mediante eventos de dominio:
   - `PatientAssignedToShiftEvent` dispara creación automática
   - `PatientAssignedToShiftHandler` crea handovers cuando se asignan pacientes
   - Ver `AUTO_HANDOVER_CREATION.md` para detalles

10. **Cálculo de shift instances:** Implementado en `ShiftInstanceCalculationService`:
    - Maneja turnos nocturnos que cruzan medianoche (agrega 1 día)
    - Soporta cualquier fecha base (aunque actualmente se usa `DateTime.Today` en producción)

### ⚠️ Limitaciones conocidas

1. **Fechas futuras:** La infraestructura existe (`ShiftInstanceCalculationService` acepta `baseDate`), pero el código de producción hardcodea `DateTime.Today`:
   - `HandoverRepository.CreateHandoverAsync()` línea 235: `var today = DateTime.Today;`
   - `AssignmentRepository.AssignPatientsAsync()` línea 92: `var today = DateTime.Today;`
   - **Impacto:** No se pueden crear handovers o assignments para fechas futuras (solo hoy)
   - **Nota:** Esto podría ser intencional para MVP, pero debería documentarse como limitación

2. **Documentación desactualizada:** Algunos documentos no reflejan V3:
   - `docs/HANDOVER-STATE-MODEL.md`: Describe estado machine antigua
   - `docs/DATABASE.md`: Describe esquema antiguo con `ASSIGNMENTS`
   - `docs/API_SCHEMA.md`: Falta documentar endpoints V3 (`/ready`, `/start`, etc.)

### 📋 Pendiente (no crítico)

1. **Tests de edge cases:**
   - Test de promoción de primary cuando solo hay un coverage
   - Test de promoción de primary cuando no quedan coverages
   - Test de creación concurrente de handovers (race conditions)

2. **Documentación:**
   - Actualizar `docs/HANDOVER-STATE-MODEL.md` para reflejar V3
   - Actualizar `docs/DATABASE.md` o marcarlo como deprecated
   - Actualizar `docs/API_SCHEMA.md` con endpoints V3

3. **Soporte de fechas futuras (si se requiere):**
   - Actualizar `HandoverRepository` y `AssignmentRepository` para aceptar parámetro de fecha
   - Actualizar endpoints para permitir fecha opcional
   - Ver `docs/FUTURE_DATE_IMPLEMENTATION_ANALYSIS.md` para plan detallado

