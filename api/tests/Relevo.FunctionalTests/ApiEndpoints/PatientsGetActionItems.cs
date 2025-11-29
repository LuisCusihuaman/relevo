using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Patients;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsGetActionItems(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsActionItemsForPatient()
  {
    var result = await _client.GetAndDeserializeAsync<GetPatientActionItemsResponse>("/patients/pat-001/action-items");

    Assert.NotNull(result);
    Assert.NotEmpty(result.ActionItems);
    Assert.Contains(result.ActionItems, i => i.Description == "Check blood pressure");
    Assert.Contains(result.ActionItems, i => i.HandoverId == "hvo-001");
  }
}

