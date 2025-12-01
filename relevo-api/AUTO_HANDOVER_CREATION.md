# Creación Automática de Handovers - Implementación

## Resumen

Se implementó la creación automática de handovers usando **eventos de dominio** según V3_PLAN.md Regla #14.

## Arquitectura

### Flujo

```
AssignmentRepository.AssignPatientsAsync()
    ↓
    Crea SHIFT_COVERAGE
    ↓
    Publica PatientAssignedToShiftEvent (solo si IS_PRIMARY=1)
    ↓
    PatientAssignedToShiftHandler.Handle()
    ↓
    Determina siguiente turno (Day→Night, Night→Day)
    ↓
    Crea handover automáticamente en estado Draft
```

### Componentes

1. **Evento de Dominio**: `PatientAssignedToShiftEvent`
   - Se publica cuando un paciente es asignado a un turno
   - Solo se publica si la asignación es `IS_PRIMARY=1`

2. **Handler**: `PatientAssignedToShiftHandler`
   - Escucha `PatientAssignedToShiftEvent`
   - Determina el siguiente turno usando `IShiftTransitionService`
   - Crea el handover automáticamente si no existe

3. **Servicio de Transición**: `ShiftTransitionService`
   - Implementa `IShiftTransitionService`
   - Determina el siguiente turno (Day→Night, Night→Day)
   - Maneja turnos nocturnos que cruzan medianoche

## Archivos Creados

- `src/Relevo.Core/Events/PatientAssignedToShiftEvent.cs`
- `src/Relevo.Core/Handlers/PatientAssignedToShiftHandler.cs`
- `src/Relevo.Core/Interfaces/IShiftTransitionService.cs`
- `src/Relevo.Infrastructure/Data/ShiftTransitionService.cs`

## Archivos Modificados

- `src/Relevo.Infrastructure/Data/AssignmentRepository.cs`
  - Publica `PatientAssignedToShiftEvent` después de crear coverage
  - Solo publica si `IS_PRIMARY=1`

- `src/Relevo.Infrastructure/InfrastructureServiceExtensions.cs`
  - Registra `IShiftTransitionService` en DI

## Características

### Idempotencia

- Verifica si ya existe handover para `(PATIENT_ID, SHIFT_WINDOW_ID)` antes de crear
- Si existe, no crea duplicado (maneja race conditions)
- El constraint `UQ_HO_PAT_WINDOW` en DB también previene duplicados

### Manejo de Errores

- Si la creación del handover falla, **no falla la asignación**
- Los errores se registran pero no bloquean el flujo principal
- El handover puede crearse manualmente después si es necesario

### Estado del Handover

- Se crea en estado **`Draft`**
- `SENDER_USER_ID` se setea automáticamente desde `SHIFT_COVERAGE` (primary)
- `RECEIVER_USER_ID` puede ser `NULL` inicialmente (se define al completar)
- `CREATED_BY_USER_ID` = usuario que asignó el paciente

## Reglas de Negocio Implementadas

✅ **V3_PLAN.md Regla #14**: Handovers se crean como efecto secundario de comandos del dominio

✅ **V3_PLAN.md Regla #9**: Máximo 1 handover activo por paciente por ventana (idempotencia)

✅ **V3_PLAN.md Regla #10**: No puede existir handover sin coverage (validado en creación)

✅ **V3_PLAN.md Regla #41**: Sender es el primary del FROM shift

## Próximos Pasos (Opcional)

1. **Creación al "asegurar transición del día"**: Si se requiere funcionalidad adicional para crear handovers en batch al inicio del día

2. **Notificaciones**: Agregar notificaciones cuando se crea un handover automáticamente

3. **Métricas**: Agregar métricas/telemetría para monitorear creación automática de handovers

## Testing

Para probar la funcionalidad:

1. Asignar un paciente a un turno (Day o Night) como primary
2. Verificar que se crea automáticamente un handover en estado Draft
3. Verificar que el handover tiene el siguiente turno como TO shift
4. Verificar que no se crean duplicados si se asigna el mismo paciente múltiples veces

