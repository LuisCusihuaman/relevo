using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Patients;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsGetById(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsPatientGivenValidId()
  {
    var result = await _client.GetAndDeserializeAsync<GetPatientByIdResponse>("/patients/pat-001");

    Assert.NotNull(result);
    Assert.Equal("pat-001", result.Id);
    Assert.Equal("María García", result.Name);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenInvalidId()
  {
    await _client.GetAndEnsureNotFoundAsync("/patients/invalid-id");
  }
}

