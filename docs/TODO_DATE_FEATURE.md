# Análisis: Implementación de Soporte para Fechas Futuras (Future-Shifts Support)

## Resumen Ejecutivo

La infraestructura base **ya existe** (`ShiftInstanceCalculationService` acepta `baseDate`), pero está hardcodeada a `DateTime.Today` en los puntos de entrada. Este documento analiza cómo implementar soporte para fechas futuras mediante un **big-bang refactor** (sin código productivo, no requiere backward compatibility).

**Nota de Diseño**: Este documento incorpora recomendaciones clave para evitar bugs sutiles relacionados con timezone y semántica de fechas, especialmente con night shifts que cruzan medianoche.

**⚠️ 3 "Must" Antes de Implementar Completo** (Feedback Crítico):

1. **Fuente de Verdad Única para "Hoy"**: Elegir UNA fuente (app vs DB) y no mezclarla. Si usás `TRUNC(SYSDATE)` en lecturas pero `_clock.Today()` en escrituras, aparecerán casos raros tipo "asigné hoy pero no me sale en el listado" alrededor de medianoche/DST. **Recomendación**: Usar `IClock` (app timezone) en todo y pasar `todayStart` como parámetro en queries.

2. **Validación de Coherencia Temporal en SHIFT_WINDOWS**: Validar `from.StartAt < to.StartAt` y `from.EndAt <= to.StartAt` al crear shift windows (aunque sea app-enforced). Si se cuela una ventana "al revés", todo lo demás se vuelve confuso.

3. **Get-or-Create Idempotente Real**: Implementar patrón insert-then-select con manejo de `ORA-00001` (unique violation) y re-select. Sin esto, bajo carga vas a terminar con excepciones intermitentes o duplicación si te falta una unique constraint.

---

## Estado Actual

### ✅ Infraestructura Existente

1. **`ShiftInstanceCalculationService`**:
   - ✅ Ya acepta `baseDate` como parámetro
   - ✅ Maneja correctamente night shifts que cruzan medianoche
   - ✅ Calcula correctamente `toShiftStartAt` y `toShiftEndAt` basado en `baseDate`

2. **`ShiftInstanceRepository`**:
   - ✅ `GetOrCreateShiftInstanceAsync` acepta `DateTime startAt, DateTime endAt`
   - ✅ No tiene restricciones de fecha (puede crear instancias para cualquier fecha)

### ❌ Limitaciones Actuales

1. **`AssignmentRepository.AssignPatientsAsync()`**:
   ```csharp
   // Línea 92: Hardcodeado
   var today = DateTime.Today;
   ```

2. **`HandoverRepository.CreateHandoverAsync()`**:
   ```csharp
   // Línea 235: Hardcodeado
   var today = DateTime.Today;
   var shiftDates = await ShiftInstanceCalculationService.CalculateShiftInstanceDatesFromDbAsync(
     conn, request.FromShiftId, request.ToShiftId, today);
   ```

3. **Auto-handover creation** (`PatientAssignedToShiftHandler`):
   - Usa `CreateHandoverAsync` que hardcodea `DateTime.Today`
   - No puede crear handovers para fechas futuras automáticamente

---

## Propuesta de Implementación (Big-Bang Refactor)

### Enfoque: DateOnly para Fechas Base + IClock para Validaciones

**Ventajas del Big-Bang**:
- ✅ Código más limpio y explícito
- ✅ No necesita lógica condicional de "si es null, usar Today"
- ✅ API más clara (fecha siempre presente)
- ✅ Menos complejidad en validaciones
- ✅ **Evita bugs de timezone**: `DateOnly` elimina ambigüedad de "qué día es"
- ✅ **Repositorios puros**: Validaciones en handlers, no en repos

**Estrategia**:
1. **`DateOnly`** en API/Commands (no `DateTime`) para fechas base
2. **`IClock`** para obtener "hoy" (no `DateTime.Today` hardcodeado)
3. **Validaciones en handlers**, no en repositorios
4. **Semántica clara**: `baseDate` = fecha del inicio del FROM shift
5. **Auto-handover**: Política clara (hoy + mañana, configurable)

---

## Implementación Detallada

### 1. Cambios en DTOs/Requests

#### `PostAssignmentsRequest` (POST /me/assignments)

```csharp
public class PostAssignmentsRequest
{
    public string ShiftId { get; set; } = string.Empty;
    public List<string>? PatientIds { get; set; }
    
    // NUEVO: Fecha opcional en API (string "YYYY-MM-DD")
    // En el endpoint, se parsea a DateOnly antes de pasar al command
    public string? AssignmentDate { get; set; } // "YYYY-MM-DD" formato ISO 8601
}
```

**Estrategia de Input JSON**:
- DTOs usan **string** para fechas (cero magia del serializer)
- Contrato HTTP claro: `"YYYY-MM-DD"` (ISO 8601)
- Parseo explícito en endpoint antes de normalizar

**Validación en Endpoint**:
- Si `AssignmentDate` es null/empty → usar `_clock.Today()` (default)
- Si `AssignmentDate` es inválido → error 400 "Invalid date format. Expected YYYY-MM-DD"
- Si `AssignmentDate` es pasado → error 400 "Cannot assign patients to past dates"
- Si `AssignmentDate` es muy futuro (>30 días) → error 400 "Cannot assign patients more than 30 days in advance" (opcional)

#### `CreateHandoverRequestDto` (POST /handovers)

```csharp
public class CreateHandoverRequestDto
{
    public string PatientId { get; set; } = string.Empty;
    public string FromDoctorId { get; set; } = string.Empty;
    public string ToDoctorId { get; set; } = string.Empty;
    public string FromShiftId { get; set; } = string.Empty;
    public string ToShiftId { get; set; } = string.Empty;
    public string InitiatedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    // NUEVO: Fecha base para calcular shift instances (string "YYYY-MM-DD")
    // En el endpoint, se parsea a DateOnly antes de pasar al command
    public string? BaseDate { get; set; } // "YYYY-MM-DD" formato ISO 8601
}
```

**Semántica de `BaseDate`**:
> `BaseDate` es la **fecha del inicio del FROM shift** (StartAt.Date del shift instance FROM).

**Ejemplos**:
- Day shift (07:00-15:00) con `baseDate=2025-12-01` → arranca `2025-12-01 07:00`
- Night shift (19:00-07:00) con `baseDate=2025-12-01` → arranca `2025-12-01 19:00`, termina `2025-12-02 07:00`
- Para window Night→Day: `baseDate` es la fecha de inicio de la noche

---

### 2. Cambios en Commands

#### `PostAssignmentsCommand`

```csharp
public record PostAssignmentsCommand(
    string UserId, 
    string ShiftId, 
    IEnumerable<string> PatientIds,
    DateOnly AssignmentDate, // NUEVO: DateOnly requerido (endpoint convierte null a Today)
    // ... existing lazy provisioning fields
) : IRequest<Result<IReadOnlyList<string>>>;
```

#### `CreateHandoverCommand`

```csharp
public record CreateHandoverCommand(
    string PatientId,
    string FromDoctorId,
    string ToDoctorId,
    string FromShiftId,
    string ToShiftId,
    string InitiatedBy,
    string? Notes,
    DateOnly BaseDate, // NUEVO: DateOnly requerido (endpoint convierte null a Today)
    // ... existing lazy provisioning fields
) : ICommand<Result<HandoverRecord>>;
```

**Ventaja de `DateOnly`**:
- Elimina ambigüedad de timezone
- No hay confusión con `"2025-12-02T00:00:00Z"` que puede correr un día
- Semántica clara: "qué día es" sin hora

---

### 3. Cambios en Repositorios

#### `IAssignmentRepository`

```csharp
public interface IAssignmentRepository
{
    // BIG-BANG: DateOnly requerido (no DateTime, no nullable)
    // Repositorio = persistencia pura, sin validaciones de negocio
    Task<IReadOnlyList<string>> AssignPatientsAsync(
        string userId, 
        string shiftId, 
        IEnumerable<string> patientIds,
        DateOnly assignmentDate); // NUEVO: DateOnly requerido
}
```

#### `AssignmentRepository.AssignPatientsAsync()`

