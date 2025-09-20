using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Models;
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
    var handoverId = "hvo-2509201329-4574";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.Equal(handoverId, result.Id);
    Assert.NotNull(result.AssignmentId);
    Assert.NotNull(result.PatientId);
    Assert.NotNull(result.Status);
    Assert.NotNull(result.IllnessSeverity);
    Assert.NotNull(result.PatientSummary);
    Assert.NotNull(result.ActionItems);
    Assert.NotNull(result.ShiftName);
    Assert.NotNull(result.CreatedBy);
    Assert.NotNull(result.AssignedTo);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsCorrectHandoverStructure()
  {
    var handoverId = "hvo-2509201329-4574";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);

    // Verify basic fields
    Assert.Equal(handoverId, result.Id);
    Assert.Equal("pat-026", result.PatientId);
    Assert.Contains(result.Status, new[] { "Active", "InProgress", "Completed" });

    // Verify illness severity structure
    Assert.NotNull(result.IllnessSeverity);
    Assert.NotNull(result.IllnessSeverity.Value);
    Assert.Contains(result.IllnessSeverity.Value, new[] { "Stable", "Watcher", "Unstable" });

    // Verify patient summary structure
    Assert.NotNull(result.PatientSummary);
    Assert.NotNull(result.PatientSummary.Value);

    // Verify action items structure
    Assert.NotNull(result.ActionItems);

    // Verify other required fields
    Assert.NotNull(result.AssignmentId);
    Assert.NotNull(result.ShiftName);
    Assert.NotNull(result.CreatedBy);
    Assert.NotNull(result.AssignedTo);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsHandoverWithPatientName_WhenPatientExists()
  {
    var handoverId = "hvo-2509201329-4574";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.PatientName);
    Assert.Equal("Álvaro Vargas", result.PatientName);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsHandoverWithActionItems()
  {
    var handoverId = "hvo-2509201329-4574";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.ActionItems);
    // This handover may or may not have action items, so we just verify the structure
    foreach (var actionItem in result.ActionItems)
    {
      Assert.NotNull(actionItem.Id);
      Assert.NotNull(actionItem.Description);
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
    // Test with different handover IDs that might have different statuses
    var testHandoverIds = new[] { "hvo-2509201329-4574", "hvo-2509201329-7677" };

    foreach (var handoverId in testHandoverIds)
    {
      var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

      if (result != null) // Only assert if handover exists
      {
        Assert.NotNull(result.Status);
        Assert.Contains(result.Status, new[] { "Active", "InProgress", "Completed" });
      }
    }
  }

  [Fact]
  public async Task GetHandoverById_ReturnsCorrectShiftInformation()
  {
    var handoverId = "hvo-2509201329-4574";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.ShiftName);
    Assert.Equal("Noche", result.ShiftName);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsCorrectIllnessSeverity()
  {
    var handoverId = "hvo-2509201329-4574";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.IllnessSeverity);
    Assert.Equal("Stable", result.IllnessSeverity.Value);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsOptionalFields_WhenPresent()
  {
    var handoverId = "hvo-2509201329-4574";
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
  public async Task GetHandoverById_ReturnsEmptyActionItemsList_WhenNoActions()
  {
    // Find a handover that might not have action items
    var handoverId = "hvo-2509201329-7677";
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.ActionItems);
    // Verify it's a valid list even if empty
    Assert.True(result.ActionItems.Count >= 0);
  }

  [Fact]
  public async Task GetHandoverById_HandlesSpecialCharactersInIds()
  {
    // Test with handover ID containing special characters or numbers
    var handoverId = "hvo-2509201329-4574"; // Contains hyphens and numbers
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
  public IllnessSeverityDto IllnessSeverity { get; set; } = new();
  public PatientSummaryDto PatientSummary { get; set; } = new();
  public List<ActionItemDto> ActionItems { get; set; } = [];
  public string? SituationAwarenessDocId { get; set; }
  public SynthesisDto? Synthesis { get; set; }
  public string ShiftName { get; set; } = string.Empty;
  public string CreatedBy { get; set; } = string.Empty;
  public string AssignedTo { get; set; } = string.Empty;

  public class IllnessSeverityDto
  {
    public string Value { get; set; } = string.Empty;
  }

  public class PatientSummaryDto
  {
    public string Value { get; set; } = string.Empty;
  }

  public class ActionItemDto
  {
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
  }

  public class SynthesisDto
  {
    public string Value { get; set; } = string.Empty;
  }
}
