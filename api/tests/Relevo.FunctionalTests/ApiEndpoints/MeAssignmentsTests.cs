using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Me;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class MeAssignmentsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PostAssignments_AssignsPatientsToShift()
    {
        // Arrange
        var request = new PostAssignmentsRequest
        {
            ShiftId = DapperTestSeeder.ShiftDayId,
            PatientIds = [DapperTestSeeder.PatientId1, DapperTestSeeder.PatientId2]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/me/assignments", request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task PostAssignments_WithEmptyPatientList_ReturnsNoContent()
    {
        // Arrange
        var request = new PostAssignmentsRequest
        {
            ShiftId = DapperTestSeeder.ShiftDayId,
            PatientIds = []
        };

        // Act
        var response = await _client.PostAsJsonAsync("/me/assignments", request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
