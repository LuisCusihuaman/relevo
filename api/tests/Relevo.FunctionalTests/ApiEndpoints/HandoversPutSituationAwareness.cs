using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Handovers;
using Xunit;
using System.Net.Http.Json;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversPutSituationAwareness(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task UpdatesSituationAwareness()
  {
    var handoverId = DapperTestSeeder.HandoverId;
    var request = new UpdateSituationAwarenessRequest
    {
        HandoverId = handoverId,
        Content = "Updated SA Content via API",
        Status = "Final"
    };
    
    var content = JsonContent.Create(new { Content = request.Content, Status = request.Status });

    var response = await _client.PutAsync($"/handovers/{request.HandoverId}/situation-awareness", content);
    response.EnsureSuccessStatusCode();
    
    var result = await _client.GetAndDeserializeAsync<GetSituationAwarenessResponse>($"/handovers/{request.HandoverId}/situation-awareness");
    Assert.NotNull(result.SituationAwareness);
    
    Assert.Equal("Updated SA Content via API", result.SituationAwareness.Content);
    Assert.Equal("Final", result.SituationAwareness.Status);
  }
}
