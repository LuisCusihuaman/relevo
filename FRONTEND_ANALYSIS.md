# Análisis de la Vista de Handover

Este documento detalla el estado actual de la vista de Handover (`relevo-frontend/src/pages/handover.tsx` y componentes relacionados), identificando lógica harcodeada, "fake features" y problemas de persistencia.

## 🟢 Componentes Conectados (Funcionan)

Los siguientes componentes están correctamente integrados con la API (`relevo-api`):

1.  **Patient Summary (`PatientSummary.tsx`)**
    - Usa `useUpdatePatientData` para guardar el resumen.
    - Soporta edición y visualización.
    - **Nota:** Envía `illnessSeverity` al guardar, pero parece usar el valor original (`patientDataProp?.illnessSeverity`) o "stable" por defecto, sin recibir actualizaciones del componente `IllnessSeverity`.

2.  **Action List (`ActionList.tsx`)**
    - Totalmente funcional.
    - Usa `useActionItems` para CRUD de tareas (Create, Update, Delete).
    - Gestiona estados de completado y prioridades.

3.  **Situation Awareness (`SituationAwareness.tsx`)**
    - Usa `useSituationAwareness` y `useUpdateSituationAwareness`.
    - Gestiona Planes de Contingencia con `useCreateContingencyPlan` y `useDeleteContingencyPlan`.
    - **Detalle:** Usa un template de texto harcodeado (`getDefaultSituationText`) si no hay datos, lo cual es comportamiento esperado (placeholder).

4.  **Autenticación (`useCurrentPhysician.ts`)**
    - Integrado con Clerk. Obtiene el usuario real.

## 🔴 Componentes con Lógica "Fake" o Sin Persistencia

1.  **Illness Severity (`IllnessSeverity.tsx`)**
    - **Problema Crítico:** La selección de severidad (Stable/Watcher/Unstable) **no se guarda**.
    - Mantiene el estado localmente (`selectedSeverity`) pero no llama a ninguna API ni propaga el cambio al padre para que `PatientSummary` lo guarde.
    - **Fake Real-time:** Contiene lógica explícita para simular actualizaciones en tiempo real (`simulateRealtimeUpdate` con `setInterval` y `Math.random`), mostrando indicadores visuales falsos de que "otro usuario editó".

2.  **Synthesis By Receiver (`SynthesisByReceiver.tsx`)**
    - **Problema de Persistencia:** El checklist de confirmación (6 items: "Illness Severity", "Clinical Background", etc.) maneja su estado (`checked`) puramente en memoria (`useState`).
    - **Consecuencia:** Si el usuario recarga la página antes de completar todo el checklist, **pierde todo el progreso**.
    - La confirmación final ("Aceptar Responsabilidad") llama a un prop `onComplete`, pero en `MainContent.tsx` no parece haber una lógica conectada para disparar la transición de estado del handover en el backend.

## ⚠️ Identificación y Permisos Incorrectos (Nombres vs IDs)

Existe un problema sistémico en cómo se identifican los médicos ("Assigned" y "Receiving") y se calculan los permisos:

1.  **Comparación por Nombre (Inseguro):**
    - Componentes como `IllnessSeverity`, `ActionList`, `SituationAwareness` y `SynthesisByReceiver` determinan permisos comparando `currentUser.name === assignedPhysician.name`.
    - Esto es frágil y propenso a errores (homónimos, formatos de nombre diferentes). **Debería compararse por `userId`**.

2.  **Información del "Receiving Physician" (Incorrecta/Harcodeada):**
    - En `useCurrentHandover.ts`, el `receivingPhysician` se deriva incorrectamente de `patientData` (datos generales del paciente) usando `formatPhysician`.
    - `formatPhysician` (en `formatters.ts`) **elimina el ID** del usuario, retornando solo nombre, iniciales y rol.
    - El Handover real tiene un campo `receiverUserId` (y `assignedTo`) que **se está ignorando**.
    - Consecuencia: `SynthesisByReceiver` no valida contra el receptor real del traspaso, sino contra un dato potencialmente desactualizado del paciente o nulo.

3.  **Información del "Responsible Physician" (Turno de Guardia):**
    - Aunque `assignedPhysician` intenta usar `handoverData.responsiblePhysicianId`, tiene un fallback a `patientData?.assignedPhysician` que pasa por `formatPhysician`, perdiendo el ID si el handover no ha cargado completamente o si se usa el fallback.
    - Esto causa inconsistencia: a veces se tiene ID, a veces no.

