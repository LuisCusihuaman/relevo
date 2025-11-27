using System.Net.Http.Json;
using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Me;
using Xunit;
using Dapper;
using Oracle.ManagedDataAccess.Client;

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
    
    // Cleanup any existing data for this user to avoid constraint violations
    CleanupUserHandovers(testUserId);

    _client.DefaultRequestHeaders.Add("x-clerk-user-token", $"test-token-{testUserId}");

    var payload = new PostAssignmentsRequest
    {
      ShiftId = "shift-day",
      PatientIds = new() { "pat-025", "pat-026" }
    };

    // POST assignment and verify success
    var postResp = await _client.PostAsJsonAsync("/me/assignments", payload);
    if (!postResp.IsSuccessStatusCode)
    {
        var content = await postResp.Content.ReadAsStringAsync();
        throw new HttpRequestException($"POST /me/assignments failed with {postResp.StatusCode}: {content}");
    }
    postResp.EnsureSuccessStatusCode();

    // Verify assignment response headers for debugging
    var assignmentUserId = postResp.Headers.GetValues("X-Debug-UserId").FirstOrDefault();
    var assignmentPatientIds = postResp.Headers.GetValues("X-Debug-PatientIds").FirstOrDefault();
    Assert.Equal(testUserId, assignmentUserId);
    Assert.Equal("pat-025,pat-026", assignmentPatientIds);

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
    Assert.Contains(result.Items, p => p.Id == "pat-025");
    Assert.Contains(result.Items, p => p.Id == "pat-026");

    // Log for debugging consistency
    Console.WriteLine($"Test completed - Assignment UserId: {assignmentUserId}, Retrieval UserId: {retrievalUserId}");
  }

  private void CleanupUserHandovers(string userId)
  {
    try
    {
      using var connection = new OracleConnection(
        "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15");
      connection.Open();

      // Find assignments for this user
      var assignmentIds = connection.Query<string>("SELECT ASSIGNMENT_ID FROM USER_ASSIGNMENTS WHERE USER_ID = :userId", new { userId }).ToList();
      
      if (assignmentIds.Any())
      {
          // Delete handovers linked to these assignments
          // Delete dependent data first
          connection.Execute(@"
            DELETE FROM HANDOVER_PARTICIPANTS WHERE HANDOVER_ID IN (
                SELECT ID FROM HANDOVERS WHERE ASSIGNMENT_ID IN :assignmentIds
            )", new { assignmentIds });
            
          connection.Execute(@"
            DELETE FROM HANDOVER_PATIENT_DATA WHERE HANDOVER_ID IN (
                SELECT ID FROM HANDOVERS WHERE ASSIGNMENT_ID IN :assignmentIds
            )", new { assignmentIds });
            
          connection.Execute(@"
            DELETE FROM HANDOVER_SITUATION_AWARENESS WHERE HANDOVER_ID IN (
                SELECT ID FROM HANDOVERS WHERE ASSIGNMENT_ID IN :assignmentIds
            )", new { assignmentIds });
            
          connection.Execute(@"
            DELETE FROM HANDOVER_SYNTHESIS WHERE HANDOVER_ID IN (
                SELECT ID FROM HANDOVERS WHERE ASSIGNMENT_ID IN :assignmentIds
            )", new { assignmentIds });

          // Delete handovers
          connection.Execute("DELETE FROM HANDOVERS WHERE ASSIGNMENT_ID IN :assignmentIds", new { assignmentIds });
      }
    }
    catch
    {
      // Ignore cleanup errors
    }
  }
}


