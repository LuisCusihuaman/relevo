using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Patients;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsGetActionItems(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task ReturnsActionItemsForPatient()
  {
    var patientId = DapperTestSeeder.PatientId1;
    var handoverId = DapperTestSeeder.HandoverId;
    
    var result = await _client.GetAndDeserializeAsync<GetPatientActionItemsResponse>($"/patients/{patientId}/action-items");

    Assert.NotNull(result);
    Assert.NotEmpty(result.ActionItems);
    Assert.Contains(result.ActionItems, i => i.Description == "Check blood pressure");
    Assert.Contains(result.ActionItems, i => i.HandoverId == handoverId);
  }
}
