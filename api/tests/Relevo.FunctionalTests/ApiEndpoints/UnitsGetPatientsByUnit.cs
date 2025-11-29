using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Units;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class UnitsGetPatientsByUnit(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsPatientsGivenUnitId1()
  {
    var result = await _client.GetAndDeserializeAsync<GetPatientsByUnitResponse>("/units/unit-1/patients");

    Assert.NotNull(result);
    Assert.NotEmpty(result.Patients);
    Assert.Contains(result.Patients, p => p.Id == "pat-001");
    Assert.Equal(2, result.TotalCount); // Seeded 2 patients for unit-1
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

