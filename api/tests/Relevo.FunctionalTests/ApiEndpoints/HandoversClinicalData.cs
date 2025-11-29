using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Xunit;
using System.Net.Http.Json;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversClinicalData(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task GetReturnsClinicalData()
  {
    var handoverId = "hvo-001";
    var result = await _client.GetAndDeserializeAsync<GetClinicalDataResponse>($"/handovers/{handoverId}/patient-data");

    Assert.NotNull(result);
    Assert.Equal(handoverId, result.HandoverId);
    Assert.Equal("Stable", result.IllnessSeverity);
  }

  [Fact]
  public async Task PutUpdatesClinicalData()
  {
    var request = new UpdateClinicalDataRequest
    {
        HandoverId = "hvo-001",
        IllnessSeverity = "Critical",
        SummaryText = "Updated Summary via API"
    };

    // Using JsonContent to ensure correct serialization
    var content = JsonContent.Create(new { IllnessSeverity = request.IllnessSeverity, SummaryText = request.SummaryText });
    var response = await _client.PutAsync($"/handovers/{request.HandoverId}/patient-data", content);
    
    response.EnsureSuccessStatusCode();

    var result = await _client.GetAndDeserializeAsync<GetClinicalDataResponse>($"/handovers/{request.HandoverId}/patient-data");
    Assert.Equal("Critical", result.IllnessSeverity);
    Assert.Equal("Updated Summary via API", result.SummaryText);
  }
}

