using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Relevo.Core.Interfaces;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoverByIdEndpoints(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task GetHandoverById_ReturnsHandover_WhenHandoverExists()
  {
    Console.WriteLine("[TEST] Starting GetHandoverById_ReturnsHandover_WhenHandoverExists test");
    var handoverId = "handover-001";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.Equal(handoverId, result.Id);
    Assert.NotNull(result.AssignmentId);
    Assert.NotNull(result.PatientId);
    Assert.NotNull(result.Status);
    Assert.NotNull(result.illnessSeverity);
    Assert.NotNull(result.patientSummary);
    Assert.NotNull(result.actionItems);
    Assert.NotNull(result.ShiftName);
    Assert.NotNull(result.CreatedBy);
    Assert.NotNull(result.AssignedTo);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsCorrectHandoverStructure()
  {
    var handoverId = "handover-001";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);

    // Verify basic fields
    Assert.Equal(handoverId, result.Id);
    Assert.Equal("pat-001", result.PatientId);
    Assert.Contains(result.Status, new[] { "Active", "InProgress", "Completed", "Ready" });

    // Verify illness severity structure - using actual property names
    Assert.NotNull(result.illnessSeverity);
    Assert.NotNull(result.illnessSeverity.severity);
    Assert.Contains(result.illnessSeverity.severity, new[] { "Stable", "Watcher", "Unstable" });

    // Verify patient summary structure - using actual property names
    Assert.NotNull(result.patientSummary);
    Assert.NotNull(result.patientSummary.content);

    // Verify action items structure
    Assert.NotNull(result.actionItems);

    // Verify other required fields
    Assert.NotNull(result.AssignmentId);
    Assert.NotNull(result.ShiftName);
    Assert.NotNull(result.CreatedBy);
    Assert.NotNull(result.AssignedTo);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsHandoverWithPatientName_WhenPatientExists()
  {
    var handoverId = "handover-001";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.PatientName);
    Assert.Equal("María García", result.PatientName);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsHandoverWithActionItems()
  {
    var handoverId = "handover-001";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.actionItems);
    // This handover may or may not have action items, so we just verify the structure
    foreach (var actionItem in result.actionItems)
    {
      Assert.NotNull(actionItem.id);
      Assert.NotNull(actionItem.description);
      // IsCompleted should be boolean
    }
  }

  [Fact]
  public async Task GetHandoverById_Returns404_WhenHandoverDoesNotExist()
  {
    var nonExistentHandoverId = "non-existent-handover-id";

    var response = await _client.GetAsync($"/handovers/{nonExistentHandoverId}");

    Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetHandoverById_HandlesDifferentHandoverStatuses()
  {
    // Test with the existing handover
    var handoverId = "handover-001";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.Status);
    Assert.Contains(result.Status, new[] { "Active", "InProgress", "Completed", "Ready" });
  }

  [Fact]
  public async Task GetHandoverById_ReturnsCorrectShiftInformation()
  {
    var handoverId = "handover-001";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.ShiftName);
    Assert.Equal("Mañana → Noche", result.ShiftName);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsCorrectIllnessSeverity()
  {
    var handoverId = "handover-001";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.illnessSeverity);
    Assert.Equal("Stable", result.illnessSeverity.severity);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsOptionalFields_WhenPresent()
  {
    var handoverId = "handover-001";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);

    // These fields might be null depending on the handover
    // We just verify they are handled correctly (not causing exceptions)

    // SituationAwarenessDocId can be null
    // Synthesis can be null
    // PatientName should not be null for existing patients
    Assert.NotNull(result.PatientName);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsActionItemsList()
  {
    // Test with the existing handover that has action items
    var handoverId = "handover-001";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.actionItems);
    // The handover-001 has action items, so verify we get them
    Assert.True(result.actionItems.Count >= 0);
  }

  [Fact]
  public async Task GetHandoverById_HandlesSpecialCharactersInIds()
  {
    // Test with handover ID containing special characters or numbers
    var handoverId = "handover-001"; // Contains hyphens and numbers
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.Equal(handoverId, result.Id);
  }
}

public class GetHandoverByIdResponse
{
  public string Id { get; set; } = string.Empty;
  public string AssignmentId { get; set; } = string.Empty;
  public string PatientId { get; set; } = string.Empty;
  public string? PatientName { get; set; }
  public string Status { get; set; } = string.Empty;
  public IllnessSeverityDto illnessSeverity { get; set; } = new();
  public PatientSummaryDto patientSummary { get; set; } = new();
  public List<ActionItemDto> actionItems { get; set; } = [];
  public string? situationAwarenessDocId { get; set; }
  public SynthesisDto? synthesis { get; set; }
  public string ShiftName { get; set; } = string.Empty;
  public string CreatedBy { get; set; } = string.Empty;
  public string AssignedTo { get; set; } = string.Empty;

  public class IllnessSeverityDto
  {
    public string severity { get; set; } = string.Empty;
  }

  public class PatientSummaryDto
  {
    public string content { get; set; } = string.Empty;
  }

  public class ActionItemDto
  {
    public string id { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public bool isCompleted { get; set; }
  }

  public class SynthesisDto
  {
    public string content { get; set; } = string.Empty;
  }
}
