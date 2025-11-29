using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Patients;
using Xunit;
using System.Net;
using System.Net.Http.Json;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsUpdateSummary(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task UpdatesSummarySuccessfully()
  {
    var patientId = DapperTestSeeder.PatientId1;
    var request = new UpdatePatientSummaryRequest
    {
        PatientId = patientId,
        SummaryText = "Updated Functional Test Summary"
    };

    var response = await _client.PutAsJsonAsync($"/patients/{patientId}/summary", request);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<UpdatePatientSummaryResponse>();
    Assert.NotNull(result);
    Assert.True(result.Success);
  }

  [Fact]
  public async Task ReturnsNotFoundForNonExistentSummary()
  {
    var request = new UpdatePatientSummaryRequest
    {
        PatientId = "pat-no-summary",
        SummaryText = "Updated Functional Test Summary"
    };

    var response = await _client.PutAsJsonAsync("/patients/pat-no-summary/summary", request);
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }
}
