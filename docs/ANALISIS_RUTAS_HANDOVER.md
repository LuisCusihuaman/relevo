# RFC: Cambio de Modelo de Rutas del Módulo de Handover

## Estado de Implementación

| PR | Descripción | Estado |
|----|-------------|--------|
| PR 1 | Infraestructura de Rutas & Hooks | ✅ **COMPLETADO** |
| PR 2 | Navegación desde Pacientes | ✅ **COMPLETADO** (incluido en PR1) |
| PR 3 | Historial - Botón "Ver completo" | ⏳ Pendiente |
| PR 4 | Modo Solo Lectura en Componentes | ⏳ Pendiente |

---

## Resumen Ejecutivo

La ruta anterior `/$patientSlug/$handoverId` **violaba el modelo mental de negocio**. Los usuarios piensan en "entrar al paciente", no en "entrar al handover 9108f2...". 

**Nueva estructura implementada**:
- `/patient/$patientId` → Handover activo (resolución automática)
- `/patient/$patientId/history/$handoverId` → Handover histórico (solo lectura)

**Decisión clave**: No se requirieron cambios de backend. La solución usa endpoints existentes.

---

## 1. Problema Original (RESUELTO ✅)

### Ruta Anterior (Eliminada)
```
/$patientSlug/$handoverId
Ejemplo: /maria-garcia/9108f213-5017-40e2-ac6a-81aaf67aee74
```

### Por Qué Era Problemático

| Problema | Consecuencia | Estado |
|----------|--------------|--------|
| URL contenía `handoverId` técnico | Cuando el handover se completaba → URL quedaba zombie | ✅ Resuelto |
| Encadenamiento invisible | Al completar, el nuevo handover no se reflejaba en navegación | ✅ Resuelto |
| El usuario debía "conocer" el ID | Violación de Regla 50: "al click entra al handover en curso" | ✅ Resuelto |
| Múltiples fetches innecesarios | `PatientDirectoryList` ignoraba `patient.handoverId` | ✅ Resuelto |

### Reglas de Negocio Que Se Violaban (Ahora Cumplidas)

- **Regla 3**: Máximo 1 handover activo por paciente → ✅ URL no requiere ID
- **Regla 50**: "Mis pacientes: al click entra al handover en curso" → ✅ Navegación directa a `/patient/$patientId`
- **Regla 15**: Encadenamiento de handovers → ✅ Hook resuelve automáticamente el activo
- **Regla 24**: Handovers se crean automáticamente → ✅ Frontend no requiere ID

---

## 2. Diseño de Rutas Propuesto

### Nuevas Rutas

| Ruta | Propósito | Modo |
|------|-----------|------|
| `/patient/$patientId` | Handover activo del paciente | Lectura/Escritura |
| `/patient/$patientId/history/$handoverId` | Handover histórico | Solo Lectura |

### Semántica

```
/patient/$patientId
├── Canon: "mostrar el handover activo para ESTE paciente y ESTE usuario"
├── Si no hay activo → UI de "no hay handover activo" + hints
└── La resolución del handover activo es responsabilidad del frontend

/patient/$patientId/history/$handoverId
├── Frontend usa /handovers/{handoverId} por debajo
├── Solo lectura (inputs disabled, botones de acción ocultos)
└── Banner: "Registro histórico – no editable"
```

### Ruta Vieja: Eliminación Directa

La ruta `/$patientSlug/$handoverId` se **elimina directamente** (big bang).
Se reemplaza por las nuevas rutas en un solo cambio.

---

## 3. Historial: Ruta + Modal (Híbrido)

### Enfoque Adoptado

- **La ruta existe** (compartible, linkeable)
- **La presentación varía** según dispositivo:
  - Mobile: full page
  - Desktop: modal/sheet superpuesto (opcional)
- TanStack Router permite ambos comportamientos

### UI del Historial

```
┌─────────────────────────────────┐
│ Historial de Traspasos          │
├─────────────────────────────────┤
│ ▸ Turno Día - 09/12/2024        │
│   De: Dr. García → Dr. López    │
│   Estado: Completado            │
│   [Ver completo →]              │  ← Link a /patient/X/history/Y
├─────────────────────────────────┤
```

### Página Histórica