```csharp
public async Task<IReadOnlyList<string>> AssignPatientsAsync(
    string userId, 
    string shiftId, 
    IEnumerable<string> patientIds,
    DateOnly assignmentDate) // NUEVO: DateOnly requerido
{
    // ... existing code hasta obtener shift template ...
    
    // BIG-BANG: Convertir DateOnly a DateTime para cálculos
    // assignmentDate ya es la fecha base (sin ambigüedad de timezone)
    var startTimeParts = startTime.Split(':');
    var endTimeParts = endTime.Split(':');
    
    // Convertir DateOnly a DateTime usando TimeOnly
    var startAt = assignmentDate.ToDateTime(new TimeOnly(
        int.Parse(startTimeParts[0]), 
        int.Parse(startTimeParts[1])));
    
    var endAt = assignmentDate.ToDateTime(new TimeOnly(
        int.Parse(endTimeParts[0]), 
        int.Parse(endTimeParts[1])));
    
    // Handle overnight shifts (existing logic - ya funciona correctamente)
    if (endAt < startAt)
    {
        endAt = endAt.AddDays(1);
    }
    
    // ... rest of existing code (sin cambios) ...
    
    // NOTA: Validaciones de "pasado/futuro" NO van aquí
    // Van en el Handler usando IClock (ver sección 4)
}
```

#### `IHandoverRepository`

```csharp
public interface IHandoverRepository
{
    // BIG-BANG: DateOnly requerido (no DateTime, no nullable)
    // Repositorio = persistencia pura, sin validaciones de negocio
    Task<HandoverRecord> CreateHandoverAsync(
        CreateHandoverRequest request,
        DateOnly baseDate); // NUEVO: DateOnly requerido
}
```

#### `HandoverRepository.CreateHandoverAsync()`

```csharp
public async Task<HandoverRecord> CreateHandoverAsync(
    CreateHandoverRequest request,
    DateOnly baseDate) // NUEVO: DateOnly requerido
{
    // ... existing code hasta obtener unitId ...
    
    // BIG-BANG: Convertir DateOnly a DateTime para ShiftInstanceCalculationService
    // baseDate ya es la fecha del inicio del FROM shift (semántica clara)
    // NOTA: DateOnly.ToDateTime() devuelve DateTimeKind.Unspecified de por sí
    // Oracle TIMESTAMP no guarda zona, así que Kind=Unspecified es correcto
    var baseDateTime = baseDate.ToDateTime(TimeOnly.MinValue); // 00:00:00, Kind=Unspecified
    
    // ShiftInstanceCalculationService espera DateTime, pero usamos solo la fecha
    var shiftDates = await ShiftInstanceCalculationService.CalculateShiftInstanceDatesFromDbAsync(
        conn, request.FromShiftId, request.ToShiftId, baseDateTime);
    
    // V3: Get or create shift instances (FROM y TO)
    var fromShiftInstanceId = await _shiftInstanceRepository.GetOrCreateShiftInstanceAsync(
        request.FromShiftId, unitId, shiftDates.FromShiftStartAt, shiftDates.FromShiftEndAt);
    
    var toShiftInstanceId = await _shiftInstanceRepository.GetOrCreateShiftInstanceAsync(
        request.ToShiftId, unitId, shiftDates.ToShiftStartAt, shiftDates.ToShiftEndAt);
    
    // V3: Get or create shift window (PASO EXPLÍCITO - crítico para V3 schema)
    // El SHIFT_WINDOW es la entidad explícita que conecta FROM y TO shift instances
    // ⚠️ GetOrCreateShiftWindowAsync valida coherencia temporal internamente
    var shiftWindowId = await _shiftWindowRepository.GetOrCreateShiftWindowAsync(
        fromShiftInstanceId, toShiftInstanceId, unitId);
    
    // ... rest of existing code (crear handover con shiftWindowId) ...
    
    // NOTA: Validaciones de "pasado/futuro" NO van aquí
    // Van en el Handler usando IClock (ver sección 4)
}
```

**⚠️ Punto Crítico - SHIFT_WINDOW Get-or-Create**:
- El schema V3 requiere `SHIFT_WINDOWS` como entidad explícita
- **Debe existir** el shift window (pair FROM→TO) antes de crear handover
- `GetOrCreateShiftWindowAsync` debe estar bien atado (idempotente)
- Si no existe, el handover no puede crearse (falta `SHIFT_WINDOW_ID`)

**⚠️ CRÍTICO - Validación de Coherencia Temporal en SHIFT_WINDOWS**:
- **Validar en `GetOrCreateShiftWindowAsync`** (aunque sea app-enforced):
  - `from.StartAt < to.StartAt` (o al menos `from.ID != to.ID` ya lo tenés)
  - Y, más fuerte: `from.EndAt <= to.StartAt` para la mayoría de transiciones
- Si se cuela una ventana "al revés", todo lo demás se vuelve confuso (especialmente en seeds y en futuros)
- Esta validación previene bugs sutiles donde FROM y TO están invertidos

**Implementación ejemplo de `GetOrCreateShiftWindowAsync`**:

```csharp
public async Task<string> GetOrCreateShiftWindowAsync(
    string fromShiftInstanceId, 
    string toShiftInstanceId, 
    string unitId)
{
    // ⚠️ CRÍTICO: Validar coherencia temporal (app-enforced)
    var fromInstance = await GetShiftInstanceByIdAsync(fromShiftInstanceId);
    var toInstance = await GetShiftInstanceByIdAsync(toShiftInstanceId);
    
    if (fromInstance.Id == toInstance.Id)
    {
        throw new InvalidOperationException("FROM and TO shift instances cannot be the same");
    }
    
    if (fromInstance.StartAt >= toInstance.StartAt)
    {
        throw new InvalidOperationException(
            $"FROM shift must start before TO shift. FROM: {fromInstance.StartAt}, TO: {toInstance.StartAt}");
    }
    
    if (fromInstance.EndAt > toInstance.StartAt)
    {
        throw new InvalidOperationException(
            $"FROM shift must end before or at TO shift start. FROM.End: {fromInstance.EndAt}, TO.Start: {toInstance.StartAt}");
    }
    
    // Get-or-create con patrón insert-then-select
    try
    {
        var newId = await InsertShiftWindowAsync(fromShiftInstanceId, toShiftInstanceId, unitId);
        return newId;
    }
    catch (OracleException ex) when (ex.Number == 1) // ORA-00001: unique constraint violated
    {
        // Ya existe, leer el existente
        var existingId = await GetShiftWindowIdAsync(fromShiftInstanceId, toShiftInstanceId, unitId);
        return existingId;
    }
}
```

**Principio**: Repositorios = persistencia pura, sin validaciones de negocio ni dependencias de `DateTime.Today`.

---

### 4. Cambios en Handlers + IClock para Validaciones

#### `IClock` Interface (Nuevo)

```csharp
/// <summary>
/// Abstraction for getting current date/time.
/// Allows testing with fake dates and future timezone support per unit.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets today's date (no time component) using server timezone.
    /// </summary>
    DateOnly Today();
}
```

**Nota sobre Timezone por Unidad**:
- Cuando se necesite soporte de timezone por unidad, crear un servicio separado:
  - `IUnitTimeProvider.Today(unitId)` o
  - `IClock.Today(ZoneId)` si ya se tienen zonas bien modeladas
- No incluir `unitId` como parámetro opcional ahora para evitar comportamiento mágico

#### Implementación de `IClock`

**Implementación basada en `TimeProvider` (.NET 8+)**:

```csharp
public sealed class SystemClock : IClock
{
    private readonly TimeProvider _timeProvider;
    
    public SystemClock(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }
    
    public DateOnly Today()
    {
        var utcNow = _timeProvider.GetUtcNow();
        var localNow = TimeZoneInfo.ConvertTime(utcNow, _timeProvider.LocalTimeZone);
        return DateOnly.FromDateTime(localNow.DateTime);
    }
}
```

**Nota**: `FakeClock` para tests funciona igual. `TimeProvider` es estándar de .NET 8 y permite testing con `FakeTimeProvider`. La conversión a timezone local evita bugs sutiles cerca de medianoche donde UTC puede estar en un día diferente.

#### `PostAssignmentsHandler`

```csharp
public class PostAssignmentsHandler(
    IAssignmentRepository _repository,
    IClock _clock) // NUEVO: Inyectar IClock
    : IRequestHandler<PostAssignmentsCommand, Result<IReadOnlyList<string>>>
{
    public async Task<Result<IReadOnlyList<string>>> Handle(
        PostAssignmentsCommand request, 
        CancellationToken cancellationToken)
    {
        // ... existing code (lazy provisioning) ...
        
        // BIG-BANG: Validaciones en handler (no en repositorio)
        var today = _clock.Today();
        
        // Validación: No permitir fechas pasadas
        if (request.AssignmentDate < today)
        {
            return Result.Error("Cannot assign patients to past dates");
        }
        
        // Validación opcional: Límite de días futuros
        var maxFutureDays = 30;
        if (request.AssignmentDate > today.AddDays(maxFutureDays))
        {
            return Result.Error($"Cannot assign patients more than {maxFutureDays} days in advance");
        }
        
        // Pasar DateOnly directamente al repositorio (ya validado)
        var assignedPatientIds = await _repository.AssignPatientsAsync(
            request.UserId, 
            request.ShiftId, 
            request.PatientIds,
            request.AssignmentDate); // DateOnly
        
        // ... rest of existing code ...
    }
}
```

