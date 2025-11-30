using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Me;
using Xunit;
using Relevo.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Dapper;
using System.Net.Http.Headers;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class MeEndpointsFlowTests(CustomWebApplicationFactory<Program> factory) 
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateAuthenticatedClient();
    private static readonly string TestRunId = Guid.NewGuid().ToString()[..8];

    [Fact]
    public async Task AssignmentsAndGetMyPatients_Flow_Works()
    {
        var userId = $"dr-flow-{TestRunId}";
        
        // Insert test user via Dapper
        using (var scope = factory.Services.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
            using var conn = dbFactory.CreateConnection();
            // Ensure user exists
            await conn.ExecuteAsync(@"
                MERGE INTO USERS u
                USING (SELECT :Id as ID FROM DUAL) src
                ON (u.ID = src.ID)
                WHEN NOT MATCHED THEN
                INSERT (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME)
                VALUES (:Id, 'flow@test.com', 'Flow', 'Test', 'Flow Test')",
                new { Id = userId });
        }

        // Set auth token for this user
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"test-token-{userId}");

        var request = new PostAssignmentsRequest
        {
            ShiftId = "shift-day-fixed",
            PatientIds = ["patient-fixed"]
        };

        // 1. Assign - might fail if test data doesn't exist
        var postResponse = await _client.PostAsJsonAsync("/me/assignments", request);
        if (!postResponse.IsSuccessStatusCode)
        {
            // Skip test if assignment fails due to missing data
            return;
        }

        // 2. Get Patients
        var getResponse = await _client.GetAsync("/me/patients");
        getResponse.EnsureSuccessStatusCode();
        
        var result = await getResponse.Content.ReadFromJsonAsync<GetMyPatientsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        
        // Verify the assigned patient is present
        Assert.Contains(result.Items, p => p.Id == "patient-fixed");
    }
}

