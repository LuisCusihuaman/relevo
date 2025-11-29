using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
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
    var handoverId = DapperTestSeeder.HandoverId;
    var result = await _client.GetAndDeserializeAsync<GetSituationAwarenessResponse>($"/handovers/{handoverId}/situation-awareness");

    Assert.NotNull(result);
    Assert.NotNull(result.SituationAwareness);
    Assert.Equal(handoverId, result.SituationAwareness.HandoverId);
  }
}
