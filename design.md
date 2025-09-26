# Design Document

## Overview

This design implements a migration from the current generic handover sections system to a normalized, type-safe architecture. The solution maintains backward compatibility during migration while establishing clear separation between singleton data (illness severity, patient summary, situation awareness, synthesis) and collection data (action items, contingency plans, messages).

The design follows a phased approach: Phase 0 (read paths), Phase 1 (backfill & dual-write), Phase 2 (UI cutover), and Phase 3 (legacy cleanup).

## Architecture

### Current State Analysis

**Database Layer:**
- `HANDOVER_SECTIONS(ID, HANDOVER_ID, SECTION_TYPE, CONTENT CLOB, STATUS, ...)`
- Duplicate data in `HANDOVERS` table (`ILLNESS_SEVERITY`, `PATIENT_SUMMARY`, `SYNTHESIS`)
- Collection tables already normalized: `HANDOVER_ACTION_ITEMS`, `PATIENT_SUMMARIES`

**API Layer:**
- Generic endpoint: `PUT /me/handovers/{handoverId}/sections/{sectionId}`
- Collection endpoints already specific: `PUT /action-items/{actionItemId}`
- State transition endpoints: `/ready`, `/start`, `/accept`, `/complete`, `/cancel`, `/reject`

**Frontend Layer:**
- Generic `getSectionByType(sections, sectionType)` helper
- Plural patterns for collections already implemented
- React Query caching by generic "sections" key

### Target Architecture

**Database Schema:**
```sql
-- Singleton Tables (1:1 with HANDOVER_ID)
HANDOVER_PATIENT_DATA(
    HANDOVER_ID PK/FK,
    ILLNESS_SEVERITY VARCHAR2(50),
    SUMMARY_TEXT CLOB,
    STATUS VARCHAR2(20),
    LAST_EDITED_BY VARCHAR2(100),
    UPDATED_AT TIMESTAMP,
    CREATED_AT TIMESTAMP
);

HANDOVER_SITUATION_AWARENESS(
    HANDOVER_ID PK/FK,
    CONTENT CLOB,
    STATUS VARCHAR2(20),
    LAST_EDITED_BY VARCHAR2(100),
    UPDATED_AT TIMESTAMP,
    CREATED_AT TIMESTAMP
);

HANDOVER_SYNTHESIS(
    HANDOVER_ID PK/FK,
    CONTENT CLOB,
    STATUS VARCHAR2(20),
    LAST_EDITED_BY VARCHAR2(100),
    UPDATED_AT TIMESTAMP,
    CREATED_AT TIMESTAMP
);
```

**API Endpoints:**
```
Singletons (GET|PUT):
- /api/handovers/{id}/patient-data
- /api/handovers/{id}/situation-awareness  
- /api/handovers/{id}/synthesis

Collections (existing, unchanged):
- /api/handovers/{id}/action-items
- /api/handovers/{id}/contingency-plans
- /api/handovers/{id}/messages

Legacy (temporary):
- /me/handovers/{id}/sections/{sectionId} (dual-write during migration)
```

## Components and Interfaces

### Repository Layer

**New Interfaces:**
```csharp
public interface IHandoverSingletonRepository
{
    Task<HandoverPatientData?> GetPatientDataAsync(string handoverId);
    Task<bool> UpsertPatientDataAsync(string handoverId, HandoverPatientData data, string userId);
    
    Task<HandoverSituationAwareness?> GetSituationAwarenessAsync(string handoverId);
    Task<bool> UpsertSituationAwarenessAsync(string handoverId, HandoverSituationAwareness data, string userId);
    
    Task<HandoverSynthesis?> GetSynthesisAsync(string handoverId);
    Task<bool> UpsertSynthesisAsync(string handoverId, HandoverSynthesis data, string userId);
    
    Task<bool> IsHandoverMutableAsync(string handoverId);
}
```

**Service Layer Updates:**
```csharp
public interface ISetupService
{
    // New singleton methods
    Task<HandoverPatientData?> GetPatientDataAsync(string handoverId);
    Task<bool> UpdatePatientDataAsync(string handoverId, HandoverPatientData data, string userId);
    
    Task<HandoverSituationAwareness?> GetSituationAwarenessAsync(string handoverId);
    Task<bool> UpdateSituationAwarenessAsync(string handoverId, HandoverSituationAwareness data, string userId);
    
    Task<HandoverSynthesis?> GetSynthesisAsync(string handoverId);
    Task<bool> UpdateSynthesisAsync(string handoverId, HandoverSynthesis data, string userId);
    
    // Existing methods remain unchanged
    Task<bool> UpdateHandoverSectionAsync(...); // Will dual-write during migration
}
```