```tsx
function HistoricalHandoverPage() {
  const { patientId, handoverId } = useParams();
  const { data: handover } = useHandover(handoverId);

  return (
    <>
      <Alert variant="warning">
        <AlertTitle>Registro histórico</AlertTitle>
        <AlertDescription>
          Handover completado el {formatDate(handover.completedAt)}.
          Los datos no pueden modificarse.
        </AlertDescription>
      </Alert>

      {/* Reusar componentes con readOnly=true */}
      <HandoverLayout handoverId={handoverId} patientId={patientId} readOnly />

      <Button
        variant="outline"
        onClick={() => navigate({ to: "/patient/$patientId", params: { patientId } })}
      >
        Volver al handover activo
      </Button>
    </>
  );
}
```

### Modo Solo Lectura

Cuando `readOnly === true`:
- No mostrar botones de guardar
- Deshabilitar textareas/inputs
- No permitir crear/editar Contingency Plans / Action Items
- Ocultar acciones de estado (ready/start/complete)

---

## 4. Resolución del Handover Activo

### Decisión: Sin Nuevo Endpoint de Backend

El backend ya provee la información necesaria:
- `/me/patients` retorna `handoverId` del handover activo
- `/patients/{id}/handovers` retorna timeline completo

### Hook `usePatientCurrentHandover`

```typescript
function usePatientCurrentHandover(patientId: string) {
  const { data: timeline, ...rest } = usePatientHandoverTimeline(patientId);

  const currentHandover = useMemo(() => {
    if (!timeline?.items) return null;
    return timeline.items.find((h) =>
      ["Draft", "Ready", "InProgress"].includes(h.stateName)
    ) ?? null;
  }, [timeline]);

  return { currentHandover, ...rest };
}
```

**Ventaja**: El mismo timeline sirve para el historial.

### Página Principal

```tsx
function PatientHandoverPage() {
  const { patientId } = useParams();
  const { currentHandover, isLoading } = usePatientCurrentHandover(patientId);

  if (isLoading) return <Spinner />;

  if (!currentHandover) {
    return <NoActiveHandoverUI patientId={patientId} />;
  }

  return (
    <HandoverPage
      handoverId={currentHandover.id}
      patientId={patientId}
      readOnly={false}
    />
  );
}
```

### Post-Complete: Invalidar y Dejar que el Hook Decida

```typescript
const completeMutation = useCompleteHandover({
  onSuccess: () => {
    queryClient.invalidateQueries(patientQueryKeys.handoverTimelineById(patientId));
    // No navigate: la URL sigue siendo /patient/$patientId
    // El hook reevalúa cuál es el activo y se re-renderiza solo
  },
});
```

**Esto respeta la regla de negocio**: "al completar, el receptor toma el pase y el próximo handover lo tendrá como emisor" → solo invalidás, el backend decide y el FE refleja.

---

## 5. Flujo de Navegación

```
┌───────────────────────────────────────────────────────────────────┐
│ /me/patients                                                      │
│ Retorna: { id, name, handoverStatus, handoverId }                │
└───────────────────────┬───────────────────────────────────────────┘
                        │
                        │ Click en paciente (simple, sin fetch extra)
                        ▼
┌───────────────────────────────────────────────────────────────────┐
│ navigate('/patient/{patientId}')                                  │
│ URL: /patient/pat-001                                             │
└───────────────────────┬───────────────────────────────────────────┘
                        │
                        │ usePatientHandoverTimeline(patientId)
                        ▼
┌───────────────────────────────────────────────────────────────────┐
│ Filter: stateName in ['Draft', 'Ready', 'InProgress']            │
└───────────────────────┬───────────────────────────────────────────┘
                        │
            ┌───────────┴───────────┐
            │                       │
            ▼                       ▼
    ┌───────────────┐       ┌───────────────────┐
    │ Renderizar    │       │ Mostrar UI:       │
    │ HandoverPage  │       │ "Sin handover     │
    │               │       │  activo"          │
    └───────┬───────┘       └───────────────────┘
            │
            │ Completar handover
            ▼
┌───────────────────────────────────────────────────────────────────┐
│ invalidateQueries → Hook reevalúa → Nuevo handover aparece        │
│ URL permanece: /patient/pat-001 ✅                                │
└───────────────────────────────────────────────────────────────────┘
```

