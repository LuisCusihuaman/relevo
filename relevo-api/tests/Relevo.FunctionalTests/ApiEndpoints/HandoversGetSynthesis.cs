using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Handovers;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversGetSynthesis(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task ReturnsSynthesisForHandover()
  {
    var handoverId = DapperTestSeeder.HandoverId;
    var result = await _client.GetAndDeserializeAsync<GetSynthesisResponse>($"/handovers/{handoverId}/synthesis");

    Assert.NotNull(result);
    Assert.NotNull(result.Synthesis);
    Assert.Equal(handoverId, result.Synthesis.HandoverId);
  }
}
