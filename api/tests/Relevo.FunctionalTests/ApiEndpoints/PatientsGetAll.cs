using Ardalis.HttpClientTestExtensions;
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
    var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>("/patients");

    Assert.NotNull(result);
    Assert.NotEmpty(result.Items);
    Assert.Contains(result.Items, p => p.Id == "pat-001");
    Assert.Contains(result.Items, p => p.Id == "pat-002");
    Assert.Equal(2, result.Pagination.TotalItems);
  }
}