---

## 6. Plan de Implementación (PRs)

### PR 1: Infraestructura de Rutas & Hooks ✅ COMPLETADO

**Archivos nuevos:**
- `src/routes/_authenticated/patient.$patientId.tsx` ✅
- `src/routes/_authenticated/patient.$patientId.history.$handoverId.tsx` ✅
- `src/hooks/usePatientCurrentHandover.ts` ✅
- `src/pages/patient-handover.tsx` ✅
- `src/pages/historical-handover.tsx` ✅

**Archivos eliminados:**
- `src/routes/_authenticated/$patientSlug.$handoverId.tsx` ✅ ELIMINADO

**Archivos modificados:**
- `src/components/handover/hooks/useCurrentHandover.ts` ✅ (soporte nuevas rutas)
- `src/components/handover/components/FullscreenEditor.tsx` ✅ (usa useCurrentHandover)
- `src/assets/locales/es/handover.json` ✅ (traducciones)
- `src/assets/locales/en/handover.json` ✅ (traducciones)

### PR 2: Navegación desde Pacientes ✅ COMPLETADO (incluido en PR1)

**Archivos modificados:**
- `src/components/home/PatientDirectoryList.tsx` ✅
- `src/pages/Patients.tsx` ✅

**Cambios:**
- Navegación simplificada a `/patient/$patientId` (sin fetch extra)

### PR 3: Historial Conectando Todo ⏳ PENDIENTE

**Archivos a modificar:**
- `src/components/handover/components/HandoverHistory.tsx`

**Cambios pendientes:**
- Agregar botón "Ver completo" en cada item del historial
- Navegar a `/patient/$patientId/history/$handoverId`

### PR 4: Modo Solo Lectura en Componentes ⏳ PENDIENTE

**Archivos a modificar:**
- `src/components/handover/layout/MainContent.tsx`
- `src/components/handover/components/PatientSummary.tsx`
- `src/components/handover/components/ActionList.tsx`
- `src/components/handover/components/SituationAwareness.tsx`
- `src/components/handover/components/IllnessSeverity.tsx`

**Cambios pendientes:**
- Agregar prop `readOnly` a todos los componentes editables
- Deshabilitar inputs, ocultar botones de acción cuando `readOnly=true`

---

## 7. Riesgos y Consideraciones

### Timeline Paginado

El hook `usePatientHandoverTimeline` trae solo la primera página.

**Asunción**: El handover activo siempre aparece en el top de la lista.

**Mitigación**: 
- Documentar en backend: "handover activo siempre primero"
- O pedir `pageSize` pequeño pero garantizado que incluye activos

### Múltiples Handovers Activos

Regla dice "max 1 por paciente+ventana", pero:
- ¿Existen handovers tipo Consult / TemporaryCoverage en paralelo?
- Si sí → `/patient/$patientId` debería mostrar "el de este doctor/contexto"
- Si no → Documentar: "se asume 0 o 1 activo relevante"

### Slug vs ID en URL

Propuesta pasa de `/maria-garcia/uuid` a `/patient/uuid`.

**Nice-to-have futuro**: `/patient/$patientId-$slug` para URLs más legibles.
No es imprescindible ahora.

---

## 8. Archivos Modificados (Resumen)

| Archivo | PR | Estado |
|---------|-----|--------|
| `src/routes/_authenticated/patient.$patientId.tsx` | 1 | ✅ NUEVO |
| `src/routes/_authenticated/patient.$patientId.history.$handoverId.tsx` | 1 | ✅ NUEVO |
| `src/hooks/usePatientCurrentHandover.ts` | 1 | ✅ NUEVO |
| `src/pages/patient-handover.tsx` | 1 | ✅ NUEVO |
| `src/pages/historical-handover.tsx` | 1 | ✅ NUEVO |
| `src/routes/_authenticated/$patientSlug.$handoverId.tsx` | 1 | ✅ ELIMINADO |
| `src/components/handover/hooks/useCurrentHandover.ts` | 1 | ✅ Actualizado |
| `src/components/handover/components/FullscreenEditor.tsx` | 1 | ✅ Actualizado |
| `src/assets/locales/es/handover.json` | 1 | ✅ Traducciones |
| `src/assets/locales/en/handover.json` | 1 | ✅ Traducciones |
| `src/components/home/PatientDirectoryList.tsx` | 2 | ✅ Navegación |
| `src/pages/Patients.tsx` | 2 | ✅ Navegación |
| `src/components/handover/components/HandoverHistory.tsx` | 3 | ⏳ Pendiente |
| `src/components/handover/layout/MainContent.tsx` | 4 | ⏳ Pendiente |
| `src/components/handover/components/PatientSummary.tsx` | 4 | ⏳ Pendiente |
| `src/components/handover/components/ActionList.tsx` | 4 | ⏳ Pendiente |
| `src/components/handover/components/SituationAwareness.tsx` | 4 | ⏳ Pendiente |
| `src/components/handover/components/IllnessSeverity.tsx` | 4 | ⏳ Pendiente |

