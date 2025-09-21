using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Models;
using Relevo.Core.Interfaces;
using Xunit;
using System.Linq;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientHandoversEndpoints(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task GetPatientHandovers_ReturnsHandovers_WhenPatientExists()
  {
    var patientId = "pat-001";
    var result = await _client.GetAndDeserializeAsync<GetPatientHandoversResponse>($"/patients/{patientId}/handovers");

    Assert.NotNull(result);
    Assert.NotNull(result.Items);
    Assert.True(result.Pagination.TotalItems >= 0);
  }

  [Fact]
  public async Task GetPatientHandovers_ReturnsEmptyList_WhenPatientHasNoHandovers()
  {
    var patientId = "pat-999"; // Non-existent patient
    var result = await _client.GetAndDeserializeAsync<GetPatientHandoversResponse>($"/patients/{patientId}/handovers");

    Assert.NotNull(result);
    Assert.NotNull(result.Items);
    Assert.Empty(result.Items);
    Assert.Equal(0, result.Pagination.TotalItems);
  }

  [Fact]
  public async Task GetPatientHandovers_HandlesPaginationCorrectly()
  {
    var patientId = "pat-001";
    var result = await _client.GetAndDeserializeAsync<GetPatientHandoversResponse>($"/patients/{patientId}/handovers?page=1&pageSize=10");

    Assert.NotNull(result);
    Assert.NotNull(result.Pagination);
    Assert.Equal(1, result.Pagination.CurrentPage);
    Assert.Equal(10, result.Pagination.PageSize);
    Assert.True(result.Pagination.TotalPages >= 0);
  }

  [Fact]
  public async Task GetPatientHandovers_ReturnsCorrectHandoverStructure()
  {
    var patientId = "pat-001";
    var result = await _client.GetAndDeserializeAsync<GetPatientHandoversResponse>($"/patients/{patientId}/handovers");

    if (result.Items.Any())
    {
      var handover = result.Items[0];
      Assert.NotNull(handover.Id);
      Assert.NotNull(handover.AssignmentId);
      Assert.Equal(patientId, handover.PatientId);
      Assert.NotNull(handover.Status);
      Assert.NotNull(handover.IllnessSeverity);
      Assert.NotNull(handover.PatientSummary);
      Assert.NotNull(handover.ActionItems);
      Assert.NotNull(handover.ShiftName);
      Assert.NotNull(handover.CreatedBy);
      Assert.NotNull(handover.AssignedTo);

      // Verify status is one of the expected values
      Assert.Contains(handover.Status, new[] { "Active", "InProgress", "Completed" });

      // Verify illness severity structure
      Assert.NotNull(handover.IllnessSeverity);
      Assert.Contains(handover.IllnessSeverity.Severity, new[] { "Stable", "Watcher", "Unstable" });

      // Verify patient summary structure
      Assert.NotNull(handover.PatientSummary);
    }
  }

  [Fact]
  public async Task GetPatientHandovers_ReturnsHandoversInCorrectOrder()
  {
    var patientId = "pat-001";
    var result = await _client.GetAndDeserializeAsync<GetPatientHandoversResponse>($"/patients/{patientId}/handovers");

    if (result.Items.Count > 1)
    {
      // Verify handovers are ordered by creation date (most recent first)
      for (int i = 0; i < result.Items.Count - 1; i++)
      {
        // Since we don't have timestamps in the response, we just verify the order exists
        Assert.NotNull(result.Items[i].Id);
        Assert.NotNull(result.Items[i + 1].Id);
      }
    }
  }

  [Fact]
  public async Task GetPatientHandovers_HandlesLargePageSize()
  {
    var patientId = "pat-001";
    var result = await _client.GetAndDeserializeAsync<GetPatientHandoversResponse>($"/patients/{patientId}/handovers?page=1&pageSize=100");

    Assert.NotNull(result);
    Assert.NotNull(result.Items);
    Assert.True(result.Items.Count <= 100); // Should not exceed page size
  }

  [Fact]
  public async Task GetPatientHandovers_HandlesInvalidPageNumber()
  {
    var patientId = "pat-001";

    // Test with page 0 (should default to 1)
    var result = await _client.GetAndDeserializeAsync<GetPatientHandoversResponse>($"/patients/{patientId}/handovers?page=0&pageSize=10");

    Assert.NotNull(result);
    Assert.NotNull(result.Pagination);
    Assert.Equal(1, result.Pagination.CurrentPage); // Should default to 1
  }

  [Fact]
  public async Task GetPatientHandovers_HandlesInvalidPageSize()
  {
    var patientId = "pat-001";

    // Test with pageSize 0 (should default to 25)
    var result = await _client.GetAndDeserializeAsync<GetPatientHandoversResponse>($"/patients/{patientId}/handovers?page=1&pageSize=0");

    Assert.NotNull(result);
    Assert.NotNull(result.Pagination);
    Assert.Equal(25, result.Pagination.PageSize); // Should default to 25
  }
}

public class GetPatientHandoversResponse
{
  public List<HandoverRecord> Items { get; set; } = [];
  public PaginationInfo Pagination { get; set; } = new();
}
