# Reglas de Negocio - Relevo V3

Este documento describe las reglas de negocio del sistema Relevo V3, extraídas del plan de implementación.

---

## Conceptos Fundamentales

### Handover (Pase de Guardia)

1. Un **handover** representa un **traspaso de responsabilidad (transfer-of-care)** entre un turno emisor y un turno receptor, **por paciente**.

2. El pase "real" ocurre en una **sesión/reunión presencial** donde se revisan **varios pacientes**, pero el **handover persistido** es **por paciente** (cada paciente tiene "su doc").

3. Regla core: **máximo 1 handover activo por paciente por ventana** (unicidad por `PATIENT_ID + SHIFT_WINDOW_ID`).

4. El handover es la **fuente de verdad** del pase (no se espera "otra fuente" paralela para el mismo paciente/transición).

### Turnos y Ventanas

5. Para el MVP hay **solo 2 plantillas de turno**: **Día** y **Noche**.

6. Los turnos "plantilla" (`SHIFTS`) se instancian como ocurrencias reales (`SHIFT_INSTANCES`) con fecha/hora (para evitar ambigüedad).

7. `SHIFT_WINDOWS` representa una **ventana temporal concreta** (`FROM_SHIFT_INSTANCE → TO_SHIFT_INSTANCE`). El "cuándo" está implícito en las fechas/horas de las instancias referenciadas. Ambas instancias deben ser de la **misma unidad** (DB-enforced, sin triggers). Si en el futuro se necesitan reglas conceptuales de transición (ej: "Día→Noche como plantilla"), se podría agregar una tabla separada `SHIFT_RULE_TRANSITIONS`.

8. Los turnos (`SHIFT_INSTANCES`) son por unidad. Cada unidad tiene sus propios turnos y ventanas.

---

## Responsabilidad y Presencia

### Responsabilidad/Firma

9. **Responsabilidad/firma:** Un handover tiene **exactamente 1 emisor responsable** (`SENDER_USER_ID` = primary del FROM shift, el primero asignado). El **receiver-of-record** es quien completa (`COMPLETED_BY_USER_ID`), no está fijado de antemano. Los "firmantes" se persisten en DB como columnas en `HANDOVERS`. No se necesita tabla `HANDOVER_MEMBERS`.

10. Para que exista un pase válido, se requiere **exactamente 1 emisor responsable** (regla fuerte). El sender debe estar seteado para pasar a `Ready`. El receiver-of-record se define al completar (quien completa debe tener coverage en el TO shift, validación de app).

11. El sender es el **primary** del FROM shift (el primero asignado). Regla de app: al insertar coverage, si no existe primary para (PATIENT_ID, SHIFT_INSTANCE_ID), setear `IS_PRIMARY=1`. Si el primary se desasigna (DELETE de la fila), promover al siguiente (el más antiguo por `ASSIGNED_AT`) y setear `IS_PRIMARY=1`.

12. **Responsables:** `HANDOVERS.SENDER_USER_ID` identifica al emisor responsable único (primary del FROM shift, el primero asignado). El **receiver-of-record** es quien completa (`COMPLETED_BY_USER_ID`), no está fijado de antemano. `RECEIVER_USER_ID` es opcional (para referencia/UI) pero no se usa como constraint fuerte.

13. Al pasar a `Ready`: se setea `SENDER_USER_ID` desde `SHIFT_COVERAGE` del `FROM_SHIFT_INSTANCE` (primary o primero por `ASSIGNED_AT`). Al iniciar/completar: quien start/complete debe tener coverage en el TO shift (validación de app). El mismo doctor no puede ser emisor y receptor (DB enforced).

14. Otros usuarios pueden colaborar/editando pero no son responsables.

15. Regla encadenada: al completar, el receptor "toma" el pase y el próximo handover lo tendrá como emisor (conceptualmente; implementación por transiciones/turnos).

### Presencia (Presence)

16. **Presencia (presence):** "Quién está mirando/colaborando ahora" es **efímero** y **NO se persiste en DB**. Se maneja por RT/cliente. Otros usuarios pueden editar/colaborar (registrado en `HANDOVER_CONTENTS.LAST_EDITED_BY` y logs), pero no son responsables del pase.

17. **Presencia (presence) NO va a DB:** Los "participants/members tipo Google Docs" (presencia: quién mira/tipea ahora) es **efímero** y **NO se persiste** en DB. Se maneja por RT/cliente. La lista de "quién está en la conversación" es volátil y no requiere persistencia.

18. **Colaboración tipo Google Docs sin members:** La colaboración se modela con `HANDOVER_MESSAGES` (discusión), `HANDOVER_CONTENTS.LAST_EDITED_BY` (último editor), y logs de auditoría. No se necesita tabla `HANDOVER_MEMBERS` porque: (a) presence es efímero, (b) responsabilidad es exactamente 1+1 (ya en `HANDOVERS` como columnas).

