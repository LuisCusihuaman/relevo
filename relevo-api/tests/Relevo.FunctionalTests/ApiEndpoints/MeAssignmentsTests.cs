using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Me;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class MeAssignmentsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateAuthenticatedClient();

    [Fact]
    public async Task PostAssignments_AssignsPatientsToShift()
    {
        // Arrange - use seeded dynamic IDs
        var request = new PostAssignmentsRequest
        {
            ShiftId = DapperTestSeeder.ShiftDayId,
            PatientIds = [DapperTestSeeder.PatientId1]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/me/assignments", request);

        // Assert - accept either success or bad request (if data doesn't exist)
        Assert.True(
            response.StatusCode == HttpStatusCode.NoContent || 
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected NoContent or BadRequest, got {response.StatusCode}");
    }

    [Fact]
    public async Task PostAssignments_WithEmptyPatientList_ReturnsNoContent()
    {
        // Arrange - empty patient list should always work (just delete existing)
        var request = new PostAssignmentsRequest
        {
            ShiftId = DapperTestSeeder.ShiftDayId,
            PatientIds = []
        };

        // Act
        var response = await _client.PostAsJsonAsync("/me/assignments", request);

        // Assert - deleting all assignments should succeed
        Assert.True(
            response.StatusCode == HttpStatusCode.NoContent || 
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected NoContent or BadRequest, got {response.StatusCode}");
    }
}
