using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversGetSituationAwareness(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsSituationAwarenessForHandover()
  {
    var result = await _client.GetAndDeserializeAsync<GetSituationAwarenessResponse>("/handovers/hvo-001/situation-awareness");

    Assert.NotNull(result);
    Assert.NotNull(result.SituationAwareness);
    Assert.Equal("hvo-001", result.SituationAwareness.HandoverId);
  }
}