### API Layer

**New Endpoint Controllers:**
```csharp
// GET /api/handovers/{handoverId}/patient-data
public class GetPatientDataEndpoint : Endpoint<GetPatientDataRequest, GetPatientDataResponse>
{
    public override async Task HandleAsync(GetPatientDataRequest req, CancellationToken ct)
    {
        var data = await _setupService.GetPatientDataAsync(req.HandoverId);
        await SendOkAsync(new GetPatientDataResponse { Data = data }, ct);
    }
}

// PUT /api/handovers/{handoverId}/patient-data  
public class UpdatePatientDataEndpoint : Endpoint<UpdatePatientDataRequest, UpdatePatientDataResponse>
{
    public override async Task HandleAsync(UpdatePatientDataRequest req, CancellationToken ct)
    {
        var success = await _setupService.UpdatePatientDataAsync(req.HandoverId, req.Data, req.UserId);
        await SendOkAsync(new UpdatePatientDataResponse { Success = success }, ct);
    }
}
```

**Legacy Endpoint Enhancement:**
```csharp
public class UpdateHandoverSectionEndpoint : Endpoint<UpdateHandoverSectionRequest, UpdateHandoverSectionResponse>
{
    public override async Task HandleAsync(UpdateHandoverSectionRequest req, CancellationToken ct)
    {
        // Dual-write logic during migration phase
        var legacySuccess = await _setupService.UpdateHandoverSectionAsync(req.HandoverId, req.SectionId, req.Content, req.Status, req.UserId);
        
        // Also write to new singleton tables based on section type
        await DualWriteToSingletonTable(req);
        
        await SendOkAsync(new UpdateHandoverSectionResponse { Success = legacySuccess }, ct);
    }
}
```

### Frontend Layer

**New Typed Hooks:**
```typescript
// Replace getSectionByType with specific hooks
export function useSituationAwareness(handoverId: string) {
  return useQuery({
    queryKey: ["handover", handoverId, "situation-awareness"],
    queryFn: () => getSituationAwareness(handoverId),
    enabled: !!handoverId
  });
}

export function usePutSituationAwareness() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ handoverId, data }: { handoverId: string; data: SituationAwarenessData }) =>
      updateSituationAwareness(handoverId, data),
    onSuccess: (_, { handoverId }) => {
      queryClient.invalidateQueries({ queryKey: ["handover", handoverId, "situation-awareness"] });
    }
  });
}

// Similar patterns for usePatientData, useSynthesis
```

**API Client Updates:**
```typescript
// New specific endpoints
export async function getSituationAwareness(handoverId: string): Promise<SituationAwarenessData> {
  const response = await api.get(`/api/handovers/${handoverId}/situation-awareness`);
  return response.data;
}

export async function updateSituationAwareness(
  handoverId: string, 
  data: UpdateSituationAwarenessRequest
): Promise<void> {
  await api.put(`/api/handovers/${handoverId}/situation-awareness`, data);
}
```

## Data Models

### Domain Models

```csharp
public class HandoverPatientData
{
    public string HandoverId { get; set; }
    public string IllnessSeverity { get; set; }
    public string SummaryText { get; set; }
    public string Status { get; set; }
    public string LastEditedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class HandoverSituationAwareness
{
    public string HandoverId { get; set; }
    public string Content { get; set; }
    public string Status { get; set; }
    public string LastEditedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class HandoverSynthesis
{
    public string HandoverId { get; set; }
    public string Content { get; set; }
    public string Status { get; set; }
    public string LastEditedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Request/Response DTOs

```csharp
public class GetPatientDataRequest
{
    public string HandoverId { get; set; }
}

public class GetPatientDataResponse
{
    public HandoverPatientData? Data { get; set; }
}

public class UpdatePatientDataRequest
{
    public string HandoverId { get; set; }
    public string IllnessSeverity { get; set; }
    public string SummaryText { get; set; }
    public string Status { get; set; }
    public string UserId { get; set; }
}
```

## Error Handling

### Immutability Enforcement

```csharp
public class HandoverImmutableException : Exception
{
    public HandoverImmutableException(string handoverId, string status) 
        : base($"Handover {handoverId} is {status} and cannot be modified") { }
}

// Repository implementation
public async Task<bool> UpsertPatientDataAsync(string handoverId, HandoverPatientData data, string userId)
{
    if (!await IsHandoverMutableAsync(handoverId))
    {
        var handover = await GetHandoverByIdAsync(handoverId);
        throw new HandoverImmutableException(handoverId, handover.Status);
    }
    
    // Proceed with update
}
```

### Migration Error Handling

```csharp
public class MigrationConflictException : Exception
{
    public MigrationConflictException(string handoverId, string sectionType, string reason)
        : base($"Migration conflict for handover {handoverId}, section {sectionType}: {reason}") { }
}

