using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Patients;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsGetById(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task ReturnsPatientGivenValidId()
  {
    var patientId = DapperTestSeeder.PatientId1;
    
    var result = await _client.GetAndDeserializeAsync<GetPatientByIdResponse>($"/patients/{patientId}");

    Assert.NotNull(result);
    Assert.Equal(patientId, result.Id);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenInvalidId()
  {
    await _client.GetAndEnsureNotFoundAsync("/patients/invalid-id");
  }
}
