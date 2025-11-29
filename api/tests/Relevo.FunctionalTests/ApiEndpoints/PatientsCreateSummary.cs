using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Patients;
using Xunit;
using System.Net;
using System.Net.Http.Json;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsCreateSummary(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsCreatedSummary()
  {
    var request = new CreatePatientSummaryRequest
    {
        PatientId = "pat-001",
        SummaryText = "Functional Test Summary"
    };

    var response = await _client.PostAsJsonAsync("/patients/pat-001/summary", request);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<CreatePatientSummaryResponse>();

    Assert.NotNull(result);
    Assert.NotNull(result.Summary);
    Assert.Equal("pat-001", result.Summary.PatientId);
    Assert.Equal("Functional Test Summary", result.Summary.SummaryText);
    Assert.Equal("dr-1", result.Summary.LastEditedBy); // Based on hardcoded mock user in endpoint
  }
}