---

## 9. Beneficios

1. **URLs estables**: `/patient/pat-003` siempre lleva al handover activo
2. **Encadenamiento automático**: Al completar, el nuevo handover aparece sin cambio de URL
3. **Modelo mental correcto**: Usuario piensa en "paciente", no en "handover X"
4. **Sin cambios de backend**: Usa endpoints existentes
5. **Historial navegable y compartible**: URLs separadas para históricos

---

## Anexo A: Arquitectura Actual de Backend

### Endpoints Existentes (Sin Cambios Necesarios)

```
/me/patients                          GET  → Lista pacientes (incluye handoverId!)
/me/handovers                         GET  → Lista handovers del usuario
/patients/{patientId}                 GET  → Detalle de paciente
/patients/{patientId}/handovers       GET  → Timeline de handovers
/handovers/{handoverId}               GET  → Detalle del handover
/handovers/{handoverId}/patient       GET  → Datos del paciente en handover
/handovers/{handoverId}/ready         POST → Marcar como Ready
/handovers/{handoverId}/start         POST → Iniciar handover
/handovers/{handoverId}/complete      POST → Completar handover
```

### Dato Clave: `handoverId` Ya Viene en `/me/patients`

```csharp
// GetMyPatients.cs
public record PatientSummaryCard
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string HandoverStatus { get; init; } = "not-started";
    public string? HandoverId { get; init; }  // ✅ YA EXISTE
}
```

### Método Existente (No Expuesto, No Necesario Exponerlo)

```csharp
// HandoverRepository.Queries.cs
public async Task<string?> GetCurrentHandoverIdAsync(string patientId)
// Retorna el handover activo - existe pero no hace falta endpoint
```

---

## Anexo B: Hooks Frontend

```typescript
// patients.ts (existentes)
useAllPatients()                       → Lista todos los pacientes
useAssignedPatients()                  → Lista pacientes asignados
usePatientDetails(patientId)           → Detalle de un paciente
usePatientHandoverTimeline(patientId)  → Timeline de handovers

// hooks/usePatientCurrentHandover.ts ✅ NUEVO
usePatientCurrentHandover(patientId)   → Resuelve handover activo desde timeline
                                        → Retorna { currentHandover, hasActiveHandover, isLoading, error, timeline }

// handovers.ts (existentes)
useHandover(handoverId)                → Detalle de handover
usePatientHandoverData(handoverId)     → Datos del paciente en handover
useCompleteHandover()                  → Mutation para completar

// components/handover/hooks/useCurrentHandover.ts ✅ ACTUALIZADO
useCurrentHandover()                   → Detecta ruta automáticamente
                                        → Soporta /patient/$patientId y /patient/$patientId/history/$handoverId
                                        → Retorna { handoverId, patientId, handoverData, patientData, ... }
```

---

## TL;DR

- ✅ Ruta anterior violaba modelo de negocio → **CORREGIDO**
- ✅ Nueva estructura implementada: `/patient/$patientId` + `/patient/$patientId/history/$handoverId`
- ✅ Sin cambios de backend: usa `/me/patients` + timeline + hook `usePatientCurrentHandover`
- ✅ Ruta vieja eliminada (big bang)
- ⏳ Modo solo lectura para históricos → Pendiente (PR 3 y 4)
- **Progreso**: 2 de 4 PRs completados
