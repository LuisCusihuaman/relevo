using System.Net.Http.Json;
using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Me;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class MeEndpoints(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task AssignmentsAndGetMyPatients_Flow_Works()
  {
    var payload = new PostAssignmentsRequest
    {
      ShiftId = "shift-day",
      PatientIds = new() { "pat-123", "pat-456" }
    };

    var postResp = await _client.PostAsJsonAsync("/me/assignments", payload);
    postResp.EnsureSuccessStatusCode();

    var result = await _client.GetAndDeserializeAsync<GetMyPatientsResponse>("/me/patients?page=1&pageSize=25");
    Assert.NotNull(result);
    Assert.True(result.TotalCount >= 2);
    Assert.Contains(result.Patients, p => p.Id == "pat-123");
    Assert.Contains(result.Patients, p => p.Id == "pat-456");
  }
}


