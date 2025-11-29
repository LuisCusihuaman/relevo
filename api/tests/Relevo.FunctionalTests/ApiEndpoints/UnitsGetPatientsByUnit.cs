using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Units;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class UnitsGetPatientsByUnit(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsPatientsGivenUnitId()
  {
    var unitId = DapperTestSeeder.UnitId;
    var patientId1 = DapperTestSeeder.PatientId1;
    var patientId2 = DapperTestSeeder.PatientId2;
    
    var result = await _client.GetAndDeserializeAsync<GetPatientsByUnitResponse>($"/units/{unitId}/patients");

    Assert.NotNull(result);
    Assert.NotEmpty(result.Patients);
    Assert.Contains(result.Patients, p => p.Id == patientId1);
    Assert.Contains(result.Patients, p => p.Id == patientId2);
    Assert.True(result.TotalCount >= 2, "Expected at least 2 seeded patients for this unit");
  }

  [Fact]
  public async Task ReturnsEmptyGivenInvalidUnitId()
  {
    var result = await _client.GetAndDeserializeAsync<GetPatientsByUnitResponse>("/units/invalid-unit/patients");

    Assert.NotNull(result);
    Assert.Empty(result.Patients);
    Assert.Equal(0, result.TotalCount);
  }
}
