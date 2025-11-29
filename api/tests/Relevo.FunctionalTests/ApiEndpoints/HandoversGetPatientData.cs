using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
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
    var handoverId = DapperTestSeeder.HandoverId;
    var patientId = DapperTestSeeder.PatientId1;
    
    var result = await _client.GetAndDeserializeAsync<GetPatientHandoverDataResponse>($"/handovers/{handoverId}/patient");

    Assert.NotNull(result);
    Assert.Equal(patientId, result.id);
    Assert.NotNull(result.illnessSeverity); // Value may change due to other tests
    
    Assert.NotNull(result.assignedPhysician);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenInvalidHandoverId()
  {
    await _client.GetAndEnsureNotFoundAsync("/handovers/invalid-id/patient");
  }
}
