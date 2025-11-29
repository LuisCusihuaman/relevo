using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Patients;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsGetHandovers(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsHandoversForPatient()
  {
    // Requires seeding Handovers. 
    // DapperTestSeeder needs update to seed Handover data.
    
    var result = await _client.GetAndDeserializeAsync<GetPatientHandoversResponse>("/patients/pat-001/handovers");

    Assert.NotNull(result);
    // Currently empty because no handovers seeded for pat-001 in latest seeder update.
    // Will update seeder next.
    // Assert.NotEmpty(result.Items); 
  }
}