---

## Coverage (Cobertura de Turnos)

19. "Coverage" significa: **"este doctor está a cargo de este paciente en este turno"**.

20. Un paciente puede tener **varios responsables en coverage** (entran/salen; multi-doctor).

21. Para tu app, **solo una persona puede desasignar** (decisión de UX), aunque el modelo puede soportar múltiples.

22. No puede haber handover "en el aire": **no puede existir handover sin coverage** (si no hay responsables asignados, no debería crearse). El comando que pasa a `Ready` debe hacer atómico: (1) verificar `SHIFT_COVERAGE` del `FROM_SHIFT_INSTANCE` → debe haber `>=1`, **elegir el primary** (o el primero por `ASSIGNED_AT` si no hay primary) como `SENDER_USER_ID`, (2) recién ahí setear `READY_AT`. **Importante:** La regla de selección del sender es "el primero que se asigna" (primary), estable y explicable ("figura como emisor porque fue el primero asignado en ese turno").

23. No se soporta cobertura parcial tipo **"me cubrís 2 horas"** (explícitamente fuera de scope).

---

## Creación y Gestión de Handovers

24. Los médicos **no crean handovers manualmente**: el handover se crea como **efecto secundario** de comandos del dominio (ej: asignar responsable / asegurar transición del día).

25. Regla de arquitectura: **un GET no debe crear** nada (evitar `GetOrCreate...` en lecturas).

26. La creación debe ser **idempotente** y "race-safe" usando **constraints + insert/merge + retry** desde la app (sin triggers).

27. Cuando un receptor se asigna pacientes para el próximo turno, el handover debería poder **estar disponible** (Draft) para completarse antes de la reunión.

28. No hay triggers en DB: cualquier regla que requiera "mirar otras filas" o "validación cruzada" se hace en la **app** (commands/workflow).

---

## Máquina de Estados

### Estados

29. Máquina de estados MVP (por timestamps): **Draft → Ready → InProgress → Completed**, con terminal **Cancelled**. Solo incluye estados **mecánicos**; no hay estados "humanos" como Rejected o Expired en el header.

30. **Ready** significa "listo para pasar" (semántica exacta aún no 100% cerrada, pero existe como etapa). Requiere que `SENDER_USER_ID` esté seteado (emisor responsable). El receiver-of-record se define al completar.

31. "InProgress" se interpreta como "**en la sala se empezó a tratar ese paciente**" (no como "hay gente conectada", porque presencia no se persiste).

32. El pase se realiza **paciente por paciente**: cada paciente tiene su "start" al ser tratado.

### Transiciones

33. Para pasar a **InProgress**: cualquier usuario con coverage en el TO shift puede iniciar. **DB lo fuerza** con constraint: `STARTED_BY_USER_ID` NO puede ser el `SENDER_USER_ID` (mismo doctor no puede ser emisor y receptor). La validación de que quien start tiene coverage en TO shift es **app-enforced**.

34. **Completed**: cualquier usuario con coverage en el TO shift puede completar. **DB lo fuerza** con constraint: `COMPLETED_BY_USER_ID` NO puede ser el `SENDER_USER_ID` (mismo doctor no puede ser emisor y receptor). La validación de que quien completa tiene coverage en TO shift es **app-enforced**. El `COMPLETED_BY_USER_ID` es el receiver-of-record.

35. **Cancelled**: puede cancelarse desde cualquier estado (incluso Draft). Si hay `CANCELLED_AT`, debe existir `CANCELLED_BY_USER_ID` y `CANCEL_REASON` (DB enforced). El `CANCEL_REASON` puede ser: `'AutoVoid_NoCoverage'`, `'Duplicate'`, `'ReceiverRefused'` (rechazo verdadero), u otros según reglas de negocio. Para cancelaciones automáticas (sistema), usar `CANCELLED_BY_USER_ID='system'` (usuario especial creado para este propósito).

36. **ReturnForChanges** (rechazo blando): no es un estado, es una **regla de app**. Si estaba `Ready` y alguien "devuelve para cambios": `READY_AT = NULL` (vuelve a Draft). Ventaja: no agrega estado nuevo, no complica constraints.

37. **ChangeReceiver**: cambio de receptor esperado. `RECEIVER_USER_ID` es opcional y puede actualizarse, pero no se usa como constraint fuerte. El receiver-of-record real es quien completa (`COMPLETED_BY_USER_ID`). No es un estado.

38. El rechazo verdadero (receptor se niega) se modela como **Cancel con `CANCEL_REASON='ReceiverRefused'`**. El rechazo blando (faltan cosas) se modela como **ReturnForChanges** (regla de app, no estado).

