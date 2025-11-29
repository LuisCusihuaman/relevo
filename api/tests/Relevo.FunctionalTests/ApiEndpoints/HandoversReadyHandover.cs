using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Handovers;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversReadyHandover(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task MarksHandoverAsReady()
  {
    var handoverId = DapperTestSeeder.HandoverId;
    
    var response = await _client.PostAsync($"/handovers/{handoverId}/ready", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
    response.EnsureSuccessStatusCode();

    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");
    Assert.NotNull(result);
    Assert.Equal("Ready", result.Status);
  }
}
