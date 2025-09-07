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
    // Use a consistent test token format that matches our TestAuthenticationService expectations
    var testUserId = "user_2test12345678901234567890123456";
    _client.DefaultRequestHeaders.Add("x-clerk-user-token", $"test-token-{testUserId}");

    var payload = new PostAssignmentsRequest
    {
      ShiftId = "shift-day",
      PatientIds = new() { "pat-001", "pat-002" }
    };

    // POST assignment and verify success
    var postResp = await _client.PostAsJsonAsync("/me/assignments", payload);
    postResp.EnsureSuccessStatusCode();

    // Verify assignment response headers for debugging
    var assignmentUserId = postResp.Headers.GetValues("X-Debug-UserId").FirstOrDefault();
    var assignmentPatientIds = postResp.Headers.GetValues("X-Debug-PatientIds").FirstOrDefault();
    Assert.Equal(testUserId, assignmentUserId);
    Assert.Equal("pat-001,pat-002", assignmentPatientIds);

    // GET my patients using the same token (same user context)
    var getResp = await _client.GetAsync("/me/patients?page=1&pageSize=25");
    getResp.EnsureSuccessStatusCode();

    var result = await getResp.Content.ReadFromJsonAsync<GetMyPatientsResponse>();
    Assert.NotNull(result);

    // Verify retrieval response headers match assignment
    var retrievalUserId = getResp.Headers.GetValues("X-Debug-UserId").FirstOrDefault();
    Assert.Equal(testUserId, retrievalUserId); // Same user ID should be used

    // Verify patients are returned
    Assert.True(result.Pagination.TotalCount >= 2);
    Assert.Contains(result.Items, p => p.Id == "pat-001");
    Assert.Contains(result.Items, p => p.Id == "pat-002");

    // Log for debugging consistency
    Console.WriteLine($"Test completed - Assignment UserId: {assignmentUserId}, Retrieval UserId: {retrievalUserId}");
  }
}


