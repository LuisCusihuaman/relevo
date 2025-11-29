using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversReadyHandover(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task MarksHandoverAsReady()
  {
    var handoverId = "hvo-001";
    
    // Send empty JSON content to ensure Content-Type header is set
    var response = await _client.PostAsync($"/handovers/{handoverId}/ready", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
    response.EnsureSuccessStatusCode();

    // Verify status change
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");
    Assert.NotNull(result);
    Assert.Equal("Ready", result.Status);
  }
}