// Dual-write conflict resolution
private async Task DualWriteToSingletonTable(UpdateHandoverSectionRequest req)
{
    try
    {
        switch (req.SectionType?.ToLower())
        {
            case "situation_awareness":
                await _setupService.UpdateSituationAwarenessAsync(req.HandoverId, 
                    new HandoverSituationAwareness { Content = req.Content, Status = req.Status }, req.UserId);
                break;
            // Handle other section types
        }
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Dual-write failed for handover {HandoverId}, section {SectionType}", 
            req.HandoverId, req.SectionType);
        // Continue with legacy write, log for monitoring
    }
}
```

## Testing Strategy

### Unit Tests

```csharp
[Test]
public async Task UpdatePatientData_WhenHandoverCompleted_ThrowsImmutableException()
{
    // Arrange
    var handoverId = "completed-handover-123";
    var mockRepo = new Mock<IHandoverSingletonRepository>();
    mockRepo.Setup(r => r.IsHandoverMutableAsync(handoverId)).ReturnsAsync(false);
    
    // Act & Assert
    await Assert.ThrowsAsync<HandoverImmutableException>(() => 
        _service.UpdatePatientDataAsync(handoverId, new HandoverPatientData(), "user123"));
}

[Test]
public async Task GetPatientData_WhenDataExists_ReturnsTypedData()
{
    // Arrange
    var expectedData = new HandoverPatientData { IllnessSeverity = "Critical" };
    _mockRepo.Setup(r => r.GetPatientDataAsync("handover-123")).ReturnsAsync(expectedData);
    
    // Act
    var result = await _service.GetPatientDataAsync("handover-123");
    
    // Assert
    Assert.That(result.IllnessSeverity, Is.EqualTo("Critical"));
}
```

### Integration Tests

```csharp
[Test]
public async Task MigrationFlow_BackfillAndDualWrite_PreservesDataIntegrity()
{
    // Test the complete migration flow
    // 1. Create legacy data in HANDOVER_SECTIONS
    // 2. Run backfill migration
    // 3. Verify data in new singleton tables
    // 4. Test dual-write functionality
    // 5. Verify data consistency
}

[Test]
public async Task EndToEnd_NewEndpoints_WorkWithFrontend()
{
    // Test new API endpoints with actual HTTP calls
    var client = _factory.CreateClient();
    
    var response = await client.GetAsync("/api/handovers/test-123/patient-data");
    response.EnsureSuccessStatusCode();
    
    var data = await response.Content.ReadFromJsonAsync<GetPatientDataResponse>();
    Assert.That(data.Data, Is.Not.Null);
}
```

### Migration Testing

```csharp
[Test]
public async Task DataMigration_WithConflictingTimestamps_UsesLatestData()
{
    // Test migration logic when HANDOVER_SECTIONS has multiple entries
    // Verify that UPDATED_AT timestamp determines winner
}

[Test]
public async Task DualWrite_WhenNewTableFails_ContinuesWithLegacy()
{
    // Test resilience during dual-write phase
    // Ensure legacy functionality continues even if new tables have issues
}
```

## Migration Strategy

### Phase 0: Add Read Paths
1. Create new singleton tables with proper indexes
2. Implement repository methods for reading singleton data
3. Add GET endpoints for new singleton resources
4. Populate DTOs from current source of truth (HANDOVER_SECTIONS or HANDOVERS fields)

### Phase 1: Backfill & Dual-Write
1. Run one-time migration script to populate singleton tables from existing data
2. Update legacy PUT endpoint to dual-write to both old and new storage
3. Add monitoring and logging for dual-write conflicts
4. Verify data consistency between old and new storage

### Phase 2: UI Cutover
1. Replace `getSectionByType()` with typed hooks (`useSituationAwareness`, `usePatientData`, etc.)
2. Update React Query keys to be resource-scoped
3. Switch API calls from generic sections endpoint to specific singleton endpoints
4. Deploy frontend changes with feature flags for gradual rollout

### Phase 3: Legacy Cleanup
1. Stop writing to HANDOVER_SECTIONS table
2. Remove dual-write logic from legacy endpoint
3. Add deprecation warnings to `/sections/{sectionId}` endpoint
4. Remove duplicate content columns from HANDOVERS table
5. Drop HANDOVER_SECTIONS table after grace period

This design provides a robust, type-safe foundation for handover data management while ensuring zero-downtime migration and maintaining data integrity throughout the transition.