## ⛔️ Ciclo de Vida del Handover Incompleto (Sin Transiciones)

Actualmente, **es imposible avanzar el estado del Handover desde la interfaz de usuario**.

1.  **Hooks Desconectados:**
    - Los hooks de mutación `useReadyHandover`, `useStartHandover`, `useAcceptHandover` y `useCompleteHandover` existen en `api/endpoints/handovers.ts` pero **NO se importan ni utilizan** en ninguna parte de la vista de Handover (`handover.tsx`, `MainContent.tsx`, `Header.tsx`).

2.  **Falta de Controles UI:**
    - No existen botones visibles para "Iniciar Handover", "Marcar como Listo" o "Aceptar".
    - El botón de "Confirmar y Aceptar" en `SynthesisByReceiver` llama a una prop `onComplete` que **no está conectada** en `MainContent.tsx`.

3.  **Ciclo Bloqueado:**
    - Un handover creado se queda en estado `Draft` (o el inicial) para siempre, a menos que se cambie vía API directa (Swagger/Postman).
    - La UI no refleja acciones disponibles según el estado actual (ej: si está `Ready`, el receptor debería ver un botón para "Empezar").

## 🟡 Áreas de Mejora / Deuda Técnica

1.  **Desconexión entre Severity y Summary:**
    - El backend espera `illnessSeverity` al actualizar el `PatientData`.
    - Sin embargo, el componente UI de `IllnessSeverity` está aislado. Al guardar el `PatientSummary`, se envía el valor antiguo o default, ignorando lo que el usuario haya seleccionado en los botones de severidad.

2.  **Falta de "Real" Real-time:**
    - La aplicación depende de `react-query` y revalidación (polling o invalidación manual) para actualizar datos.
    - Los indicadores visuales de "Live Update" en `IllnessSeverity` son falsos.

## 📋 Plan de Acción para Handover

1.  **Implementar Ciclo de Vida (Prioridad Alta):**
    - **Crear `HandoverStatusControls`:** Un nuevo componente en el Header o MainContent que muestre botones de acción según el estado (`Draft` -> `Ready`, `Ready` -> `InProgress`, etc.).
    - **Conectar `SynthesisByReceiver`:** Pasar una función a `onComplete` en `MainContent` que llame a `useCompleteHandover`.
    - **Integrar Hooks:** Usar los hooks de transición existentes en estos controles.

2.  **Corregir Identificación de Usuarios:**
    - **Actualizar `formatPhysician`:** Modificar para que preserve y retorne el `id`.
    - **Corregir `useCurrentHandover`:**
      - Obtener `receivingPhysician` preferentemente de `handoverData.receiverUserId` (buscando detalles del usuario si es necesario) o `handoverData.assignedTo`.
      - Asegurar que `assignedPhysician` (responsable del turno) siempre tenga ID válido proveniente del Handover.
    - **Refactorizar Componentes:** Cambiar todas las comparaciones de `name` a `id`.

3.  **Conectar Illness Severity:**
    - Elevar el estado de `illnessSeverity` a `MainContent` o usar un store (Zustand) compartido.
    - Asegurar que cuando se cambie la severidad, se llame a `updatePatientData`.

4.  **Persistir Checklist de Síntesis:**
    - Backend: Verificar si existe endpoint para guardar el progreso del checklist o si se debe guardar como metadata del Handover.
    - Frontend: Conectar los checkboxes a la API para que el progreso no se pierda.

# Análisis del Frontend (Relevo)

Este documento detalla las áreas del frontend (`relevo-frontend`) donde existe lógica harcodeada, datos estáticos que deberían provenir de la API, y funcionalidades faltantes o incompletas en comparación con el backend.

## 🚨 Datos Harcodeados y Mockeados

La fuente principal de datos harcodeados es el archivo `src/pages/data.ts`. Estos datos se utilizan para simular funcionalidades que aún no están integradas con el backend.

### 1. Dashboard (Home)

- **Actividad Reciente (Activity Feed):**
  - **Estado Actual:** Se utiliza `recentPreviews` importado de `data.ts` en `DashboardSidebar.tsx`.
  - **Problema:** Muestra eventos estáticos ("New patient assigned", "Severity changed") que no reflejan la actividad real del sistema.
  - **Backend:** Existe `GetHandoverActivityLog` (para un handover específico), pero no parece haber un endpoint para un "Global Activity Feed" del usuario o unidad.
  - **Ubicación:** `src/components/home/DashboardSidebar.tsx` -> `RecentActivityCard.tsx`.

