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
    // Add authentication token for the test
    _client.DefaultRequestHeaders.Add("x-clerk-user-token", "test-token-valid");

    var payload = new PostAssignmentsRequest
    {
      ShiftId = "shift-day",
      PatientIds = new() { "pat-001", "pat-002" }
    };

    var postResp = await _client.PostAsJsonAsync("/me/assignments", payload);
    postResp.EnsureSuccessStatusCode();

    var result = await _client.GetAndDeserializeAsync<GetMyPatientsResponse>("/me/patients?page=1&pageSize=25");
    Assert.NotNull(result);
    Assert.True(result.Pagination.TotalCount >= 2);
    Assert.Contains(result.Items, p => p.Id == "pat-001");
    Assert.Contains(result.Items, p => p.Id == "pat-002");
  }
}


