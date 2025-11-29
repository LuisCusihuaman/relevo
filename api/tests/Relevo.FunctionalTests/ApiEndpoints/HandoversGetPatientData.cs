using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversGetPatientData(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsPatientDataGivenValidHandoverId()
  {
    var result = await _client.GetAndDeserializeAsync<GetPatientHandoverDataResponse>("/handovers/hvo-001/patient");

    Assert.NotNull(result);
    Assert.Equal("pat-001", result.id);
    Assert.Equal("María García", result.name);
    Assert.Equal("UCI", result.unit);
    Assert.Equal("Stable", result.illnessSeverity);
    
    Assert.NotNull(result.assignedPhysician);
    Assert.Equal("Dr. One", result.assignedPhysician.name);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenInvalidHandoverId()
  {
    await _client.GetAndEnsureNotFoundAsync("/handovers/invalid-id/patient");
  }
}

