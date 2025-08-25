using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Units;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class UnitsPatientsEndpoints(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task GetPatientsByUnit_ReturnsPagedPatients()
  {
    var route = "/units/unit-1/patients?page=1&pageSize=2";
    var result = await _client.GetAndDeserializeAsync<GetPatientsByUnitResponse>(route);
    Assert.NotNull(result);
    Assert.True(result.TotalCount >= 2);
    Assert.Equal(2, result.Patients.Count);
  }
}