**Nota sobre Validación de Handover Completado**:
- La validación de "no asignar si handover completado" se maneja mejor a nivel de base de datos o como warning/soft-block
- Ver sección "Validaciones y Reglas de Negocio" para alternativas más robustas

#### `CreateHandoverHandler`

```csharp
public class CreateHandoverHandler(
    IHandoverRepository _repository,
    IUserRepository _userRepository,
    IClock _clock) // NUEVO: Inyectar IClock
    : ICommandHandler<CreateHandoverCommand, Result<HandoverRecord>>
{
    public async Task<Result<HandoverRecord>> Handle(
        CreateHandoverCommand request, 
        CancellationToken cancellationToken)
    {
        // ... existing code (lazy provisioning) ...
        
        // BIG-BANG: Validaciones en handler (no en repositorio)
        var today = _clock.Today();
        
        // Validación: No permitir fechas pasadas
        if (request.BaseDate < today)
        {
            return Result.Error("Cannot create handover for past dates");
        }
        
        // Pasar DateOnly directamente al repositorio (ya validado)
        var handover = await _repository.CreateHandoverAsync(
            createRequest,
            request.BaseDate); // DateOnly
        
        // ... rest of existing code ...
    }
}
```

#### `PatientAssignedToShiftHandler` (Auto-handover creation)

**BIG-BANG**: Política clara de auto-handover con `IClock`

**Decisión**: **Opción B** (recomendada) - Auto-handover si `shiftDate <= today + 1` (configurable).

```csharp
public class PatientAssignedToShiftHandler(
    IHandoverRepository _handoverRepository,
    IShiftTransitionService _shiftTransitionService,
    IShiftInstanceRepository _shiftInstanceRepository,
    IClock _clock, // NUEVO: Inyectar IClock
    IOptions<SchedulingOptions> _options, // NUEVO: Config real (no const)
    ILogger<PatientAssignedToShiftHandler> _logger) 
    : INotificationHandler<PatientAssignedToShiftEvent>
{
    // Configurable: máximo días futuros para auto-handover (desde config)
    private int MaxAutoHandoverDays => _options.Value.MaxAutoHandoverDays;
    
    public async Task Handle(PatientAssignedToShiftEvent domainEvent, CancellationToken cancellationToken)
    {
        // ... existing code (check IsPrimary) ...
        
        if (!domainEvent.IsPrimary)
        {
            return;
        }
        
        // Get next shift ID
        var nextShiftId = await _shiftTransitionService.GetNextShiftIdAsync(domainEvent.ShiftId);
        if (string.IsNullOrEmpty(nextShiftId))
        {
            _logger.LogWarning("Next shift not found for {ShiftId}", domainEvent.ShiftId);
            return;
        }
        
        // BIG-BANG: Obtener fecha del shift instance (semántica clara)
        var shiftInstance = await _shiftInstanceRepository.GetShiftInstanceByIdAsync(domainEvent.ShiftInstanceId);
        if (shiftInstance == null)
        {
            _logger.LogWarning("Shift instance not found: {ShiftInstanceId}", domainEvent.ShiftInstanceId);
            return;
        }
        
        // BIG-BANG: Usar DateOnly para fecha base (sin ambigüedad)
        var shiftDate = DateOnly.FromDateTime(shiftInstance.StartAt);
        var today = _clock.Today();
        var maxDate = today.AddDays(MaxAutoHandoverDays);
        
        // BIG-BANG: Política clara - solo crear si está dentro del rango permitido
        if (shiftDate > maxDate)
        {
            _logger.LogDebug(
                "Skipping auto-handover: shift date {ShiftDate} is more than {MaxDays} days in the future. PatientId={PatientId}",
                shiftDate, MaxAutoHandoverDays, domainEvent.PatientId);
            return;
        }
        
        // BIG-BANG: Usar fecha del shift instance (semántica: fecha del inicio del FROM shift)
        var createRequest = new CreateHandoverRequest(
            domainEvent.PatientId,
            domainEvent.UserId,
            null,
            domainEvent.ShiftId,
            nextShiftId,
            domainEvent.UserId,
            $"Auto-created handover when patient assigned to {domainEvent.ShiftId} shift"
        );

        try
        {
            // BIG-BANG: Pasar DateOnly (no DateTime)
            var handover = await _handoverRepository.CreateHandoverAsync(createRequest, shiftDate);
            _logger.LogInformation(
                "Auto-created handover. HandoverId={HandoverId}, PatientId={PatientId}, ShiftDate={ShiftDate}",
                handover.Id, domainEvent.PatientId, shiftDate);
        }
        catch (InvalidOperationException ex)
        {
            // ... existing error handling ...
        }
    }
}
```

**Política de Auto-Handover (Final)**:
- ✅ **Crear auto-handover si** `shiftDate <= today + MaxAutoHandoverDays`
- ✅ **Default**: `MaxAutoHandoverDays = 1` (hoy y mañana)
- ✅ **Configurable**: vía `appsettings.json` usando `IOptions<SchedulingOptions>` (no `const`)

**Configuración**:

```csharp
// SchedulingOptions.cs (unificar config de scheduling)
public class SchedulingOptions
{
    public int MaxAssignmentFutureDays { get; set; } = 30; // Default: 30 días
    public int MaxAutoHandoverDays { get; set; } = 1; // Default: hoy y mañana
}

// En startup
services.Configure<SchedulingOptions>(configuration.GetSection("Scheduling"));

// appsettings.json
{
  "Scheduling": {
    "MaxAssignmentFutureDays": 30,
    "MaxAutoHandoverDays": 1
  }
}
```
- ✅ **Condiciones adicionales**:
  - Si no existe próximo shift → no crear
  - Si no hay coverage esperado para TO → dejar en Draft (ya implementado)

**Ventajas**:
- ✅ Soporta planificación "mañana" (muy real en hospitales)
- ✅ Evita handovers prematuros (>1 día)
- ✅ Código explícito con `IClock` (testeable)
- ✅ Semántica clara: `shiftDate` = fecha del inicio del FROM shift

---

### 5. Cambios en Endpoints

#### `PostAssignments` Endpoint

```csharp
public class PostAssignments(IMediator _mediator, ICurrentUser _currentUser, IClock _clock)
    : Endpoint<PostAssignmentsRequest>
{
    public override void Configure()
    {
        Post("/me/assignments");
    }

    public override async Task HandleAsync(PostAssignmentsRequest req, CancellationToken ct)
    {
        var userId = _currentUser.Id;
        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        // BIG-BANG: Parsear string a DateOnly usando helper centralizado
        // Si es null/empty, usar Today desde IClock
        DateOnly assignmentDate;
        if (string.IsNullOrWhiteSpace(req.AssignmentDate))
        {
            assignmentDate = _clock.Today();
        }
        else if (!DateParsingHelper.TryParseIsoDateOnly(req.AssignmentDate, out assignmentDate))
        {
            AddError("Invalid date format. Expected YYYY-MM-DD");
            await SendErrorsAsync(statusCode: 400, ct);
            return;
        }
        
        // Validación temprana en endpoint (mejor UX)
        // Nota: Validación completa está en Handler, esto es solo para UX rápida
        var today = _clock.Today();
        if (assignmentDate < today)
        {
            AddError("Cannot assign patients to past dates");
            await SendErrorsAsync(statusCode: 400, ct);
            return;
        }

        try
        {
            var result = await _mediator.Send(
                new PostAssignmentsCommand(
                    userId, 
                    req.ShiftId, 
                    req.PatientIds ?? [],
                    assignmentDate, // DateOnly (no nullable)
                    _currentUser.Email,
                    _currentUser.FirstName,
                    _currentUser.LastName,
                    _currentUser.FullName,
                    _currentUser.AvatarUrl,
                    _currentUser.OrgRole
                ),
                ct);

            if (result.IsSuccess)
            {
                await SendNoContentAsync(ct);
            }
            else
            {
                // No esconder errores del handler - devolver mensaje real
                var errorMessage = result.Errors.FirstOrDefault() ?? "Failed to assign patients";
                AddError(errorMessage);
                await SendErrorsAsync(statusCode: 400, ct);
            }
        }
        catch (Exception)
        {
            AddError("Assignment failed: referenced shift or patient does not exist");
            await SendErrorsAsync(statusCode: 400, ct);
        }
    }
}
```

