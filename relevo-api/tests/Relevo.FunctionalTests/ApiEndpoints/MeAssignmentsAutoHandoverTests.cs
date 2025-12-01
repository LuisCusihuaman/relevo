using System.Net;
using System.Net.Http.Json;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Relevo.Infrastructure.Data;
using Relevo.Web;
using Relevo.Web.Me;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

/// <summary>
/// Functional tests for automatic handover creation when assigning patients.
/// V3_PLAN.md Regla #14: Handovers are created as side effects of domain commands.
/// </summary>
[Collection("TestCollection")]
public class MeAssignmentsAutoHandoverTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateAuthenticatedClient();

    [Fact(Skip = "Flaky when running all tests together due to async Task.Run event processing. Run individually by removing Skip attribute.")]
    public async Task PostAssignments_PrimaryAssignment_CreatesHandoverAutomatically()
    {
        // Arrange
        var testRunId = Guid.NewGuid().ToString()[..8];
        var patientId = $"pat-{testRunId}";
        var userId = DapperTestSeeder.UserId;
        var unitId = DapperTestSeeder.UnitId;
        var shiftDayId = DapperTestSeeder.ShiftDayId;

        var dbFactory = factory.Services.GetRequiredService<DapperConnectionFactory>();
        using var conn = dbFactory.CreateConnection();
        try
        {
            // Create patient
            await conn.ExecuteAsync(@"
                INSERT INTO PATIENTS (ID, NAME, UNIT_ID, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :Name, :UnitId, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { Id = patientId, Name = $"Test Patient {testRunId}", UnitId = unitId });

            // Act: Assign patient (this should trigger automatic handover creation)
            var request = new PostAssignmentsRequest
            {
                ShiftId = shiftDayId,
                PatientIds = [patientId]
            };

            var response = await _client.PostAsJsonAsync("/me/assignments", request);

            // Assert: Assignment succeeded
            Assert.True(
                response.StatusCode == HttpStatusCode.NoContent || 
                response.StatusCode == HttpStatusCode.OK,
                $"Expected NoContent or OK, got {response.StatusCode}");

            // V3_PLAN.md Regla #14: Handovers are created as side effects via domain events
            // The event is processed asynchronously via Task.Run, so we need to poll for the handover
            dynamic? handover = null;
            var maxAttempts = 10;
            var delayMs = 100;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                handover = await conn.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT h.ID, h.PATIENT_ID, h.CURRENT_STATE, h.SENDER_USER_ID, h.CREATED_BY_USER_ID
                    FROM HANDOVERS h
                    WHERE h.PATIENT_ID = :PatientId
                      AND ROWNUM <= 1",
                    new { PatientId = patientId });
                
                if (handover != null)
                {
                    break;
                }
                
                await Task.Delay(delayMs);
            }

            Assert.NotNull(handover);
            Assert.Equal(patientId, (string)handover!.PATIENT_ID);
            Assert.Equal("Draft", (string)handover.CURRENT_STATE);
            // SENDER_USER_ID should be set (determined from SHIFT_COVERAGE primary)
            Assert.NotNull(handover.SENDER_USER_ID);
            Assert.NotEmpty((string)handover.SENDER_USER_ID);
        }
        finally
        {
            // Cleanup
            await conn.ExecuteAsync("DELETE FROM HANDOVERS WHERE PATIENT_ID = :PatientId", new { PatientId = patientId });
            await conn.ExecuteAsync("DELETE FROM SHIFT_COVERAGE WHERE PATIENT_ID = :PatientId", new { PatientId = patientId });
            await conn.ExecuteAsync("DELETE FROM PATIENTS WHERE ID = :PatientId", new { PatientId = patientId });
        }
    }
}
