using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Xunit;
using System.Net.Http.Json;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversPutSituationAwareness(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task UpdatesSituationAwareness()
  {
    var request = new UpdateSituationAwarenessRequest
    {
        HandoverId = "hvo-001",
        Content = "Updated SA Content via API",
        Status = "Final"
    };
    
    // Ensure we are sending proper JSON
    var content = JsonContent.Create(new { Content = request.Content, Status = request.Status });

    var response = await _client.PutAsync($"/handovers/{request.HandoverId}/situation-awareness", content);
    response.EnsureSuccessStatusCode();
    
    // Verify update
    var result = await _client.GetAndDeserializeAsync<GetSituationAwarenessResponse>($"/handovers/{request.HandoverId}/situation-awareness");
    Assert.NotNull(result.SituationAwareness);
    
    Assert.Equal("Updated SA Content via API", result.SituationAwareness.Content);
    Assert.Equal("Final", result.SituationAwareness.Status);
  }
}
