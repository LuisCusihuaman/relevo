using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Patients;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsGetSummary(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsSummaryGivenValidPatientId()
  {
    var patientId = DapperTestSeeder.PatientId1;
    var result = await _client.GetAndDeserializeAsync<GetPatientSummaryResponse>($"/patients/{patientId}/summary");

    Assert.NotNull(result);
    Assert.NotNull(result.Summary);
    Assert.Equal(patientId, result.Summary.PatientId);
    Assert.NotNull(result.Summary.SummaryText); // Value may change due to other tests
  }

  [Fact]
  public async Task ReturnsNullSummaryGivenPatientWithoutSummary()
  {
    var result = await _client.GetAndDeserializeAsync<GetPatientSummaryResponse>("/patients/pat-no-summary/summary");
    
    Assert.NotNull(result);
    Assert.Null(result.Summary);
  }
}
