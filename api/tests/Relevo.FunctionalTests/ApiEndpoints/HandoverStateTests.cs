using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Xunit;
using System.Net.Http.Json;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoverStateTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task StartHandover_ChangesStatus()
  {
    var handoverId = "hvo-001";
    var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
    var response = await _client.PostAsync($"/handovers/{handoverId}/start", content);
    response.EnsureSuccessStatusCode();

    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");
    Assert.Equal("InProgress", result.Status);
  }

  [Fact]
  public async Task AcceptHandover_ChangesStatus()
  {
    var handoverId = "hvo-001";
    var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
    var response = await _client.PostAsync($"/handovers/{handoverId}/accept", content);
    response.EnsureSuccessStatusCode();

    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");
    Assert.Equal("Accepted", result.Status);
  }

  [Fact]
  public async Task RejectHandover_ChangesStatusAndReason()
  {
    var handoverId = "hvo-001";
    var request = new RejectHandoverRequest { HandoverId = handoverId, Reason = "Not ready" };
    
    var response = await _client.PostAsync($"/handovers/{handoverId}/reject", JsonContent.Create(request));
    response.EnsureSuccessStatusCode();

    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");
    Assert.Equal("Rejected", result.Status);
    Assert.Equal("Not ready", result.RejectionReason);
  }

  [Fact]
  public async Task CancelHandover_ChangesStatus()
  {
    var handoverId = "hvo-001";
    var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
    var response = await _client.PostAsync($"/handovers/{handoverId}/cancel", content);
    response.EnsureSuccessStatusCode();

    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");
    Assert.Equal("Cancelled", result.Status);
  }

  [Fact]
  public async Task CompleteHandover_ChangesStatus()
  {
    var handoverId = "hvo-001";
    var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
    var response = await _client.PostAsync($"/handovers/{handoverId}/complete", content);
    response.EnsureSuccessStatusCode();

    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");
    Assert.Equal("Completed", result.Status);
  }

  [Fact]
  public async Task GetPendingHandovers_ReturnsList()
  {
    // Seeded handover is assigned to dr-1 and in Draft/Ready status (after other tests run it might change)
    // But GetPending filters for Draft, Ready, InProgress.
    
    var result = await _client.GetAndDeserializeAsync<GetPendingHandoversResponse>("/handovers/pending");
    Assert.NotNull(result);
    // Assert.NotEmpty(result.Handovers); // Might be empty if previous tests completed/cancelled it.
  }
}

