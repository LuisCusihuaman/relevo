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
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

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
  public async Task ReturnsBadRequestForNonExistentPatient()
  {
    // Patient summary now requires a handover, which requires an assignment
    // If patient has no assignment, GetOrCreateCurrentHandoverIdAsync returns null
    // Handler returns Error (BadRequest) instead of NotFound
    var request = new UpdatePatientSummaryRequest
    {
        PatientId = "pat-no-summary",
        SummaryText = "Updated Functional Test Summary"
    };

    var response = await _client.PutAsJsonAsync("/patients/pat-no-summary/summary", request);
    // Returns BadRequest because patient has no assignment (cannot create handover)
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }
}