### Reglas Temporales

39. Regla temporal: `READY_AT >= CREATED_AT` y `STARTED_AT` requiere `READY_AT` (y típicamente `STARTED_AT >= READY_AT`).

40. Estados terminales deben ser **mutuamente excluyentes** (si se marca uno terminal no se pueden marcar otros a la vez). Los estados terminales globales son: **Completed** y **Cancelled**. Se puede cancelar desde cualquier estado, incluso Draft.

41. `COMPLETED_AT` requiere `STARTED_AT` (consistencia fuerte).

---

## Auditoría y Consistencia

42. Para histórico/auditoría: se quiere saber **quién marcó Ready** (`READY_BY_USER_ID`), **quién dio Start** (`STARTED_BY_USER_ID`), **quién dio Complete** (`COMPLETED_BY_USER_ID` = receiver-of-record), y **quién canceló** (`CANCELLED_BY_USER_ID`). Start y Complete NO pueden ser el sender (DB enforced).

43. Si se guardan `READY_BY_USER_ID` / `STARTED_BY_USER_ID` / `COMPLETED_BY_USER_ID` / `CANCELLED_BY_USER_ID`, debe ser consistente: si hay `READY_AT` entonces hay `READY_BY_USER_ID` (y análogo para started/completed/cancelled). DB lo fuerza con constraints.

44. **Denormalización de UNIT_ID:** `HANDOVERS.UNIT_ID` es un snapshot para scoping rápido. Debe coincidir con la unidad de la ventana (DB enforced). `SHIFT_COVERAGE.UNIT_ID` evita cruces de unidad vía FK compuesta.

---

## Validaciones

### DB-Enforced

45. **Validaciones DB-enforced:** `SHIFT_WINDOWS` une instancias de la **misma unidad** (DB enforced). `HANDOVERS.UNIT_ID` coincide con la unidad de la ventana (DB enforced). El mismo doctor no puede ser emisor y receptor (DB enforced).

### App-Enforced

46. **Qué queda app-enforced (sin triggers):** Para pasar a `Ready`: verificar `SHIFT_COVERAGE` del `FROM_SHIFT_INSTANCE` (`>=1`, elegir primary o primero por `ASSIGNED_AT` como `SENDER_USER_ID`), luego setear `READY_AT`. Al iniciar/completar: verificar que quien start/complete tiene coverage en el TO shift.

47. Que `PATIENT_ID` sea de esa unidad sigue siendo mejor **app-enforced**, porque si mañana el paciente cambia de unidad, no querés que se rompa el historial.

---

## Usuario System

48. Se recomienda crear un usuario especial `USERS.ID='system'` (o `'handover-bot'`) para representar acciones automáticas del sistema (ej: `AutoVoid_NoCoverage`). Este usuario se usa como `CANCELLED_BY_USER_ID` en cancelaciones automáticas, manteniendo auditoría completa sin permitir NULLs.

49. ✅ **IMPLEMENTADO:** El usuario system existe con `ID='system'`, `EMAIL='system@relevo.app'`, `FULL_NAME='System Bot'`, `ROLE='system'`.

---

## Interfaz de Usuario

50. "Mis pacientes": un usuario ve su lista y al click entra al handover en curso (modo doc).

51. También existe una vista global donde se pueden ver handovers de otros turnos/unidades (al menos lectura).

52. El **patient summary** es el bloque más estable y típicamente se **copia/arrastra** del handover previo al nuevo.

---

## Notas de Implementación

### ✅ Implementado

- **Selección de sender:** Implementada en `HandoverRepository.CreateHandoverAsync()` y `HandoverRepository.MarkAsReadyAsync()`.
- **Validación de coverage:** No se puede crear handover sin coverage (lanza excepción). No se puede pasar a Ready sin coverage (retorna `false`).
- **Promoción de primary:** Implementada en `AssignmentRepository.RemoveCoverageWithPrimaryPromotionAsync()`. Cuando se elimina un coverage que es primary, promueve al siguiente (más antiguo por `ASSIGNED_AT`).
- **Transiciones de estado:** Implementadas en `HandoverStateMachineHandlers` con validaciones de coverage y sender.
- **ReturnForChanges:** Implementado en `ReturnForChangesHandler`. Limpia `READY_AT` y `READY_BY_USER_ID` (vuelve a Draft).
- **Auto-creación de handovers:** Implementada mediante eventos de dominio `PatientAssignedToShiftEvent` y `PatientAssignedToShiftHandler`.

### ⚠️ Limitaciones Conocidas

- **Fechas futuras:** La infraestructura existe, pero el código de producción hardcodea `DateTime.Today`. No se pueden crear handovers o assignments para fechas futuras (solo hoy). Esto podría ser intencional para MVP.
