using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Patients;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsGetAll(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsAllPatients()
  {
    var patientId1 = DapperTestSeeder.PatientId1;
    var patientId2 = DapperTestSeeder.PatientId2;
    
    var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>("/patients");

    Assert.NotNull(result);
    Assert.NotEmpty(result.Items);
    Assert.Contains(result.Items, p => p.Id == patientId1);
    Assert.Contains(result.Items, p => p.Id == patientId2);
  }
}
