# Migración Frontend V2 → Backend V3

## Resumen Ejecutivo

Este documento detalla las diferencias entre el frontend (actualmente en V2) y el backend (actualizado a V3), identificando todos los endpoints, estructuras de datos y comportamientos que requieren actualización.

**Estado Actual:**
- ✅ Backend: **V3** (actualizado)
- ❌ Frontend: **V2** (desactualizado)

**Estrategia de Migración:**
- 🔥 **Big Bang Refactor** - No es código productivo, podemos deprecar y eliminar código antiguo
- ✅ **Eliminar duplicación** - Hay funciones/hooks duplicados que deben consolidarse
- ✅ **Usar endpoints `/me/*`** - Priorizar endpoints autenticados sobre públicos
- ✅ **Un solo contrato** - No soportar formatos híbridos, eliminar completamente código V2

---

## 1. Cambios en Estructura de Endpoints

### 1.1. Endpoints Duplicados: `/handovers/*` vs `/me/handovers/*`

El backend V3 introduce una separación clara entre endpoints **públicos** (`/handovers/*`) y endpoints **autenticados** (`/me/handovers/*`). El frontend actualmente usa una mezcla inconsistente.

#### Contingency Plans

**Backend V3 - Dos rutas disponibles:**

1. **Público (sin autenticación requerida):**
   - `GET /handovers/{handoverId}/contingency-plans`
   - `POST /handovers/{handoverId}/contingency-plans`
   - `DELETE /handovers/{handoverId}/contingency-plans/{contingencyId}`

2. **Autenticado (requiere usuario):**
   - `GET /me/handovers/{handoverId}/contingency-plans`
   - `POST /me/handovers/{handoverId}/contingency-plans`
   - `DELETE /me/handovers/{handoverId}/action-items/{itemId}` (no existe para contingency plans)

**Frontend V2 - Estado actual:**
```typescript
// ❌ INCORRECTO: Usa endpoint público pero debería usar /me/ para operaciones autenticadas
export async function getHandoverContingencyPlans(handoverId: string): Promise<Array<HandoverContingencyPlan>> {
	const { data } = await api.get<Array<HandoverContingencyPlan>>(`/me/handovers/${handoverId}/contingency-plans`);
	return data;
}

export async function createContingencyPlan(...): Promise<...> {
	const { data } = await api.post<...>(`/me/handovers/${handoverId}/contingency-plans`, ...);
	return data;
}
```

**✅ Recomendación V3 (Big Bang - Eliminar endpoints públicos):**
- 🔥 **ELIMINAR completamente** todos los endpoints `/handovers/*/contingency-plans` del frontend
- ✅ **Usar únicamente** `/me/handovers/*/contingency-plans` para todas las operaciones
- ✅ **Un solo contrato:** `GET /me/... -> { contingencyPlans: [...] }`
- ⚠️ **Nota sobre DELETE:** El backend no expone `DELETE /me/handovers/{id}/contingency-plans/{id}`, solo existe `DELETE /handovers/{id}/contingency-plans/{id}`. Esta inconsistencia requiere decisión explícita (ver sección 1.2)

#### Action Items

**Backend V3:**
- `GET /me/handovers/{handoverId}/action-items` ✅
- `POST /me/handovers/{handoverId}/action-items` ✅
- `PUT /me/handovers/{handoverId}/action-items/{itemId}` ✅
- `DELETE /me/handovers/{handoverId}/action-items/{itemId}` ✅

**Frontend V2 - Estado actual:**
```typescript
// ✅ CORRECTO: Ya usa /me/ para action items
export async function getHandoverActionItems(handoverId: string): Promise<{...}> {
	const { data } = await api.get<{...}>(`/me/handovers/${handoverId}/action-items`);
	return data;
}
```

#### Messages

**Backend V3:**
- `GET /me/handovers/{handoverId}/messages` ✅
- `POST /me/handovers/{handoverId}/messages` ✅

**Frontend V2 - Estado actual:**
```typescript
// ✅ CORRECTO: Ya usa /me/ para messages
export async function getHandoverMessages(...): Promise<Array<HandoverMessage>> {
	const data = await authenticatedApiCall<{messages: Array<HandoverMessage>}>({
		method: "GET",
		url: `/me/handovers/${handoverId}/messages`,
	});
	return data.messages;
}
```

### 1.2. DELETE de Contingency Plans - Inconsistencia en Backend

**⚠️ Decisión Explícita Requerida:**

El backend V3 tiene una inconsistencia en los endpoints de contingency plans:

- ✅ `GET /me/handovers/{id}/contingency-plans` (autenticado)
- ✅ `POST /me/handovers/{id}/contingency-plans` (autenticado)
- ❌ `DELETE /me/handovers/{id}/contingency-plans/{id}` **NO EXISTE**
- ✅ `DELETE /handovers/{id}/contingency-plans/{id}` (requiere auth, pero no es `/me/*`)

**Opciones:**

1. **Opción A - Usar endpoint público con `authenticatedApiCall`:**
   ```typescript
   // Usar /handovers/ pero con authenticatedApiCall (requiere auth igual)
   export async function deleteContingencyPlan(
     authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>,
     handoverId: string,
     contingencyId: string
   ): Promise<void> {
     await authenticatedApiCall({
       method: "DELETE",
       url: `/handovers/${handoverId}/contingency-plans/${contingencyId}`,
     });
   }
   ```

2. **Opción B - Solicitar endpoint `/me/` en backend:**
   - Crear `DELETE /me/handovers/{id}/contingency-plans/{id}` en backend
   - Mantener consistencia total con GET/POST

**✅ Recomendación:** Opción A (usar endpoint público con `authenticatedApiCall`) para no bloquear la migración, pero documentar como deuda técnica.

---

## 2. Cambios en Estructuras de Respuesta

### 2.1. Contingency Plans - Respuestas Diferentes

**Backend V3 - Endpoint `/handovers/{handoverId}/contingency-plans`:**
```csharp
public class GetContingencyPlansResponse
{
  public List<ContingencyPlanDto> Plans { get; set; } = [];
}

public class ContingencyPlanDto
{
    public string Id { get; set; } = string.Empty;
    public string HandoverId { get; set; } = string.Empty;
    public string ConditionText { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Backend V3 - Endpoint `/me/handovers/{handoverId}/contingency-plans`:**
```csharp
public record GetMeContingencyPlansResponse
{
    public IReadOnlyList<ContingencyPlanRecord> ContingencyPlans { get; init; } = [];
}

