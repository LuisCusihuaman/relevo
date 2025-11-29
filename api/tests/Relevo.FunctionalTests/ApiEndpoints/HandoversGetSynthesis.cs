using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversGetSynthesis(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsSynthesisForHandover()
  {
    var result = await _client.GetAndDeserializeAsync<GetSynthesisResponse>("/handovers/hvo-001/synthesis");

    Assert.NotNull(result);
    Assert.NotNull(result.Synthesis);
    Assert.Equal("hvo-001", result.Synthesis.HandoverId);
  }
}