#### `CreateHandover` Endpoint

```csharp
public class CreateHandover(IMediator _mediator, ICurrentUser _currentUser, IClock _clock)
    : Endpoint<CreateHandoverRequestDto, CreateHandoverResponse>
{
    public override void Configure()
    {
        Post("/handovers");
    }

    public override async Task HandleAsync(CreateHandoverRequestDto req, CancellationToken ct)
    {
        var userId = _currentUser.Id;
        if (string.IsNullOrEmpty(userId)) 
        { 
            await SendUnauthorizedAsync(ct); 
            return; 
        }
        
        // BIG-BANG: Parsear string a DateOnly usando helper centralizado
        // Si es null/empty, usar Today desde IClock
        DateOnly baseDate;
        if (string.IsNullOrWhiteSpace(req.BaseDate))
        {
            baseDate = _clock.Today();
        }
        else if (!DateParsingHelper.TryParseIsoDateOnly(req.BaseDate, out baseDate))
        {
            AddError("Invalid date format. Expected YYYY-MM-DD");
            await SendErrorsAsync(statusCode: 400, ct);
            return;
        }
        
        // Validación temprana en endpoint (mejor UX)
        // Nota: Validación completa está en Handler, esto es solo para UX rápida
        var today = _clock.Today();
        if (baseDate < today)
        {
            AddError("Cannot create handover for past dates");
            await SendErrorsAsync(statusCode: 400, ct);
            return;
        }
        
        var command = new CreateHandoverCommand(
            req.PatientId,
            req.FromDoctorId,
            req.ToDoctorId,
            req.FromShiftId,
            req.ToShiftId,
            req.InitiatedBy,
            req.Notes,
            baseDate, // DateOnly (no nullable)
            _currentUser.Email,
            _currentUser.FirstName,
            _currentUser.LastName,
            _currentUser.FullName,
            _currentUser.AvatarUrl,
            _currentUser.OrgRole
        );

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var handover = result.Value;
            Response = new CreateHandoverResponse
            {
                Id = handover.Id,
                PatientId = handover.PatientId,
                PatientName = handover.PatientName,
                Status = handover.Status,
                CreatedAt = handover.CreatedAt,
                ShiftWindowId = handover.ShiftWindowId
            };
            await SendAsync(Response, cancellation: ct);
        }
        else
        {
            // No esconder errores del handler - devolver mensaje real
            var errorMessage = result.Errors.FirstOrDefault() ?? "Failed to create handover";
            AddError(errorMessage);
            await SendErrorsAsync(statusCode: 400, ct);
        }
    }
}
```

**Estrategia de Serialización JSON (MVP Robusto)**:
- DTOs usan **string** para fechas (cero magia del serializer)
- Contrato HTTP claro: `"YYYY-MM-DD"` (ISO 8601)
- Parseo explícito en endpoint con `DateOnly.ParseExact(req.AssignmentDate, "yyyy-MM-dd")`
- Si el string es null/empty → usar `_clock.Today()` como default
- Si el formato es inválido → error 400 con mensaje claro

**⚠️ Centralizar Parseo de Fechas (Evitar Copy/Paste)**:
- Crear helper simple sin acoplamiento al endpoint:
```csharp
public static class DateParsingHelper
{
    /// <summary>
    /// Intenta parsear string ISO 8601 a DateOnly.
    /// Retorna true si el parseo fue exitoso, false si es null/empty o formato inválido.
    /// </summary>
    public static bool TryParseIsoDateOnly(string? s, out DateOnly value)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            value = default;
            return false;
        }
        
        return DateOnly.TryParseExact(
            s, 
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out value);
    }
}
```
- Uso en endpoints:
```csharp
DateOnly assignmentDate;
if (string.IsNullOrWhiteSpace(req.AssignmentDate))
{
    assignmentDate = _clock.Today();
}
else if (!DateParsingHelper.TryParseIsoDateOnly(req.AssignmentDate, out assignmentDate))
{
    AddError("Invalid date format. Expected YYYY-MM-DD");
    await SendErrorsAsync(statusCode: 400, ct);
    return;
}
```
- Ventajas: mensajes consistentes, estilos uniformes, cero acoplamiento al framework, cero sorpresas

**Ventajas**:
- No requiere configuración especial de System.Text.Json
- Contrato HTTP explícito y predecible
- Manejo de errores claro (formato inválido)
- `DateOnly` se mantiene solo en capa interna (commands/handlers/repos)

---

## Validaciones y Reglas de Negocio

### 1. Validación de Fechas Pasadas

**Regla**: No permitir asignar pacientes o crear handovers para fechas pasadas.

```csharp
var today = _clock.Today();
if (baseDate < today)
{
    return Result.Error("Cannot assign patients to past dates");
}
```

**Justificación**: 
- Asignaciones pasadas no tienen sentido
- Handovers pasados deberían ser históricos, no creados manualmente

### 2. Límite de Días Futuros (Configurable)

**Regla**: Limitar cuántos días en el futuro se pueden asignar pacientes.

**Configuración**: Usar `SchedulingOptions` (no hardcoded):

```csharp
// SchedulingOptions.cs
public class SchedulingOptions
{
    public int MaxAssignmentFutureDays { get; set; } = 30; // Default: 30 días
    public int MaxAutoHandoverDays { get; set; } = 1; // Default: hoy y mañana
}

// En handler
public class PostAssignmentsHandler(
    IAssignmentRepository _repository,
    IClock _clock,
    IOptions<SchedulingOptions> _options) // NUEVO: Config real
{
    public async Task<Result<IReadOnlyList<string>>> Handle(...)
    {
        var today = _clock.Today();
        var maxFutureDays = _options.Value.MaxAssignmentFutureDays;
        if (request.AssignmentDate > today.AddDays(maxFutureDays))
        {
            return Result.Error($"Cannot assign patients more than {maxFutureDays} days in advance");
        }
        // ...
    }
}
```

**Justificación**:
- Evita asignaciones demasiado anticipadas
- Reduce complejidad de planificación
- Configurable por organización vía `appsettings.json`
- Evita números mágicos en código

### 3. Validación de Shift Instances Existentes

**Regla**: Si el shift instance ya existe para esa fecha, reutilizarlo (ya implementado).

**Comportamiento actual**: `GetOrCreateShiftInstanceAsync` ya maneja esto correctamente.

**⚠️ CRÍTICO - Idempotencia y Concurrencia en GetOrCreate**:
- **Deben tener unique constraints** en DB (o equivalente) para prevenir duplicados:
  - `UQ_SI_SHIFT_START` para `SHIFT_INSTANCES` (por ejemplo: `(SHIFT_ID, UNIT_ID, START_AT)`)
  - `UQ_SW_PAIR` para `SHIFT_WINDOWS` (por ejemplo: `(FROM_SHIFT_INSTANCE_ID, TO_SHIFT_INSTANCE_ID, UNIT_ID)`)
- **Patrón insert-then-select en Oracle** (implementación real):
  1. Intentar `INSERT` (apoyado en unique constraints)
  2. Si falla por `ORA-00001` (unique violation) → `SELECT` del existente y devolverlo
  3. Si el INSERT tiene éxito → devolver el ID insertado
- **Implementación ejemplo**:
```csharp
public async Task<string> GetOrCreateShiftInstanceAsync(...)
{
    try
    {
        // Intentar INSERT
        var newId = await InsertShiftInstanceAsync(...);
        return newId;
    }
    catch (OracleException ex) when (ex.Number == 1) // ORA-00001: unique constraint violated
    {
        // Ya existe, leer el existente
        var existingId = await GetShiftInstanceIdAsync(...);
        return existingId;
    }
}
```
- Esto aplica tanto para `GetOrCreateShiftInstanceAsync` como para `GetOrCreateShiftWindowAsync`
- Es el bug más común en "get-or-create" patterns si no se maneja correctamente
- Sin este patrón, bajo carga vas a terminar con excepciones intermitentes o duplicación si te falta una unique constraint

### 4. Validación de Coverage para Fechas Futuras

**Regla**: Para crear handover, debe existir coverage en el FROM shift instance.

