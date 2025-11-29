using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Patients;
using Xunit;
using System.Net;
using System.Net.Http.Json;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsCreateSummary(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task ReturnsCreatedSummary()
  {
    var patientId = DapperTestSeeder.PatientId1;
    var request = new CreatePatientSummaryRequest
    {
        PatientId = patientId,
        SummaryText = "Functional Test Summary"
    };

    var response = await _client.PostAsJsonAsync($"/patients/{patientId}/summary", request);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<CreatePatientSummaryResponse>();

    Assert.NotNull(result);
    Assert.NotNull(result.Summary);
    Assert.Equal(patientId, result.Summary.PatientId);
    Assert.Equal("Functional Test Summary", result.Summary.SummaryText);
  }
}
