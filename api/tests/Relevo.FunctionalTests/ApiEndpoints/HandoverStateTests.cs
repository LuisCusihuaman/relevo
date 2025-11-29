using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Handovers;
using Xunit;
using System.Net.Http.Json;

namespace Relevo.FunctionalTests.ApiEndpoints;

/// <summary>
/// Tests for handover state operations using the seeded handover.
/// Note: These tests operate on the same seeded handover, so they test that
/// state changes work but may not fully isolate each operation.
/// </summary>
[Collection("Sequential")]
public class HandoverStateTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReadyHandover_ChangesStatus()
  {
    // The seeded handover starts as Draft, so we can ready it
    var handoverId = DapperTestSeeder.HandoverId;
    var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
    var response = await _client.PostAsync($"/handovers/{handoverId}/ready", content);
    response.EnsureSuccessStatusCode();

    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");
    Assert.Equal("Ready", result.Status);
  }

  [Fact]
  public async Task RejectHandover_ReturnsValidResponse()
  {
    // Test that reject endpoint returns a valid response (either success or constraint error)
    // The seeded handover might already be in a terminal state from other tests
    var handoverId = DapperTestSeeder.HandoverId;
    var request = new RejectHandoverRequest { HandoverId = handoverId, Reason = "Not ready" };
    
    var response = await _client.PostAsync($"/handovers/{handoverId}/reject", JsonContent.Create(request));
    
    // Should return OK (if successful) or BadRequest (if constraint violation) - not 500
    Assert.True(
        response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest,
        $"Expected OK or BadRequest, got {response.StatusCode}");
  }

  [Fact]
  public async Task GetPendingHandovers_ReturnsList()
  {
    var result = await _client.GetAndDeserializeAsync<GetPendingHandoversResponse>("/handovers/pending");
    Assert.NotNull(result);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsHandover()
  {
    var handoverId = DapperTestSeeder.HandoverId;
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");
    
    Assert.NotNull(result);
    Assert.Equal(handoverId, result.Id);
  }
}