**Comportamiento actual**: Ya validado en `CreateHandoverAsync` (Regla #10 de V3_PLAN.md).

**Consideración**: Si se crea handover para fecha futura, el coverage debe existir para esa fecha futura.

### 5. Validación de Handover Completado (Decisión Final)

**Contexto**: Un handover completado significa que el turno FROM ya terminó. Los handovers duran lo que dura un turno porque es el handover de ese turno al otro.

**Decisión Final: Opción B - Soft-Block / Warning (Implementada)**

**No bloquear** en backend. Dejar que asignen tarde si quieren (pasa en la vida real en hospitales).

- Si el handover ya fue Completed, eso se trata como warning/auditoría (logs, evento, métricas), no error 400
- Si de verdad se quiere bloquear, hacerlo **en la UI** ("este turno ya fue entregado, ¿seguro?")

**Ventajas**:
- ✅ Más flexible para casos reales (asignaciones tardías pasan)
- ✅ No complica la lógica del handler
- ✅ Permite asignaciones tardías cuando es necesario
- ✅ Encaja perfecto con realidad hospitalaria

**Alternativas Descartadas**:

#### Opción A: Constraint en Base de Datos (❌ Descartada)

**Razón de descarte**: Agregar `UQ_SC_PAT_SI` implica que **un paciente solo puede tener 1 cobertura total por turno**, lo que **mata el caso "secondary coverage / co-management"** (bastante real en hospitales).

**Ya existen constraints que previenen duplicados**:
- `UQ_SC` por `(RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID)` → evita duplicado del mismo médico
- `UQ_SC_PRIMARY_ACTIVE` parcial → garantiza **1 solo primary**

#### Opción C: Validación Post-Asignación (❌ Descartada - Futuro si se requiere)

Si en el futuro se requiere bloqueo duro en backend, implementar validación post-asignación con transacción. Por ahora, no se implementa.

---

## Casos de Uso

### Caso 1: Asignar Pacientes para Mañana

```http
POST /me/assignments
{
  "shiftId": "shift-day",
  "patientIds": ["pat-123", "pat-456"],
  "assignmentDate": "2024-12-20" // Mañana
}
```

**Flujo**:
1. Usuario envía request con `assignmentDate = tomorrow`
2. `PostAssignmentsHandler` pasa fecha al repositorio
3. `AssignmentRepository` crea shift instance para mañana
4. Crea coverage para mañana
5. Si es primary, dispara evento `PatientAssignedToShiftEvent`
6. **Auto-handover**: Se crea si `shiftDate <= today + MaxAutoHandoverDays` (hoy o mañana por defecto)

### Caso 2: Crear Handover Manual para Fecha Futura

```http
POST /handovers
{
  "patientId": "pat-123",
  "fromShiftId": "shift-day",
  "toShiftId": "shift-night",
  "baseDate": "2024-12-20" // Mañana
}
```

**Flujo**:
1. Usuario envía request con `baseDate = tomorrow`
2. `CreateHandoverHandler` pasa fecha al repositorio
3. `HandoverRepository` calcula shift instances para mañana
4. Verifica que existe coverage en FROM shift instance para mañana
5. Crea handover en estado `Draft` para mañana

### Caso 3: Sin Fecha (Default a Today)

```http
POST /me/assignments
{
  "shiftId": "shift-day",
  "patientIds": ["pat-123"]
  // assignmentDate no se envía (null)
}
```

**Flujo**:
1. `assignmentDate` es `null` en request
2. Endpoint normaliza a `_clock.Today()` (DateOnly)
3. Command recibe `DateOnly` (no nullable)
4. Repositorio usa fecha directamente (sin lógica condicional)
5. **Comportamiento**: Igual que antes, pero código más limpio

---

## Consideraciones Adicionales

### 1. Timezone Handling - ⚠️ CRÍTICO: Elegir UNA Fuente de Verdad

**Problema**: Si mezclás `TRUNC(SYSDATE)` (timezone = **DB**) con `_clock.Today()` (timezone = **APP**), te van a aparecer casos raros tipo "asigné hoy pero no me sale en el listado" alrededor de medianoche/DST.

**⚠️ DECISIÓN CRÍTICA - Fuente de Verdad para "Hoy"**:
- **NO mezclar** `TRUNC(SYSDATE)` en lecturas con `_clock.Today()` en escrituras/validaciones
- **Recomendación (MVP prolijo)**: Hacer que **todo** use el reloj de la app y pasar parámetros:
  - Para ">= hoy": `si.START_AT >= :todayStart` donde `todayStart = _clock.Today().ToDateTime(TimeOnly.MinValue)`
  - Para "día específico": `:dayStart <= START_AT < :dayEnd`
- **Evitar `SYSDATE`/`TRUNC` en queries** → Te queda testeable y consistente

**⚠️ IMPORTANTE - Oracle TIMESTAMP y DateTimeKind**:
- Oracle `TIMESTAMP` no guarda zona horaria
- `DateOnly.ToDateTime()` devuelve `DateTimeKind.Unspecified` de por sí (correcto)
- Todos los `DateTime` que persistimos son **hora local del servidor** y `DateTimeKind.Unspecified`
- Si la app corre en servidores con TZ distinto, podés tener "hoy" corrido
- **Documentar explícitamente**: "Todos los DateTime son hora local del servidor, Kind=Unspecified"
- No es para "arreglarlo ahora", es para que no se convierta en bug fantasma después

**Mejora futura**: 
- Crear `IUnitTimeProvider.Today(unitId)` para timezone por unidad
- Aceptar timezone en request
- Convertir a UTC antes de guardar
- Convertir a timezone local al mostrar

### 2. Auto-Handover Creation para Fechas Futuras

**Política Final**: Auto-handover se crea si `shiftDate <= today + MaxAutoHandoverDays`.

- **Default**: `MaxAutoHandoverDays = 1` (hoy y mañana)
- **Configurable**: vía `appsettings.json`
- **Implementado**: Ya se pasa fecha del shift instance a `CreateHandoverAsync`

### 3. Queries y Filtros - Date Scope Explícito

**Problema**: Las queries existentes filtran por `>= TRUNC(SYSDATE)`, pero falta control de UX.

**Recomendación**: Agregar parámetro opcional `date` en endpoints de lectura.

#### `GetMyPatients` Endpoint (Ejemplo)

```csharp
public class GetMyPatientsRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    
    // NUEVO: Fecha opcional para filtrar pacientes
    // Si es null, muestra pacientes de hoy y futuros (comportamiento actual)
    // Si viene, muestra pacientes de esa fecha específica
    public DateOnly? Date { get; set; }
}
```

#### `GetMyPatientsAsync` Repository

```csharp
// ⚠️ REPO PURO: Recibe todayStart como parámetro (calculado en handler/service)
public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetMyPatientsAsync(
    string userId, 
    int page, 
    int pageSize,
    DateOnly? date = null, // NUEVO: Fecha opcional
    DateTime todayStart) // NUEVO: "Hoy" calculado desde IClock en handler
{
    using var conn = _connectionFactory.CreateConnection();

    // ⚠️ PERFORMANCE: Usar rango en vez de TRUNC para permitir uso de índices
    // TRUNC(si.START_AT) rompe uso de índices sobre START_AT
    string dateFilter;
    object parameters;
    
    if (date.HasValue)
    {
        // Filtrar por rango: dayStart <= START_AT < dayEnd
        var dayStart = date.Value.ToDateTime(TimeOnly.MinValue); // 00:00:00
        var dayEnd = dayStart.AddDays(1); // 00:00:00 del día siguiente
        dateFilter = "AND si.START_AT >= :dayStart AND si.START_AT < :dayEnd";
        parameters = new { userId, dayStart, dayEnd, offset, maxRow = offset + ps };
    }
    else
    {
        // Filtrar por >= hoy (comportamiento actual)
        // ⚠️ todayStart viene del handler (calculado desde IClock)
        // NO usar TRUNC(SYSDATE) para evitar mezclar timezones
        dateFilter = "AND si.START_AT >= :todayStart";
        parameters = new { userId, todayStart, offset, maxRow = offset + ps };
    }

    // ... rest of query con dateFilter y parameters ...
}
```

**En el Handler/Service**:
```csharp
// Handler calcula todayStart desde IClock (fuente de verdad única)
var todayStart = _clock.Today().ToDateTime(TimeOnly.MinValue);
var (patients, total) = await _repository.GetMyPatientsAsync(
    userId, page, pageSize, date, todayStart);
```

**Ventajas del rango**:
- ✅ Permite uso de índices sobre `START_AT` (mejor performance)
- ✅ Evita `TRUNC` que rompe índices
- ✅ Semántica clara: "turnos que empiezan en este día"

**Nota sobre "hoy" en default**:
- **Decisión final**: Usar **app timezone** (`IClock`) como fuente de verdad única
- **NO usar `TRUNC(SYSDATE)`** en queries para evitar mezclar timezones
- El handler/service debe pasar `todayStart` desde `IClock` como parámetro
- Esto garantiza consistencia: escrituras y lecturas usan la misma fuente de verdad

**Casos de Uso**:
- `GET /me/patients` → pacientes de hoy y futuros (default)
- `GET /me/patients?date=2025-12-20` → pacientes de esa fecha específica
- `GET /me/patients?date=2025-12-20&page=1&pageSize=25` → paginado de esa fecha

**Ventajas**:
- ✅ Control explícito de qué fecha mostrar
- ✅ Evita confusión de "mis pacientes de hoy" vs "mis pacientes de mañana"
- ✅ Preparado para UI que muestre tabs por fecha

### 4. Testing

**Tests necesarios**:

#### Tests Básicos
1. ✅ Asignar pacientes para fecha futura
2. ✅ Crear handover para fecha futura
3. ✅ Validar que no se puede asignar para fecha pasada
4. ✅ Validar default (sin fecha = hoy)
5. ✅ Night shift crossing midnight con fecha futura
6. ✅ Auto-handover se crea para hoy y mañana (Opción B)

#### Tests Críticos (Agregar)

**1. Night Shift BaseDate Test**:
```csharp
[Fact]
public async Task AssignPatients_NightShift_BaseDate_CorrectEndDate()
{
    // Arrange: Night shift 19:00-07:00 con baseDate=2025-12-01
    var baseDate = new DateOnly(2025, 12, 1);
    var shiftId = "shift-night"; // 19:00-07:00
    
    // Act
    var result = await _repository.AssignPatientsAsync(userId, shiftId, patientIds, baseDate);
    
    // Assert: endAt debe ser 2025-12-02 07:00 (día siguiente)
    var shiftInstance = await _shiftInstanceRepository.GetShiftInstanceByIdAsync(...);
    Assert.Equal(new DateTime(2025, 12, 1, 19, 0, 0), shiftInstance.StartAt);
    Assert.Equal(new DateTime(2025, 12, 2, 7, 0, 0), shiftInstance.EndAt);
}
```

**2. Window Coherencia Test**:
```csharp
[Fact]
public async Task CreateHandover_WindowCoherence_FROM_End_Before_TO_Start()
{
    // Arrange: FROM shift (Day 07-15), TO shift (Night 19-07)
    var baseDate = new DateOnly(2025, 12, 1);
    
    // Act
    var handover = await _repository.CreateHandoverAsync(request, baseDate);
    
    // Assert: FROM.endAt <= TO.startAt
    var fromInstance = await _shiftInstanceRepository.GetShiftInstanceByIdAsync(fromInstanceId);
    var toInstance = await _shiftInstanceRepository.GetShiftInstanceByIdAsync(toInstanceId);
    Assert.True(fromInstance.EndAt <= toInstance.StartAt, 
        "FROM shift must end before or at TO shift start");
}

[Fact]
public async Task GetOrCreateShiftWindow_ValidatesTemporalCoherence()
{
    // Arrange: FROM shift instance y TO shift instance
    var fromInstance = await CreateShiftInstanceAsync(..., startAt: new DateTime(2025, 12, 1, 7, 0, 0), endAt: new DateTime(2025, 12, 1, 15, 0, 0));
    var toInstance = await CreateShiftInstanceAsync(..., startAt: new DateTime(2025, 12, 1, 19, 0, 0), endAt: new DateTime(2025, 12, 2, 7, 0, 0));
    
    // Act
    var windowId = await _shiftWindowRepository.GetOrCreateShiftWindowAsync(fromInstance.Id, toInstance.Id, unitId);
    
    // Assert: Window creado correctamente (validación interna debe pasar)
    Assert.NotNull(windowId);
    
    // Assert: Intentar crear ventana invertida debe fallar
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
    {
        await _shiftWindowRepository.GetOrCreateShiftWindowAsync(toInstance.Id, fromInstance.Id, unitId);
    });
}
```

**3. Timezone Paranoia Test**:
```csharp
[Fact]
public async Task AssignPatients_WithFakeClock_DoesNotDependOnServerTime()
{
    // Arrange: Fake clock con fecha controlada
    var fakeClock = new FakeClock(new DateOnly(2025, 12, 15));
    var handler = new PostAssignmentsHandler(_repository, fakeClock);
    
    // Act: Intentar asignar para "ayer" según fake clock
    var yesterday = fakeClock.Today().AddDays(-1);
    var result = await handler.Handle(new PostAssignmentsCommand(..., yesterday), ct);
    
    // Assert: Debe fallar (no puede asignar para pasado)
    Assert.False(result.IsSuccess);
    Assert.Contains("past dates", result.Errors.First());
}
```

**Implementación de `FakeClock` para tests**:
```csharp
public class FakeClock : IClock
{
    private readonly DateOnly _fixedDate;
    
    public FakeClock(DateOnly fixedDate)
    {
        _fixedDate = fixedDate;
    }
    
    public DateOnly Today() => _fixedDate;
}
```

---

## Resumen de Cambios (Big-Bang Refactor)

### Archivos a Modificar

1. **Nuevos Archivos** (3 archivos):
   - `IClock` interface → abstracción para obtener "hoy" (DateOnly)
   - `SystemClock` implementation → implementación basada en `TimeProvider` (.NET 8+)
   - `DateParsingHelper` → helper para centralizar parseo de fechas ISO 8601 (`TryParseIsoDateOnly`)

2. **DTOs/Requests** (2 archivos):
- `PostAssignmentsRequest` → agregar `AssignmentDate?` (string "YYYY-MM-DD", opcional en API)
- `CreateHandoverRequestDto` → agregar `BaseDate?` (string "YYYY-MM-DD", opcional en API)
- `SchedulingOptions` → clase de configuración para `MaxAssignmentFutureDays` y `MaxAutoHandoverDays`

3. **Commands** (2 archivos):
   - `PostAssignmentsCommand` → cambiar a `DateOnly AssignmentDate` (requerido, no nullable)
   - `CreateHandoverCommand` → cambiar a `DateOnly BaseDate` (requerido, no nullable)

4. **Repositorios** (4 archivos):
   - `IAssignmentRepository` → cambiar firma a `DateOnly assignmentDate` (requerido)
   - `AssignmentRepository.AssignPatientsAsync()` → **ELIMINAR** `var today = DateTime.Today`, convertir `DateOnly` a `DateTime` para cálculos
   - `IHandoverRepository` → cambiar firma a `DateOnly baseDate` (requerido)
   - `HandoverRepository.CreateHandoverAsync()` → **ELIMINAR** `var today = DateTime.Today`, convertir `DateOnly` a `DateTime` para cálculos, **asegurar** `GetOrCreateShiftWindowAsync` explícito
   - **ELIMINAR validaciones** de "pasado/futuro" de repositorios
   - **NO agregar** constraint `UQ_SC_PAT_SI` (mata secondary coverage - ver sección 5.1)
   - **`GetOrCreateShiftInstanceAsync`** → implementar patrón insert-then-select con manejo de `ORA-00001`
   - **`GetOrCreateShiftWindowAsync`** → implementar patrón insert-then-select con manejo de `ORA-00001` + **validación de coherencia temporal** (`from.StartAt < to.StartAt` y `from.EndAt <= to.StartAt`)
   - **Asegurar unique constraints** en DB: `UQ_SI_SHIFT_START` para `SHIFT_INSTANCES` y `UQ_SW_PAIR` para `SHIFT_WINDOWS`

5. **Handlers** (3 archivos):
   - `PostAssignmentsHandler` → inyectar `IClock` y `IOptions<SchedulingOptions>`, agregar validaciones, pasar `DateOnly` al repositorio
   - `CreateHandoverHandler` → inyectar `IClock`, agregar validaciones, pasar `DateOnly` al repositorio
   - `PatientAssignedToShiftHandler` → inyectar `IClock` y `IOptions<SchedulingOptions>`, obtener fecha del shift instance, aplicar política de auto-handover

6. **Endpoints** (2 archivos):
   - `PostAssignments` → inyectar `IClock`, usar `DateParsingHelper.TryParseIsoDateOnly()` para parsear, normalizar, validar, pasar a command
   - `CreateHandover` → inyectar `IClock`, usar `DateParsingHelper.TryParseIsoDateOnly()` para parsear, normalizar, validar, pasar a command
7. **Configuración** (1 archivo):
   - `SchedulingOptions` → clase de configuración unificada para `MaxAssignmentFutureDays` y `MaxAutoHandoverDays`
   - Registrar `IOptions<SchedulingOptions>` en DI

7. **Queries (Opcional, Futuro)**:
   - `GetMyPatients` → agregar parámetro `Date?` para date scope explícito
   - **ELIMINAR `TRUNC(SYSDATE)` de todas las queries** → usar `IClock` y pasar `todayStart` como parámetro

**Total**: ~13 archivos modificados + 3 nuevos.

### Cambios Clave del Big-Bang

✅ **`DateOnly` en lugar de `DateTime`** para fechas base (elimina ambigüedad de timezone)
✅ **`IClock` para obtener "hoy"** (testeable, preparado para timezone por unidad)
✅ **Fuente de verdad única**: `IClock` (app timezone) en todo, NO mezclar con `TRUNC(SYSDATE)`
✅ **Validaciones en handlers**, no en repositorios (repositorios = persistencia pura)
✅ **Semántica clara**: `baseDate` = fecha del inicio del FROM shift
✅ **Auto-handover con política clara**: hoy + mañana (configurable)
✅ **Date scope explícito** en queries (preparado para UI)
✅ **Get-or-create idempotente real**: Patrón insert-then-select con manejo de `ORA-00001`
✅ **Validación de coherencia temporal**: `from.StartAt < to.StartAt` y `from.EndAt <= to.StartAt` en `SHIFT_WINDOWS`
✅ **Centralización de parseo**: Helper `ParseIsoDateOnlyOr400()` para evitar divergencias
✅ **Tests críticos** para night shifts, coherencia de windows, timezone, get-or-create concurrente

---

## Plan de Implementación

### Fase 1: Backend (Repositorios - Persistencia Pura)
1. Modificar `IAssignmentRepository`: cambiar firma a `DateOnly assignmentDate` (requerido)
2. Modificar `AssignmentRepository.AssignPatientsAsync()`: 
   - **ELIMINAR** `var today = DateTime.Today`
   - Convertir `DateOnly` a `DateTime` para cálculos
3. Modificar `IHandoverRepository`: cambiar firma a `DateOnly baseDate` (requerido)
4. Modificar `HandoverRepository.CreateHandoverAsync()`:
   - **ELIMINAR** `var today = DateTime.Today`
   - Convertir `DateOnly` a `DateTime` para cálculos
   - **Asegurar** que `GetOrCreateShiftWindowAsync` está explícito y bien atado
5. **NO agregar validaciones** de negocio en repositorios (van en handlers)

### Fase 2: Commands y Handlers (Validaciones)
1. Modificar `PostAssignmentsCommand`: `DateOnly AssignmentDate` (requerido)
2. Modificar `PostAssignmentsHandler`:
   - Inyectar `IClock`
   - **Agregar validaciones** (fechas pasadas, límite futuro)
   - Pasar `DateOnly` al repositorio
3. Modificar `CreateHandoverCommand`: `DateOnly BaseDate` (requerido)
4. Modificar `CreateHandoverHandler`:
   - Inyectar `IClock`
   - **Agregar validaciones** (fechas pasadas)
   - Pasar `DateOnly` al repositorio
5. Modificar `PatientAssignedToShiftHandler`:
   - Inyectar `IClock` y `IOptions<SchedulingOptions>`
   - Obtener fecha del shift instance
   - Aplicar política de auto-handover (configurable)

### Fase 3: API Layer (DTOs y Endpoints)
1. Modificar `PostAssignmentsRequest`: `string? AssignmentDate` (formato "YYYY-MM-DD")
2. Modificar `PostAssignments` endpoint:
   - Inyectar `IClock`
   - Parsear string a `DateOnly` con `DateOnly.ParseExact`
   - Normalizar fecha (null → Today)
   - Validación temprana (mejor UX)
3. Modificar `CreateHandoverRequestDto`: `string? BaseDate` (formato "YYYY-MM-DD")
4. Modificar `CreateHandover` endpoint:
   - Inyectar `IClock`
   - Parsear string a `DateOnly` con `DateOnly.ParseExact`
   - Normalizar fecha (null → Today)
   - Validación temprana (mejor UX)
5. Crear `SchedulingOptions` y registrar en DI (`IOptions<SchedulingOptions>`)

### Fase 4: Testing
1. Tests unitarios para validaciones
2. Tests de integración para fechas futuras
3. Tests de backward compatibility

---

## Conclusión (Big-Bang Refactor con Mejoras de Diseño)

La implementación es **factible y más robusta** con las mejoras de diseño porque:

✅ La infraestructura base ya existe (`ShiftInstanceCalculationService` acepta `baseDate`)
✅ **`DateOnly` elimina bugs de timezone**: Sin ambigüedad de "qué día es"
✅ **`IClock` hace código testeable**: No depende de `DateTime.Today` hardcodeado
✅ **Repositorios puros**: Sin validaciones de negocio, solo persistencia
✅ **Semántica clara**: `baseDate` = fecha del inicio del FROM shift (documentado)
✅ **Auto-handover con política clara**: Hoy + mañana, configurable
✅ **Date scope explícito**: Preparado para UI con filtros por fecha
✅ **Tests críticos**: Night shifts, coherencia de windows, timezone paranoia
✅ No requiere cambios en base de datos
✅ **Sin backward compatibility**: Podemos hacer cambios más agresivos y limpios

**⚠️ 3 "Must" Antes de Implementar Completo** (Feedback Crítico):

1. **✅ Dejar de usar `TRUNC(SYSDATE)` en lecturas** → Usar `IClock` como fuente de verdad única (app timezone)
2. **✅ Validación de coherencia temporal al crear `SHIFT_WINDOWS`** → `from.StartAt < to.StartAt` y `from.EndAt <= to.StartAt`
3. **✅ Get-or-create idempotente real** → Patrón insert-then-select con manejo de `ORA-00001` y re-select

**Estimación**: 3-4 días de desarrollo + 1-2 días de testing (incluye tests críticos).

**Riesgo**: Bajo (cambios aislados, código más robusto que antes) **si se implementan los 3 "must"**.

**Recomendación**: Implementar ahora (big-bang refactor) ya que no hay código productivo. Las mejoras de diseño (`DateOnly`, `IClock`, validaciones en handlers) previenen bugs sutiles relacionados con timezone y semántica de fechas, especialmente crítico para night shifts que cruzan medianoche. **Con los 3 "must" implementados, esto queda sólido y muy difícil de romper después con timezones o shifts nocturnos.**

---

## Checklist de Implementación

### Fase 1: Infraestructura Base
- [ ] Crear `IClock` interface
- [ ] Crear `SystemClock` implementation basado en `TimeProvider` (.NET 8+)
- [ ] Registrar `IClock` en DI container (inyectar `TimeProvider` en `SystemClock`)
- [ ] Crear `DateParsingHelper.TryParseIsoDateOnly()` helper para centralizar parseo de fechas

### Fase 2: Repositorios (Persistencia Pura)
- [ ] Modificar `IAssignmentRepository`: `DateOnly assignmentDate` (requerido)
- [ ] Modificar `AssignmentRepository`: Eliminar `DateTime.Today`, convertir `DateOnly` a `DateTime`
- [ ] **Eliminar validaciones** de pasado/futuro de repositorio
- [ ] Modificar `IHandoverRepository`: `DateOnly baseDate` (requerido)
- [ ] Modificar `HandoverRepository`: Eliminar `DateTime.Today`, convertir `DateOnly` a `DateTime`
- [ ] **Eliminar validaciones** de pasado/futuro de repositorio
- [ ] **Implementar validación de coherencia temporal** en `GetOrCreateShiftWindowAsync`: `from.StartAt < to.StartAt` y `from.EndAt <= to.StartAt`
- [ ] **Implementar get-or-create idempotente real** con patrón insert-then-select y manejo de `ORA-00001` para `GetOrCreateShiftInstanceAsync` y `GetOrCreateShiftWindowAsync`
- [ ] **Asegurar unique constraints** en DB: `UQ_SI_SHIFT_START` y `UQ_SW_PAIR`

### Fase 3: Commands
- [ ] Modificar `PostAssignmentsCommand`: `DateOnly AssignmentDate` (requerido)
- [ ] Modificar `CreateHandoverCommand`: `DateOnly BaseDate` (requerido)

### Fase 4: Handlers (Validaciones)
- [ ] Modificar `PostAssignmentsHandler`: Inyectar `IClock`, agregar validaciones, pasar `DateOnly`
- [ ] Modificar `CreateHandoverHandler`: Inyectar `IClock`, agregar validaciones, pasar `DateOnly`
- [ ] Modificar `PatientAssignedToShiftHandler`: Inyectar `IClock`, obtener fecha del shift instance, aplicar política auto-handover

### Fase 5: Endpoints (Normalización y Parseo)
- [ ] Crear helper `DateParsingHelper.TryParseIsoDateOnly()` para centralizar parseo de fechas
- [ ] Modificar `PostAssignmentsRequest`: `string? AssignmentDate` (formato "YYYY-MM-DD")
- [ ] Modificar `PostAssignments` endpoint: Inyectar `IClock`, usar helper para parsear, normalizar, validar, devolver errores reales del handler
- [ ] Modificar `CreateHandoverRequestDto`: `string? BaseDate` (formato "YYYY-MM-DD")
- [ ] Modificar `CreateHandover` endpoint: Inyectar `IClock`, usar helper para parsear, normalizar, validar, devolver errores reales del handler
- [ ] Crear `SchedulingOptions` y registrar en DI (`IOptions<SchedulingOptions>`)
- [ ] **Eliminar `TRUNC(SYSDATE)` de todas las queries** → usar `_clock.Today()` y pasar `todayStart` como parámetro

### Fase 6: Testing
- [ ] Test: Night shift baseDate (endAt debe ser día siguiente)
- [ ] Test: Window coherencia (FROM.endAt <= TO.startAt)
- [ ] Test: Validación de coherencia temporal en `GetOrCreateShiftWindowAsync` (ventana invertida debe fallar)
- [ ] Test: Get-or-create idempotente con concurrencia (simular race condition)
- [ ] Test: Timezone paranoia (usar FakeClock)
- [ ] Test: Consistencia de timezone (app vs DB - no mezclar `TRUNC(SYSDATE)` con `IClock`)
- [ ] Test: Auto-handover para hoy y mañana
- [ ] Test: Auto-handover NO se crea para >1 día futuro
- [ ] Test: Validación de fechas pasadas
- [ ] Test: Validación de límite de días futuros

### Fase 7: Documentación
- [ ] Documentar semántica de `baseDate` (fecha del inicio del FROM shift)
- [ ] Documentar política de auto-handover (hoy + mañana, configurable)
- [ ] Documentar uso de `IClock` para testing

---

## Notas de Diseño Finales

### Semántica de `BaseDate` (Crítico)

> **`BaseDate` es la fecha del inicio del FROM shift** (StartAt.Date del shift instance FROM).

**Ejemplos**:
- Day shift (07:00-15:00) con `baseDate=2025-12-01` → arranca `2025-12-01 07:00`
- Night shift (19:00-07:00) con `baseDate=2025-12-01` → arranca `2025-12-01 19:00`, termina `2025-12-02 07:00`
- Para window Night→Day: `baseDate` es la fecha de inicio de la noche

### Política de Auto-Handover

**Política Final**: Crear auto-handover si `shiftDate <= today + MaxAutoHandoverDays`.

**Default**: `MaxAutoHandoverDays = 1` (hoy y mañana).

**Configuración**: vía `appsettings.json` (puede ajustarse por organización).

**Condiciones adicionales**:
- Si no existe próximo shift → no crear
- Si no hay coverage esperado para TO → dejar en Draft (ya implementado)

### Principios de Diseño

1. **Repositorios = Persistencia Pura**: Sin validaciones de negocio, sin `DateTime.Today` hardcodeado
2. **Validaciones en Handlers**: Usar `IClock.Today()` para obtener "hoy" (DateOnly)
3. **`DateOnly` para Fechas Base**: Elimina ambigüedad de timezone
4. **Normalización en Endpoints**: Mantiene API flexible (acepta string `"YYYY-MM-DD"`), código interno simple
5. **Semántica Clara**: Documentar qué significa cada fecha
6. **IClock Simple**: Solo `Today()` sin parámetros; timezone por unidad se maneja con servicio separado
7. **Fuente de Verdad Única para "Hoy"**: Usar `IClock` (app timezone) en todo, NO mezclar con `TRUNC(SYSDATE)` (DB timezone)
8. **Repositorios Puros**: Repos reciben `todayStart` como parámetro (calculado en handler), NO usan `IClock` internamente
9. **Get-or-Create Idempotente**: Patrón insert-then-select con manejo de `ORA-00001` y unique constraints
10. **Validación de Coherencia Temporal**: Validar `from.StartAt < to.StartAt` y `from.EndAt <= to.StartAt` en `SHIFT_WINDOWS`
11. **Centralización de Parseo**: Helper `TryParseIsoDateOnly()` simple sin acoplamiento al framework

---

## Orden de Implementación Recomendado (Commits Incrementales)

Para evitar romper todo de una vez, se recomienda implementar en este orden:

### Commit 1: Infraestructura Base
- Crear `IClock` interface
- Crear `SystemClock` implementation (basado en `TimeProvider`)
- Crear `SchedulingOptions` (unificado)
- Crear `DateParsingHelper.TryParseIsoDateOnly()`
- Registrar en DI container

### Commit 2: Commands y Handlers (DateOnly)
- Modificar `PostAssignmentsCommand`: `DateOnly AssignmentDate`
- Modificar `CreateHandoverCommand`: `DateOnly BaseDate`
- Modificar handlers: inyectar `IClock` y `IOptions<SchedulingOptions>`, agregar validaciones

### Commit 3: Repositorios (Signatures + Conversiones)
- Modificar `IAssignmentRepository`: `DateOnly assignmentDate`
- Modificar `AssignmentRepository`: eliminar `DateTime.Today`, convertir `DateOnly` a `DateTime`
- Modificar `IHandoverRepository`: `DateOnly baseDate`
- Modificar `HandoverRepository`: eliminar `DateTime.Today`, convertir `DateOnly` a `DateTime`
- **Eliminar validaciones** de pasado/futuro de repositorios

### Commit 4: Get-or-Create Idempotente + Validación Window
- Implementar patrón insert-then-select con manejo de `ORA-00001` en `GetOrCreateShiftInstanceAsync`
- Implementar patrón insert-then-select con manejo de `ORA-00001` en `GetOrCreateShiftWindowAsync`
- Agregar validación de coherencia temporal en `GetOrCreateShiftWindowAsync` (`from.StartAt < to.StartAt` y `from.EndAt <= to.StartAt`)
- Asegurar unique constraints en DB: `UQ_SI_SHIFT_START` y `UQ_SW_PAIR`

### Commit 5: Endpoints (Normalización y Parseo)
- Modificar `PostAssignmentsRequest`: `string? AssignmentDate`
- Modificar `PostAssignments` endpoint: usar `DateParsingHelper.TryParseIsoDateOnly()`
- Modificar `CreateHandoverRequestDto`: `string? BaseDate`
- Modificar `CreateHandover` endpoint: usar `DateParsingHelper.TryParseIsoDateOnly()`

### Commit 6: Reemplazar `TRUNC(SYSDATE)` en Lecturas
- Modificar queries para recibir `todayStart` como parámetro
- Actualizar handlers para calcular `todayStart` desde `IClock` y pasarlo a repos
- Eliminar todas las referencias a `TRUNC(SYSDATE)` en queries

### Commit 7: Tests Críticos
- Test: Night shift baseDate (endAt debe ser día siguiente)
- Test: Window coherencia (FROM.endAt <= TO.startAt)
- Test: Validación de coherencia temporal en `GetOrCreateShiftWindowAsync` (ventana invertida debe fallar)
- Test: Get-or-create idempotente con concurrencia (simular race condition con `ORA-00001`)
- Test: Timezone paranoia (usar `FakeTimeProvider`)
- Test: Consistencia de timezone (app vs DB - no mezclar `TRUNC(SYSDATE)` con `IClock`)
- Test: Auto-handover para hoy y mañana
- Test: Auto-handover NO se crea para >1 día futuro
- Test: Validación de fechas pasadas
- Test: Validación de límite de días futuros

**Ventajas de este orden**:
- ✅ Cada commit es compilable y testeable
- ✅ Cambios incrementales, fácil de revisar
- ✅ Si algo falla, es fácil identificar en qué commit
- ✅ Permite hacer rollback granular si es necesario

