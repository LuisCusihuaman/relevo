using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Patients;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsGetHandovers(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task ReturnsHandoversForPatient()
  {
    var patientId = DapperTestSeeder.PatientId1;
    
    var result = await _client.GetAndDeserializeAsync<GetPatientHandoversResponse>($"/patients/{patientId}/handovers");

    Assert.NotNull(result);
    Assert.NotEmpty(result.Items); // Handover is seeded for this patient
  }
}
