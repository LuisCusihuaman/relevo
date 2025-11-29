using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Xunit;
using System.Net;
using System.Net.Http.Json;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversPutSynthesis(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task UpdatesSynthesis()
  {
    var request = new PutSynthesisRequest
    {
        HandoverId = "hvo-001",
        Content = "Functional test synthesis",
        Status = "draft"
    };

    var response = await _client.PutAsJsonAsync("/handovers/hvo-001/synthesis", request);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<PutSynthesisResponse>();
    Assert.NotNull(result);
    Assert.True(result.Success);
  }
}

