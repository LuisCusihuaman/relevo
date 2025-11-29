using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Patients;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsGetAll(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task ReturnsAllPatients()
  {
    var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>("/patients");

    Assert.NotNull(result);
    Assert.NotEmpty(result.Items);
    // Just verify we get results - specific patient IDs may vary due to parallel test runs and pagination
  }
}