- **Métricas del Dashboard:**
  - **Estado Actual:** El objeto `metrics` en `data.ts` contiene valores fijos (Pacientes asignados: 12, Handovers en progreso: 5, etc.).
  - **Problema:** Los números no son reales.
  - **Backend:** `GetMyPatients` y `GetPendingHandovers` podrían usarse para calcular algunas métricas, pero faltan endpoints agregados para estadísticas.

- **Búsqueda Global (Command Palette):**
  - **Estado Actual:** El componente `CommandPalette.tsx` filtra sobre una lista estática `searchResults` definida en `data.ts`.
  - **Problema:** La búsqueda es puramente local y ficticia. No busca pacientes ni handovers reales en la base de datos.
  - **Backend:** El endpoint `GetAllPatients` actualmente solo soporta paginación, no búsqueda por texto.

### 2. Lista de Pacientes (Patient Directory)

- **Datos de Visualización:**
  - **Estado Actual:** Aunque `Home.tsx` usa el hook `useAssignedPatients` para traer pacientes reales, el componente de visualización `PatientDirectoryList.tsx` rellena datos faltantes con valores harcodeados.
  - **Detalle:**
    - `unit`: Se muestra siempre como "Assigned" (harcodeado en línea 115).
    - `date`: Se muestra siempre la fecha actual `new Date()` (línea 116).
  - **Solución:** Mapear correctamente la unidad y la fecha de admisión/asignación desde la respuesta de la API.

### 3. Perfil del Paciente (Patient Profile Header)

- **Estado Actual:** El componente `PatientProfileHeader.tsx` es un placeholder visual que imita un dashboard de Vercel.
- **Problema Crítico:** Muestra información irrelevante para el dominio médico: "Production Deployment", "Build Logs", "Runtime Logs", "Instant Rollback".
- **Solución:** Reemplazar completamente este componente para mostrar datos demográficos y clínicos del paciente (`GetPatientById` / `GetPatientSummary`).

## 🛠 Funcionalidades Faltantes o Incompletas

### Backend vs Frontend Gap

| Funcionalidad                | Backend (`relevo-api`)                        | Frontend (`relevo-frontend`)            | Estado                                                             |
| ---------------------------- | --------------------------------------------- | --------------------------------------- | ------------------------------------------------------------------ |
| **Búsqueda de Pacientes**    | `GetAllPatients` (Solo paginación)            | `CommandPalette` (Harcodeado)           | 🔴 Falta soporte de búsqueda en Backend e integración en Frontend. |
| **Feed de Actividad Global** | No existe endpoint global (solo por handover) | Mock estático en `DashboardSidebar`     | 🔴 Requiere nuevo endpoint o agregación en frontend.               |
| **Métricas**                 | Datos dispersos en varios endpoints           | Mock estático en `data.ts`              | 🟡 Requiere lógica de cálculo o endpoint dedicado.                 |
| **Detalle de Paciente**      | `GetPatientById`, `GetPatientSummary`         | Header con datos de "Deployment" falsos | 🔴 UI Incorrecta/Placeholder.                                      |
| **Notificaciones**           | Modelo de datos existe                        | No visible en la UI                     | ⚪️ Pendiente de implementación.                                    |

## 📋 Plan de Acción Recomendado

1.  **Limpieza de `data.ts`:** Eliminar progresivamente las exportaciones de este archivo a medida que se conecten con la API.
2.  **Integrar Perfil de Paciente:** Reescribir `PatientProfileHeader.tsx` para usar los datos reales de `usePatientDetails` (nombre, MRN, edad, diagnóstico, unidad).
3.  **Corregir Lista de Pacientes:** Actualizar `PatientDirectoryList.tsx` para mostrar la Unidad real y la fecha real provenientes de `PatientSummaryCard`.
4.  **Implementar Búsqueda Real:**
    - Backend: Añadir filtro `SearchTerm` a `GetAllPatientsRequest`.
    - Frontend: Conectar `CommandPalette` a la API de búsqueda.
5.  **Desarrollar Feed de Actividad:** Evaluar si se necesita un endpoint `/me/activity` en el backend o si se puede construir con la data existente.