public record ContingencyPlanRecord(
    string Id,
    string HandoverId,
    string ConditionText,
    string ActionText,
    string Priority,
    string Status,
    string CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
```

**Frontend V2 - Tipo actual:**
```typescript
export type ContingencyPlansResponse = {
	plans: HandoverContingencyPlan[];
};

export type HandoverContingencyPlan = {
	id: string;
	handoverId: string;
	conditionText: string;
	actionText: string;
	priority: "low" | "medium" | "high";
	status: "active" | "planned" | "completed";
	createdBy: string;
	createdAt: string;
	updatedAt: string;
};
```

**⚠️ Problemas identificados:**
1. **CÓDIGO DUPLICADO:** Hay dos sets de funciones para contingency plans:
   - **Antiguo (usar `/handovers/*`):** `getContingencyPlans`, `createHandoverContingencyPlan`, `useContingencyPlans`, `useCreateHandoverContingencyPlan`
   - **Nuevo (usa `/me/handovers/*`):** `getHandoverContingencyPlans`, `createContingencyPlan`, `useHandoverContingencyPlans`, `useCreateContingencyPlan`
2. El endpoint `/me/handovers/*/contingency-plans` devuelve `ContingencyPlans` (plural), no `plans`
3. El componente `SituationAwareness.tsx` usa `useContingencyPlans` (antiguo) que espera `plans`, pero debería usar `/me/` que devuelve `contingencyPlans`
4. El tipo `status` en el frontend es un union type, pero el backend devuelve un string genérico

**✅ Solución V3 (Big Bang - Eliminar código antiguo, un solo contrato):**
```typescript
// ✅ ELIMINAR estas funciones antiguas:
// - getContingencyPlans
// - createHandoverContingencyPlan  
// - useContingencyPlans
// - useCreateHandoverContingencyPlan
// - ContingencyPlansResponse (tipo que soporta 'plans')

// ✅ MANTENER y actualizar estas funciones (UN SOLO CONTRATO):
export type MeContingencyPlansResponse = {
	contingencyPlans: HandoverContingencyPlan[]; // ✅ Único formato V3
};

export async function getHandoverContingencyPlans(
	authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>,
	handoverId: string
): Promise<Array<HandoverContingencyPlan>> {
	const data = await authenticatedApiCall<MeContingencyPlansResponse>({
		method: "GET",
		url: `/me/handovers/${handoverId}/contingency-plans`,
	});
	return data.contingencyPlans; // ✅ Extraer del objeto de respuesta
}
```

**⚠️ Nota sobre Casing:**
- El backend usa `System.Text.Json` con camelCase por defecto
- **Confirmar una vez** con Swagger/response real que los campos vienen en camelCase

### 2.2. Create Contingency Plan - Respuestas Diferentes

**Backend V3 - `/handovers/{handoverId}/contingency-plans` (POST):**
```csharp
public class CreateContingencyPlanResponse
{
    public ContingencyPlanDto Plan { get; set; } = new();
}
```

**Backend V3 - `/me/handovers/{handoverId}/contingency-plans` (POST):**
```csharp
public record CreateMeContingencyPlanResponse
{
    public bool Success { get; init; }
    public ContingencyPlanRecord? ContingencyPlan { get; init; }
}
```

**Frontend V2 - Estado actual:**
```typescript
// ❌ CÓDIGO ANTIGUO (eliminar):
export async function createHandoverContingencyPlan(...): Promise<HandoverContingencyPlan> {
	const { data } = await api.post<HandoverContingencyPlan>(`/handovers/${handoverId}/contingency-plans`, request);
	return data;
}

// ⚠️ CÓDIGO NUEVO (actualizar):
export async function createContingencyPlan(...): Promise<{ success: boolean; message: HandoverContingencyPlan }> {
	const { data } = await api.post<{ success: boolean; message: HandoverContingencyPlan }>(
		`/me/handovers/${handoverId}/contingency-plans`,
		{ conditionText, actionText, priority }
	);
	return data;
}
```

**⚠️ Problemas:**
1. Hay dos funciones: `createHandoverContingencyPlan` (antigua, `/handovers/*`) y `createContingencyPlan` (nueva, `/me/handovers/*`)
2. El frontend espera `{ success: boolean; message: HandoverContingencyPlan }`
3. El backend devuelve `{ success: boolean; contingencyPlan: ContingencyPlanRecord }`
4. `createContingencyPlan` no usa `authenticatedApiCall` pero debería (es endpoint `/me/*`)

**✅ Solución V3 (Big Bang - Eliminar código antiguo):**
```typescript
// ✅ ELIMINAR: createHandoverContingencyPlan, useCreateHandoverContingencyPlan

// ✅ ACTUALIZAR: createContingencyPlan
export async function createContingencyPlan(
	authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>,
	handoverId: string,
	conditionText: string,
	actionText: string,
	priority: "low" | "medium" | "high" = "medium"
): Promise<{ success: boolean; contingencyPlan: HandoverContingencyPlan }> {
	const data = await authenticatedApiCall<{ success: boolean; contingencyPlan: HandoverContingencyPlan }>({
		method: "POST",
		url: `/me/handovers/${handoverId}/contingency-plans`,
		data: { conditionText, actionText, priority },
	});
	return data; // ✅ Backend devuelve 'contingencyPlan', no 'message'
}
```

### 2.3. Action Items - Estructura Correcta

**Backend V3:**
```csharp
public class GetHandoverActionItemsResponse
{
    public IReadOnlyList<HandoverActionItemFullRecord> ActionItems { get; set; } = [];
}

public record HandoverActionItemFullRecord(
    string Id,
    string HandoverId,
    string Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt
);
```

**Frontend V2 - Estado actual:**
```typescript
export async function getHandoverActionItems(handoverId: string): Promise<{
	actionItems: Array<{
		id: string;
		handoverId: string;
		description: string;
		isCompleted: boolean;
		createdAt: string;
		updatedAt: string;
		completedAt: string | null;
	}>;
}> {
	const { data } = await api.get<{...}>(`/me/handovers/${handoverId}/action-items`);
	return data;
}
```

**✅ Estado:** Correcto, pero debería usar un tipo definido en lugar de inline.

---

## 3. Cambios en Campos de Handover

### 3.1. Nuevos Campos en V3

**Backend V3 - `GetHandoverByIdResponse`:**
```csharp
public class GetHandoverByIdResponse
{
  // ... campos existentes ...
  
  // V3 Fields (nuevos)
  public string? ShiftWindowId { get; set; }
  public string? PreviousHandoverId { get; set; }
  public string? SenderUserId { get; set; }
  public string? ReadyByUserId { get; set; }
  public string? StartedByUserId { get; set; }
  public string? CompletedByUserId { get; set; }
  public string? CancelledByUserId { get; set; }
  public string? CancelReason { get; set; }
}
```

**Frontend V2 - Tipo `Handover`:**
```typescript
export type Handover = {
	id: string;
	assignmentId: string;
	patientId: string;
	patientName?: string;
	status: string;
	illnessSeverity: HandoverIllnessSeverity;
	patientSummary: HandoverPatientSummary;
	situationAwarenessDocId?: string;
	synthesis?: HandoverSynthesis;
	shiftName: string;
	createdBy: string;
	assignedTo: string;
	receiverUserId?: string;
	responsiblePhysicianId: string;
	responsiblePhysicianName: string;
	createdAt?: string;
	readyAt?: string;
	startedAt?: string;
	acknowledgedAt?: string;
	acceptedAt?: string;
	completedAt?: string;
	cancelledAt?: string;
	rejectedAt?: string;
	rejectionReason?: string;
	expiredAt?: string;
	handoverType?: "ShiftToShift" | "TemporaryCoverage" | "Consult";
	handoverWindowDate?: string;
	fromShiftId?: string;
	toShiftId?: string;
	toDoctorId?: string;
	stateName: "Draft" | "Ready" | "InProgress" | "Accepted" | "Completed" | "Cancelled" | "Rejected" | "Expired";
	// ❌ FALTAN CAMPOS V3:
	// shiftWindowId?: string;
	// previousHandoverId?: string;
	// senderUserId?: string;
	// readyByUserId?: string;
	// startedByUserId?: string;
	// completedByUserId?: string;
	// cancelledByUserId?: string;
	// cancelReason?: string;
};
```

**✅ Solución V3:**
```typescript
export type Handover = {
	// ... campos existentes ...
	
	// V3 Fields
	shiftWindowId?: string;
	previousHandoverId?: string;
	senderUserId?: string;
	readyByUserId?: string;
	startedByUserId?: string;
	completedByUserId?: string;
	cancelledByUserId?: string;
	cancelReason?: string;
};
```

---

## 4. Cambios en Request Bodies

### 4.1. Update Situation Awareness - Campo `Status` Requerido

**Backend V3:**
```csharp
public class UpdateSituationAwarenessRequest
{
    [FromRoute]
    public string HandoverId { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = string.Empty; // ⚠️ REQUERIDO
}
```

**Frontend V2 - Estado actual:**
```typescript
export type UpdateSituationAwarenessRequest = {
	content: string; // ❌ Falta campo 'status'
};

export async function updateSituationAwareness(handoverId: string, request: UpdateSituationAwarenessRequest): Promise<ApiResponse> {
	const { data } = await api.put<ApiResponse>(`/handovers/${handoverId}/situation-awareness`, request);
	return data;
}
```

**✅ Solución V3:**
```typescript
// ✅ Tipar status como union type para protección en tiempo de compilación
export type SituationAwarenessStatus = "Draft" | "Ready" | "InProgress" | "Completed";

export type UpdateSituationAwarenessRequest = {
	content: string;
	status: SituationAwarenessStatus; // ✅ Agregar campo requerido
};

export function useUpdateSituationAwareness() {
	return useMutation({
		mutationFn: ({
			handoverId,
			content,
			status = "Draft", // ✅ Default para primeras ediciones
		}: {
			handoverId: string;
			content: string;
			status?: SituationAwarenessStatus; // Opcional en hook, pero siempre se envía
		}) => updateSituationAwareness(handoverId, { content, status: status ?? "Draft" }),
		// ...
	});
}
```

**⚠️ Nota importante:**
- El frontend debe poder enviar `status` **siempre**, incluso en primeras ediciones
- Usar default `"Draft"` en el hook si no se proporciona
- Tipar como union type (`"Draft" | "Ready" | ...`) para que TypeScript proteja contra valores inválidos

### 4.2. Update Synthesis - Campo `Status` Requerido

**Backend V3:**
```csharp
public class PutSynthesisRequest
{
    [FromRoute]
    public string HandoverId { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = "Draft"; // ⚠️ REQUERIDO (default: "Draft")
}
```

**Frontend V2 - Estado actual:**
```typescript
export async function updateSynthesis(handoverId: string, request: { content?: string; status: string }): Promise<ApiResponse> {
    const { data } = await api.put<ApiResponse>(`/handovers/${handoverId}/synthesis`, request);
    return data;
}
```

**✅ Estado:** Ya incluye `status`, pero debería validar que siempre se envíe.

### 4.3. Update Patient Data (Clinical Data)

**Backend V3:**
```csharp
public class UpdateClinicalDataRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public string IllnessSeverity { get; set; } = string.Empty;
    public string SummaryText { get; set; } = string.Empty;
    // ❌ NO incluye campo 'status'
}
```

**Frontend V2 - Estado actual:**
```typescript
export type UpdatePatientDataRequest = {
    illnessSeverity: string;
    summaryText?: string;
    status: string; // ⚠️ El backend NO espera este campo
};
```

**✅ Solución V3:**
```typescript
export type UpdatePatientDataRequest = {
    illnessSeverity: string;
    summaryText?: string;
    // ❌ Remover 'status' - no es parte del request
};
```

---

## 5. Cambios en Estructuras de Respuesta de Secciones

### 5.1. Situation Awareness Response

**Backend V3:**
```csharp
public class GetSituationAwarenessResponse
{
    public SituationAwarenessDto? SituationAwareness { get; set; }
}

public class SituationAwarenessDto
{
    public string HandoverId { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = string.Empty;
    public string LastEditedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
```

**Frontend V2 - Estado actual:**
```typescript
export type SituationAwarenessSection = {
	id: string;
	handoverId: string;
	sectionType: string;
	content: string | null;
	status: string;
	lastEditedBy: string | null;
	createdAt: string;
	updatedAt: string;
};

export type SituationAwarenessResponse = {
	section: SituationAwarenessSection | null;
};
```

**⚠️ Problemas:**
1. El backend devuelve `SituationAwareness` (singular), el frontend espera `section`
2. El backend NO incluye `id`, `sectionType`, `createdAt` - solo `handoverId`, `content`, `status`, `lastEditedBy`, `updatedAt`

**✅ Solución V3:**
```typescript
export type SituationAwarenessDto = {
	handoverId: string;
	content: string | null;
	status: string;
	lastEditedBy: string;
	updatedAt: string; // DateTime serializado como string
};

export type SituationAwarenessResponse = {
	situationAwareness: SituationAwarenessDto | null; // ✅ Cambiar de 'section' a 'situationAwareness'
};
```

### 5.2. Synthesis Response

**Backend V3:**
```csharp
public class GetSynthesisResponse
{
    public SynthesisDto? Synthesis { get; set; }
}

public class SynthesisDto
{
    public string HandoverId { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = string.Empty;
    public string LastEditedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
```

**Frontend V2 - Estado actual:**
```typescript
export type SynthesisDto = {
    handoverId: string;
    content?: string;
    status: string;
    lastEditedBy?: string;
    updatedAt: string;
};

export type SynthesisResponse = {
    synthesis: SynthesisDto | null;
};
```

**✅ Estado:** Estructura correcta, pero `lastEditedBy` debería ser `string` (no opcional) según el backend.

---

## 6. Cambios en Endpoints de Patient Data

### 6.1. Get Patient Data - Estructura Correcta

**Backend V3:**
```csharp
public class GetPatientHandoverDataResponse
{
  public string id { get; set; } = string.Empty;
  public string name { get; set; } = string.Empty;
  public string dob { get; set; } = string.Empty;
  public string mrn { get; set; } = string.Empty;
  public string admissionDate { get; set; } = string.Empty;
  public string currentDateTime { get; set; } = string.Empty;
  public string primaryTeam { get; set; } = string.Empty;
  public string primaryDiagnosis { get; set; } = string.Empty;
  public string room { get; set; } = string.Empty;
  public string unit { get; set; } = string.Empty;
  public PhysicianDto? assignedPhysician { get; set; }
  public PhysicianDto? receivingPhysician { get; set; }
  public string? illnessSeverity { get; set; }
  public string? summaryText { get; set; }
  public string? lastEditedBy { get; set; }
  public string? updatedAt { get; set; }
}
```

**Frontend V2 - Estado actual:**
```typescript
export type PatientHandoverData = {
	id: string;
	name: string;
	dob: string;
	mrn: string;
	admissionDate: string;
	currentDateTime: string;
	primaryTeam: string;
	primaryDiagnosis: string;
	room: string;
	unit: string;
	assignedPhysician: {
		name: string;
		role: string;
		color: string;
		shiftEnd?: string;
		shiftStart?: string;
		status: string;
		patientAssignment: string;
	} | null;
	receivingPhysician: {
		name: string;
		role: string;
		color: string;
		shiftEnd?: string;
		shiftStart?: string;
		status: string;
		patientAssignment: string;
	} | null;
	illnessSeverity?: string;
	summaryText?: string;
	lastEditedBy?: string;
	updatedAt?: string;
};
```

**✅ Estado:** Estructura correcta y alineada con el backend.

---

## 7. Cambios en Prioridades de Contingency Plans

### 7.1. Prioridad en Lowercase

**Backend V3:**
```csharp
public record CreateMeContingencyPlanRequest
{
    public string Priority { get; init; } = "medium"; // V3: Must be lowercase per CHK_CONT_PRIORITY constraint
}
```

**Frontend V2 - Estado actual:**
```typescript
export type CreateContingencyPlanRequest = {
	conditionText: string;
	actionText: string;
	priority: "low" | "medium" | "high"; // ✅ Ya está en lowercase
};
```

**✅ Estado:** Correcto, pero asegurar que siempre se envíe en lowercase.

---

## 8. Endpoints que Requieren Autenticación

### 8.1. Regla de Autenticación: "Autenticado" != "/me"

**⚠️ Regla Crítica:**
> **Si requiere autenticación, SIEMPRE usar `authenticatedApiCall`, incluso si la ruta NO es `/me/*`.**

**Ejemplos de endpoints que requieren auth pero NO son `/me/*`:**
- `POST /handovers/{id}/ready` ✅ Requiere auth
- `POST /handovers/{id}/start` ✅ Requiere auth
- `POST /handovers/{id}/complete` ✅ Requiere auth
- `POST /handovers/{id}/cancel` ✅ Requiere auth
- `POST /handovers/{id}/reject` ✅ Requiere auth
- `PUT /handovers/{id}/patient-data` ✅ Requiere auth
- `PUT /handovers/{id}/situation-awareness` ✅ Requiere auth
- `PUT /handovers/{id}/synthesis` ✅ Requiere auth
- `DELETE /handovers/{id}/contingency-plans/{id}` ✅ Requiere auth (ver sección 1.2)

### 8.2. Endpoints `/me/*` - Requieren Autenticación

Todos los endpoints bajo `/me/*` requieren autenticación y usan `ICurrentUser` para obtener el usuario actual.

**Endpoints afectados:**
- `GET /me/handovers`
- `GET /me/handovers/{handoverId}/action-items`
- `POST /me/handovers/{handoverId}/action-items`
- `PUT /me/handovers/{handoverId}/action-items/{itemId}`
- `DELETE /me/handovers/{handoverId}/action-items/{itemId}`
- `GET /me/handovers/{handoverId}/contingency-plans`
- `POST /me/handovers/{handoverId}/contingency-plans`
- `GET /me/handovers/{handoverId}/messages`
- `POST /me/handovers/{handoverId}/messages`
- `GET /me/patients`
- `GET /me/profile`

**Frontend V2 - Estado actual:**
```typescript
// ✅ CORRECTO: Usa authenticatedApiCall para /me/ endpoints
export async function getHandoverMessages(
	authenticatedApiCall: ReturnType<typeof useAuthenticatedApi>["authenticatedApiCall"],
	handoverId: string
): Promise<Array<HandoverMessage>> {
	const data = await authenticatedApiCall<{messages: Array<HandoverMessage>}>({
		method: "GET",
		url: `/me/handovers/${handoverId}/messages`,
	});
	return data.messages;
}
```

**⚠️ Problema identificado:**
Algunos endpoints `/me/*` en el frontend usan `api.get` directamente en lugar de `authenticatedApiCall`:

```typescript
// ❌ INCORRECTO: No usa authenticatedApiCall
export async function getHandoverContingencyPlans(handoverId: string): Promise<Array<HandoverContingencyPlan>> {
	const { data } = await api.get<Array<HandoverContingencyPlan>>(`/me/handovers/${handoverId}/contingency-plans`);
	return data;
}
```

**✅ Solución V3:**
```typescript
// ✅ CORRECTO: Usar authenticatedApiCall
export async function getHandoverContingencyPlans(
	authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>,
	handoverId: string
): Promise<Array<HandoverContingencyPlan>> {
	const data = await authenticatedApiCall<{contingencyPlans: Array<HandoverContingencyPlan>}>({
		method: "GET",
		url: `/me/handovers/${handoverId}/contingency-plans`,
	});
	return data.contingencyPlans; // ✅ Nota: el backend devuelve 'contingencyPlans', no array directo
}
```

---

## 9. Código a Eliminar (Big Bang Refactor)

### 9.0. Lista de Eliminación

**🔥 ELIMINAR completamente estas funciones y hooks:**

```typescript
// ❌ ELIMINAR de relevo-frontend/src/api/endpoints/handovers.ts:

// Función antigua - usa endpoint público
export async function getContingencyPlans(handoverId: string): Promise<ContingencyPlansResponse> {
	const { data } = await api.get<ContingencyPlansResponse>(`/handovers/${handoverId}/contingency-plans`);
	return data;
}

// Función antigua - usa endpoint público
export async function createHandoverContingencyPlan(
	handoverId: string,
	conditionText: string,
	actionText: string,
	priority: "low" | "medium" | "high" = "medium"
): Promise<HandoverContingencyPlan> {
	const request: CreateContingencyPlanRequest = {
		conditionText,
		actionText,
		priority,
	};
	const { data } = await api.post<HandoverContingencyPlan>(`/handovers/${handoverId}/contingency-plans`, request);
	return data;
}

// Hook antiguo - usa función antigua
export function useContingencyPlans(handoverId: string) {
	return useQuery({
		queryKey: handoverQueryKeys.contingencyPlans(handoverId),
		queryFn: () => getContingencyPlans(handoverId),
		enabled: !!handoverId,
		staleTime: 5 * 60 * 1000,
	});
}

// Hook antiguo - usa función antigua
export function useCreateHandoverContingencyPlan() {
	const queryClient = useQueryClient();
	return useMutation({
		mutationFn: ({ handoverId, conditionText, actionText, priority }) =>
			createHandoverContingencyPlan(handoverId, conditionText, actionText, priority),
		onSuccess: (_data, variables) => {
			queryClient.invalidateQueries({
				queryKey: handoverQueryKeys.contingencyPlans(variables.handoverId),
			});
		},
	});
}
```

**✅ MANTENER y actualizar estas funciones/hooks:**

```typescript
// ✅ MANTENER y actualizar:
// - getHandoverContingencyPlans (actualizar para usar authenticatedApiCall)
// - createContingencyPlan (actualizar para usar authenticatedApiCall y cambiar respuesta)
// - useHandoverContingencyPlans (actualizar para usar función actualizada)
// - useCreateContingencyPlan (actualizar para usar función actualizada)
```

**✅ ACTUALIZAR componentes que usan código antiguo:**

```typescript
// ❌ En SituationAwareness.tsx - CAMBIAR:
const { data: contingencyData } = useContingencyPlans(handoverId); // ❌ Antiguo

// ✅ A:
const { data: contingencyData } = useHandoverContingencyPlans(handoverId); // ✅ Nuevo

// Y actualizar el acceso a los datos:
// ❌ Antiguo: contingencyData?.plans
// ✅ Nuevo: contingencyData?.contingencyPlans
```

---

## 9. Checklist de Migración

### 9.1. Tipos TypeScript

- [ ] Actualizar `Handover` para incluir campos V3:
  - `shiftWindowId`
  - `previousHandoverId`
  - `senderUserId`
  - `readyByUserId`
  - `startedByUserId`
  - `completedByUserId`
  - `cancelledByUserId`
  - `cancelReason`

- [ ] **ELIMINAR** `ContingencyPlansResponse` (tipo antiguo que soporta `plans`)
- [ ] **CREAR** `MeContingencyPlansResponse` con un solo formato:
  - `contingencyPlans: HandoverContingencyPlan[]` (único contrato V3)

- [ ] Actualizar `SituationAwarenessResponse`:
  - Cambiar `section` → `situationAwareness`
  - Remover campos que no existen en el backend: `id`, `sectionType`, `createdAt`

- [ ] Actualizar `UpdateSituationAwarenessRequest`:
  - Agregar campo `status: SituationAwarenessStatus` (requerido, union type)
  - Crear tipo `SituationAwarenessStatus = "Draft" | "Ready" | "InProgress" | "Completed"`
  - En hook, usar default `status ?? "Draft"` para primeras ediciones

- [ ] Actualizar `UpdatePatientDataRequest`:
  - Remover campo `status` (no es parte del request)

- [ ] Actualizar `SynthesisDto`:
  - Cambiar `lastEditedBy?: string` → `lastEditedBy: string`

### 9.2. Funciones API - Eliminar Código Antiguo

**🔥 ELIMINAR (código antiguo que usa `/handovers/*`):**
- [ ] `getContingencyPlans` - Eliminar función antigua
- [ ] `createHandoverContingencyPlan` - Eliminar función antigua
- [ ] `deleteContingencyPlan` (si usa `/handovers/*`) - Verificar y eliminar si corresponde

**✅ ACTUALIZAR (código nuevo que usa `/me/handovers/*`):**
- [ ] `getHandoverContingencyPlans`:
  - Cambiar a usar `authenticatedApiCall`
  - Actualizar tipo de respuesta para usar `contingencyPlans` en lugar de array directo
  - Actualizar para extraer `data.contingencyPlans` del objeto de respuesta

- [ ] `createContingencyPlan`:
  - Cambiar a usar `authenticatedApiCall`
  - Actualizar tipo de respuesta: `message` → `contingencyPlan`

- [ ] `updateSituationAwareness`:
  - Agregar campo `status` al request
  - Actualizar todos los lugares donde se llama para incluir `status`

- [ ] `updatePatientData`:
  - Remover campo `status` del request

- [ ] `getSituationAwareness`:
  - Actualizar para usar `situationAwareness` en lugar de `section`

### 9.3. Hooks React Query - Eliminar Código Antiguo

**🔥 ELIMINAR (hooks antiguos):**
- [ ] `useContingencyPlans` - Eliminar hook antiguo
- [ ] `useCreateHandoverContingencyPlan` - Eliminar hook antiguo

**✅ ACTUALIZAR (hooks nuevos):**
- [ ] `useHandoverContingencyPlans`:
  - Actualizar para usar `authenticatedApiCall` en `getHandoverContingencyPlans`
  - Actualizar tipo de respuesta para usar `contingencyPlans`

- [ ] `useCreateContingencyPlan`:
  - Actualizar para usar `authenticatedApiCall` en `createContingencyPlan`
  - Actualizar tipo de respuesta: `message` → `contingencyPlan`

- [ ] `useUpdateSituationAwareness`:
  - Agregar `status` como parámetro requerido

- [ ] `useUpdatePatientData`:
  - Remover `status` del request

**✅ ACTUALIZAR Componentes:**
- [ ] `SituationAwareness.tsx`:
  - Cambiar `useContingencyPlans` → `useHandoverContingencyPlans`
  - Cambiar `useCreateContingencyPlan` (ya está correcto, pero verificar que use la función actualizada)
  - Actualizar para usar `contingencyPlans` en lugar de `plans` en la respuesta

### 9.4. Componentes UI

- [ ] Revisar todos los componentes que usan `Handover` y asegurar que manejen los nuevos campos V3

- [ ] Revisar componentes que crean/actualizan contingency plans y asegurar que usen `authenticatedApiCall`

- [ ] Revisar componentes que actualizan situation awareness y asegurar que incluyan `status`

---

## 10. Estrategia de Migración Directa (Big Bang)

### 10.0. Cambios Directos en Lugar

**Estrategia:** Cambiar directamente el código existente, sin crear nuevas carpetas o capas.

**Enfoque:**
- 🔥 **Eliminar** funciones/hooks antiguos directamente
- ✅ **Actualizar** funciones/hooks existentes en su lugar
- ✅ **Actualizar** tipos existentes en su lugar
- ✅ **Actualizar** componentes para usar las funciones actualizadas

**Estructura (sin cambios de carpeta):**
```
src/api/
├── endpoints/
│   └── handovers.ts       # ✅ ACTUALIZAR directamente aquí
├── hooks/
│   └── useHandovers.ts    # ✅ ACTUALIZAR directamente aquí
└── types.ts               # ✅ ACTUALIZAR directamente aquí
```

**Regla crítica:**
> **Los componentes NUNCA deben usar `api.get`, `api.post`, etc. directamente. Solo deben importar funciones/hooks de `@/api`.**

**Ejemplo de actualización directa:**
```typescript
// src/api/endpoints/handovers.ts
// ✅ ACTUALIZAR función existente directamente:
export async function getHandoverContingencyPlans(
  authenticatedApiCall: ReturnType<typeof createAuthenticatedApiCall>,
  handoverId: string
): Promise<Array<HandoverContingencyPlan>> {
  const data = await authenticatedApiCall<MeContingencyPlansResponse>({
    method: "GET",
    url: `/me/handovers/${handoverId}/contingency-plans`,
  });
  return data.contingencyPlans; // ✅ Extrae array del objeto de respuesta
}

// src/api/hooks/useHandovers.ts
// ✅ ACTUALIZAR hook existente directamente:
export function useHandoverContingencyPlans(handoverId: string) {
  const { authenticatedApiCall } = useAuthenticatedApi();
  return useQuery({
    queryKey: handoverQueryKeys.contingencyPlans(handoverId),
    queryFn: () => getHandoverContingencyPlans(authenticatedApiCall, handoverId),
    enabled: !!handoverId,
  });
}
```

**Componentes importan de `@/api` (sin cambios de path):**
```typescript
// ✅ Componente - import path no cambia
import { useHandoverContingencyPlans } from "@/api/hooks/useHandovers";

export function SituationAwareness({ handoverId }: Props) {
  const { data } = useHandoverContingencyPlans(handoverId);
  // data es Array<HandoverContingencyPlan>
}
```

---

## 11. Endpoints por Categoría

### 11.1. Handovers - Endpoints Principales

| Endpoint | Método | Autenticación | Estado Frontend V2 | Acción Requerida |
|----------|--------|---------------|-------------------|------------------|
| `/handovers` | POST | ✅ | ✅ Correcto | Ninguna |
| `/handovers/{id}` | GET | ❌ | ✅ Correcto | Ninguna |
| `/handovers/{id}/ready` | POST | ✅ | ✅ Correcto | Ninguna |
| `/handovers/{id}/start` | POST | ✅ | ✅ Correcto | Ninguna |
| `/handovers/{id}/complete` | POST | ✅ | ✅ Correcto | Ninguna |
| `/handovers/{id}/cancel` | POST | ✅ | ✅ Correcto | Ninguna |
| `/handovers/{id}/reject` | POST | ✅ | ✅ Correcto | Ninguna |
| `/handovers/pending` | GET | ❌ | ✅ Correcto | Ninguna |
| `/me/handovers` | GET | ✅ | ✅ Correcto | Ninguna |

### 11.2. Handovers - Secciones

| Endpoint | Método | Autenticación | Estado Frontend V2 | Acción Requerida |
|----------|--------|---------------|-------------------|------------------|
| `/handovers/{id}/patient` | GET | ❌ | ✅ Correcto | Ninguna |
| `/handovers/{id}/patient-data` | PUT | ✅ | ⚠️ Incluye `status` | Remover `status` del request |
| `/handovers/{id}/situation-awareness` | GET | ❌ | ⚠️ Usa `section` | Cambiar a `situationAwareness` |
| `/handovers/{id}/situation-awareness` | PUT | ✅ | ⚠️ Falta `status` | Agregar `status` al request |
| `/handovers/{id}/synthesis` | GET | ❌ | ✅ Correcto | Ninguna |
| `/handovers/{id}/synthesis` | PUT | ✅ | ✅ Correcto | Validar que siempre se envíe `status` |

### 11.3. Contingency Plans

| Endpoint | Método | Autenticación | Estado Frontend V2 | Acción Requerida |
|----------|--------|---------------|-------------------|------------------|
| `/handovers/{id}/contingency-plans` | GET | ❌ | 🔥 **ELIMINAR** - Código antiguo | Eliminar `getContingencyPlans`, `useContingencyPlans` |
| `/handovers/{id}/contingency-plans` | POST | ✅ | 🔥 **ELIMINAR** - Código antiguo | Eliminar `createHandoverContingencyPlan`, `useCreateHandoverContingencyPlan` |
| `/handovers/{id}/contingency-plans/{id}` | DELETE | ✅ | ⚠️ Inconsistencia backend | Usar `/handovers/` con `authenticatedApiCall` (ver sección 1.2) |
| `/me/handovers/{id}/contingency-plans` | GET | ✅ | ⚠️ No usa `authenticatedApiCall` | Cambiar a `authenticatedApiCall`, actualizar respuesta `contingencyPlans` |
| `/me/handovers/{id}/contingency-plans` | POST | ✅ | ⚠️ No usa `authenticatedApiCall` | Cambiar a `authenticatedApiCall`, actualizar respuesta `contingencyPlan` |

### 11.4. Action Items

| Endpoint | Método | Autenticación | Estado Frontend V2 | Acción Requerida |
|----------|--------|---------------|-------------------|------------------|
| `/me/handovers/{id}/action-items` | GET | ✅ | ✅ Correcto | Ninguna |
| `/me/handovers/{id}/action-items` | POST | ✅ | ✅ Correcto | Ninguna |
| `/me/handovers/{id}/action-items/{id}` | PUT | ✅ | ✅ Correcto | Ninguna |
| `/me/handovers/{id}/action-items/{id}` | DELETE | ✅ | ✅ Correcto | Ninguna |

### 11.5. Messages

| Endpoint | Método | Autenticación | Estado Frontend V2 | Acción Requerida |
|----------|--------|---------------|-------------------|------------------|
| `/me/handovers/{id}/messages` | GET | ✅ | ✅ Correcto | Ninguna |
| `/me/handovers/{id}/messages` | POST | ✅ | ✅ Correcto | Ninguna |

### 11.6. Patients

| Endpoint | Método | Autenticación | Estado Frontend V2 | Acción Requerida |
|----------|--------|---------------|-------------------|------------------|
| `/patients` | GET | ❌ | ✅ Correcto | Ninguna |
| `/patients/{id}` | GET | ❌ | ✅ Correcto | Ninguna |
| `/patients/{id}/handovers` | GET | ❌ | ✅ Correcto | Ninguna |
| `/patients/{id}/summary` | GET | ✅ | ✅ Correcto | Ninguna |
| `/patients/{id}/summary` | POST | ✅ | ✅ Correcto | Ninguna |
| `/patients/{id}/summary` | PUT | ✅ | ✅ Correcto | Ninguna |
| `/me/patients` | GET | ✅ | ✅ Correcto | Ninguna |

---

## 12. Grep Checklist (Buscar Referencias)

Antes de empezar la migración, ejecutar estos greps para encontrar todas las referencias:

### 12.1. Buscar Código Antiguo a Eliminar

```bash
# Buscar funciones/hooks antiguos de contingency plans
grep -r "getContingencyPlans" relevo-frontend/src/
grep -r "createHandoverContingencyPlan" relevo-frontend/src/
grep -r "useContingencyPlans" relevo-frontend/src/
grep -r "useCreateHandoverContingencyPlan" relevo-frontend/src/

# Buscar endpoints antiguos de contingency plans
grep -r "/handovers/.*contingency-plans" relevo-frontend/src/
grep -r '"/handovers/.*contingency-plans"' relevo-frontend/src/

# Buscar tipos antiguos
grep -r "ContingencyPlansResponse" relevo-frontend/src/
grep -r "\.plans" relevo-frontend/src/  # Acceso a propiedad antigua
```

### 12.2. Buscar Problemas de Autenticación

```bash
# Buscar api.get/api.post en rutas que requieren auth
grep -r "api\.get.*handovers.*ready\|start\|complete\|cancel\|reject" relevo-frontend/src/
grep -r "api\.post.*handovers.*ready\|start\|complete\|cancel\|reject" relevo-frontend/src/
grep -r "api\.put.*handovers.*patient-data\|situation-awareness\|synthesis" relevo-frontend/src/

# Buscar /me/ endpoints que no usan authenticatedApiCall
grep -r "api\.get.*/me/" relevo-frontend/src/
grep -r "api\.post.*/me/" relevo-frontend/src/
```

### 12.3. Buscar Problemas de Shape

```bash
# Buscar acceso a propiedades incorrectas
grep -r "\.section" relevo-frontend/src/  # Debería ser .situationAwareness
grep -r "\.plans" relevo-frontend/src/    # Debería ser .contingencyPlans
grep -r "\.message" relevo-frontend/src/   # Debería ser .contingencyPlan (en create)
```

### 12.4. Buscar Problemas de Status

```bash
# Buscar updateSituationAwareness sin status
grep -r "updateSituationAwareness" relevo-frontend/src/ -A 5
# Verificar que todas las llamadas incluyan status

# Buscar updatePatientData con status (debería NO tenerlo)
grep -r "updatePatientData" relevo-frontend/src/ -A 5
```

---

## 13. Resumen de Cambios Críticos

### 🔴 Críticos (Rompen funcionalidad)

1. **Contingency Plans - Código Duplicado y Respuestas Incorrectas:**
   - 🔥 **CÓDIGO DUPLICADO:** Hay funciones/hooks antiguos que usan `/handovers/*` y nuevos que usan `/me/handovers/*`
   - Frontend espera `plans` (código antiguo) pero backend devuelve `contingencyPlans` (en `/me/`)
   - Frontend espera `{ success, message }` pero backend devuelve `{ success, contingencyPlan }`
   - `getHandoverContingencyPlans` no usa `authenticatedApiCall` pero debería (es `/me/*`)
   - `createContingencyPlan` no usa `authenticatedApiCall` pero debería (es `/me/*`)

2. **Situation Awareness - Estructura incorrecta:**
   - Frontend espera `section` pero backend devuelve `situationAwareness`
   - Frontend incluye campos que no existen: `id`, `sectionType`, `createdAt`

3. **Update Situation Awareness - Falta campo `status`:**
   - Backend requiere `status` pero frontend no lo envía

### 🟡 Importantes (Funcionalidad parcial)

1. **Handover - Faltan campos V3:**
   - Nuevos campos de tracking no están en el tipo TypeScript

2. **Contingency Plans - No usa autenticación:**
   - Algunos endpoints usan `api.get` en lugar de `authenticatedApiCall`

3. **Update Patient Data - Campo extra:**
   - Frontend envía `status` pero backend no lo espera (puede causar errores)

### 🟢 Menores (Mejoras)

1. **Tipos inline vs definidos:**
   - Algunos tipos están inline y deberían estar definidos

2. **Validación de prioridades:**
   - Asegurar que siempre se envíe en lowercase

---

## 14. Definition of Done (DoD)

La migración está completa cuando:

### 14.1. Código Eliminado

- [x] ✅ No existen referencias a `useContingencyPlans` (hook antiguo)
- [x] ✅ No existen calls a `getContingencyPlans` (función antigua)
- [x] ✅ No existen calls a `createHandoverContingencyPlan` (función antigua)
- [x] ✅ No existen referencias a `useCreateHandoverContingencyPlan` (hook antiguo)
- [x] ✅ No existen endpoints `/handovers/*/contingency-plans` en el código
- [x] ✅ No existe tipo `ContingencyPlansResponse` (tipo antiguo con `plans`)

### 14.2. Autenticación

- [x] ✅ Todos los endpoints `/me/*` usan `authenticatedApiCall`
- [x] ✅ Todos los endpoints que requieren auth (aunque no sean `/me/*`) usan `authenticatedApiCall`:
  - `/handovers/{id}/ready`
  - `/handovers/{id}/start`
  - `/handovers/{id}/complete`
  - `/handovers/{id}/cancel`
  - `/handovers/{id}/reject`
  - `/handovers/{id}/patient-data` (PUT)
  - `/handovers/{id}/situation-awareness` (PUT)
  - `/handovers/{id}/synthesis` (PUT)

### 14.3. Estructuras de Datos

- [x] ✅ `SituationAwareness` PUT siempre manda `status` (con default "Draft" si no se proporciona)
- [x] ✅ `UpdatePatientData` NO manda `status` (removido del request)
- [x] ✅ Respuestas usan `situationAwareness` (no `section`)
- [x] ✅ Respuestas usan `contingencyPlans` (no `plans`)
- [x] ✅ Create contingency plan usa `contingencyPlan` (no `message`)

### 14.4. Tipos TypeScript

- [x] ✅ `Handover` incluye todos los campos V3
- [x] ✅ `SituationAwarenessStatus` es union type
- [x] ✅ `UpdateSituationAwarenessRequest` incluye `status: SituationAwarenessStatus`
- [x] ✅ `UpdatePatientDataRequest` NO incluye `status`
- [x] ✅ `SituationAwarenessResponse` usa `situationAwareness` (no `section`)
- [x] ✅ `MeContingencyPlansResponse` usa `contingencyPlans` (único contrato)

### 14.5. Componentes

- [x] ✅ `SituationAwareness.tsx` usa `useHandoverContingencyPlans` (no `useContingencyPlans`)
- [x] ✅ Todos los componentes acceden a `data.contingencyPlans` (no `data.plans`)
- [x] ✅ Todos los componentes acceden a `data.situationAwareness` (no `data.section`)

### 14.6. Testing

- [ ] ✅ Flujos de contingency plans funcionan end-to-end
- [ ] ✅ Actualización de situation awareness funciona con `status`
- [x] ✅ No hay errores de compilación TypeScript
- [x] ✅ No hay warnings de linter relacionados con código antiguo

---

## 15. Orden Recomendado de Implementación (Big Bang Refactor)

1. **Fase 1 - Eliminar Código Antiguo:**
   - 🔥 Eliminar `getContingencyPlans` (usa `/handovers/*`)
   - 🔥 Eliminar `createHandoverContingencyPlan` (usa `/handovers/*`)
   - 🔥 Eliminar `useContingencyPlans` (hook antiguo)
   - 🔥 Eliminar `useCreateHandoverContingencyPlan` (hook antiguo)
   - Buscar y eliminar cualquier otra referencia a endpoints `/handovers/*/contingency-plans`

2. **Fase 2 - Tipos TypeScript:**
   - Actualizar todos los tipos según sección 9.1
   - Eliminar tipos obsoletos relacionados con código antiguo
   - Esto ayudará a identificar errores de compilación

3. **Fase 3 - Funciones API Core:**
   - Actualizar `getHandoverContingencyPlans`:
     - Agregar `authenticatedApiCall` como parámetro
     - Cambiar respuesta de array directo a `{ contingencyPlans: [...] }`
   - Actualizar `createContingencyPlan`:
     - Agregar `authenticatedApiCall` como parámetro
     - Cambiar respuesta `message` → `contingencyPlan`
   - Actualizar `updateSituationAwareness`:
     - Agregar campo `status` al request
   - Actualizar `getSituationAwareness`:
     - Cambiar `section` → `situationAwareness` en respuesta

4. **Fase 4 - Hooks React Query:**
   - Actualizar `useHandoverContingencyPlans` para usar `authenticatedApiCall`
   - Actualizar `useCreateContingencyPlan` para usar `authenticatedApiCall`
   - Actualizar `useUpdateSituationAwareness` para incluir `status`
   - Actualizar `useUpdatePatientData` para remover `status`

5. **Fase 5 - Componentes UI:**
   - Actualizar `SituationAwareness.tsx`:
     - Cambiar `useContingencyPlans` → `useHandoverContingencyPlans`
     - Actualizar para usar `contingencyPlans` en lugar de `plans`
   - Actualizar todos los componentes que usan los hooks actualizados
   - Agregar manejo de nuevos campos V3 en Handover

6. **Fase 6 - Testing:**
   - Probar todos los flujos de contingency plans
   - Probar actualización de situation awareness
   - Validar que los nuevos campos V3 se muestren correctamente
   - Verificar que no queden referencias a código eliminado

---

## 16. Notas Adicionales

### 16.1. Estrategia Big Bang - Sin Compatibilidad Backward

**🔥 Eliminar código antiguo:**
- No necesitamos mantener compatibilidad con endpoints públicos `/handovers/*/contingency-plans`
- Eliminar todas las funciones/hooks que usan endpoints antiguos
- Consolidar en una sola implementación usando `/me/handovers/*`

**✅ Priorizar endpoints autenticados:**
- Usar siempre `/me/handovers/*` para operaciones que requieren contexto de usuario
- Esto simplifica el código y asegura que todas las operaciones estén autenticadas

### 16.2. Validación de Prioridades

El backend tiene una constraint `CHK_CONT_PRIORITY` que valida que las prioridades sean lowercase. Asegurar que el frontend siempre envíe valores en lowercase.

### 16.3. Campos Opcionales vs Requeridos

Algunos campos que eran opcionales en V2 ahora son requeridos en V3 (ej: `status` en `UpdateSituationAwarenessRequest`). Revisar todas las validaciones del frontend.

### 16.4. Casing en JSON (camelCase)

**Confirmación requerida:**
- El backend usa `System.Text.Json` que por defecto serializa en camelCase
- **Verificar una vez** con Swagger/response real que los campos vienen en camelCase

**Ejemplo de verificación:**
```typescript
// Verificar respuesta real del backend
const response = await authenticatedApiCall({ ... });
console.log(response); // Verificar que los campos sean camelCase
```

---

## 17. Plan de Implementación

**Orden recomendado de trabajo:**

### Fase 1: Big Bang - Actualizar Todo Directamente

**Objetivo:** Actualizar código directamente en lugar, eliminar código antiguo

**Cambios (en orden):**
- [x] **Tipos TypeScript:**
  - Actualizar `Handover` con campos V3
  - Eliminar `ContingencyPlansResponse` (tipo antiguo)
  - Crear `MeContingencyPlansResponse` (único contrato)
  - Actualizar `SituationAwarenessResponse` (`section` → `situationAwareness`)
  - Actualizar `UpdateSituationAwarenessRequest` (agregar `status`)
  - Actualizar `UpdatePatientDataRequest` (remover `status`)
  - Crear `SituationAwarenessStatus` union type

- [x] **Funciones API (actualizar directamente):**
  - **ELIMINAR:** `getContingencyPlans`, `createHandoverContingencyPlan`
  - **ACTUALIZAR:** `getHandoverContingencyPlans` (usar `authenticatedApiCall`, `contingencyPlans`)
  - **ACTUALIZAR:** `createContingencyPlan` (usar `authenticatedApiCall`, `contingencyPlan`)
  - **ACTUALIZAR:** `updateSituationAwareness` (agregar `status`)
  - **ACTUALIZAR:** `updatePatientData` (remover `status`)
  - **ACTUALIZAR:** `getSituationAwareness` (`section` → `situationAwareness`)

- [x] **Hooks React Query (actualizar directamente):**
  - **ELIMINAR:** `useContingencyPlans`, `useCreateHandoverContingencyPlan`
  - **ACTUALIZAR:** `useHandoverContingencyPlans` (usar función actualizada)
  - **ACTUALIZAR:** `useCreateContingencyPlan` (usar función actualizada)
  - **ACTUALIZAR:** `useUpdateSituationAwareness` (incluir `status` con default)
  - **ACTUALIZAR:** `useUpdatePatientData` (remover `status`)

- [x] **Componentes (actualizar directamente):**
  - `SituationAwareness.tsx` → usar `useHandoverContingencyPlans`, `contingencyPlans`
  - Todos los componentes que usan contingency plans
  - Todos los componentes que actualizan situation awareness
  - Todos los componentes que actualizan patient data

**Verificación:**
- [ ] ✅ Todos los tests pasan
- [ ] ✅ Flujos end-to-end funcionan
- [x] ✅ No hay referencias a código antiguo (verificar con grep checklist)
- [x] ✅ Compila sin errores TypeScript
- [x] ✅ Definition of Done (sección 14) completo
- [x] ✅ No hay código antiguo en el repo

---

### Fase 2: Cleanup y Documentación (Opcional)

**Objetivo:** Limpiar y documentar

**Cambios:**
- [ ] Actualizar documentación interna
- [ ] Agregar comentarios JSDoc a funciones actualizadas
- [ ] Verificar que no queden tipos obsoletos
- [ ] Limpiar imports no usados

**Verificación:**
- ✅ Linter sin warnings
- ✅ Documentación actualizada

---

## 18. Referencias

- Backend Endpoints: `relevo-api/src/Relevo.Web/`
- Frontend API: `relevo-frontend/src/api/`
- Tipos Frontend: `relevo-frontend/src/api/types.ts`
- Modelos Backend: `relevo-api/src/Relevo.Core/Models/`

---

**Última actualización:** 2024
**Versión Backend:** V3
**Versión Frontend:** V2 (requiere actualización)

