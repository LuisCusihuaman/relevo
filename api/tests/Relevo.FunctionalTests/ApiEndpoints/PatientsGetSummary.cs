using Ardalis.HttpClientTestExtensions;
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
    var result = await _client.GetAndDeserializeAsync<GetPatientSummaryResponse>("/patients/pat-001/summary");

    Assert.NotNull(result);
    Assert.NotNull(result.Summary);
    Assert.Equal("pat-001", result.Summary.PatientId);
    Assert.Equal("Patient history...", result.Summary.SummaryText);
  }

  [Fact]
  public async Task ReturnsNullSummaryGivenPatientWithoutSummary()
  {
    // Note: The endpoint returns 200 OK with null summary property if not found (legacy behavior preserved),
    // OR 404 depending on handler.
    // Our handler returns NotFound(), so FastEndpoints might return 404.
    // Let's check if we mapped it to 200+null in the endpoint.
    // In GetPatientSummary.cs: if (result.Status == Ardalis.Result.ResultStatus.NotFound) Response = new GetPatientSummaryResponse { Summary = null };
    
    var result = await _client.GetAndDeserializeAsync<GetPatientSummaryResponse>("/patients/pat-no-summary/summary");
    
    Assert.NotNull(result);
    Assert.Null(result.Summary);
  }
}

