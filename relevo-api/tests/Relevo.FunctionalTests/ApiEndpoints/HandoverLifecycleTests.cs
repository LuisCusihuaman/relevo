using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Handovers;
using Relevo.Infrastructure.Data;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Dapper;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

/// <summary>
/// Tests for handover lifecycle state transitions.
/// These tests validate that the state machine constraints are enforced.
/// Each test creates its own handover to avoid race conditions.
/// </summary>
[Collection("Sequential")]
public class HandoverLifecycleTests(CustomWebApplicationFactory<Program> factory) 
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateAuthenticatedClient();

    [Fact]
    public async Task Handover_CanTransition_Ready_Start_Complete()
    {
        // V3: State machine is Draft → Ready → InProgress → Completed (no Accepted state)
        // Create a unique handover for this test to avoid race conditions
        // V3_PLAN.md regla #10: Ready requires coverage >= 1
        var handoverId = await CreateHandoverWithCoverageForTest("lifecycle-transition");
        
        // Verify initial state is Draft
        var initial = await GetHandover(handoverId);
        Assert.Equal("Draft", initial.Status);

        // Ready (requires coverage - we created it)
        var readyResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/ready", new { });
        readyResponse.EnsureSuccessStatusCode();
        
        var afterReady = await GetHandover(handoverId);
        Assert.Equal("Ready", afterReady.Status);

        // Start
        var startResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/start", new { });
        startResponse.EnsureSuccessStatusCode();

        var afterStart = await GetHandover(handoverId);
        Assert.Equal("InProgress", afterStart.Status);
        
        // Complete (V3: no Accept step, can complete directly from InProgress)
        var completeResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/complete", new { });
        completeResponse.EnsureSuccessStatusCode();

        var afterComplete = await GetHandover(handoverId);
        Assert.Equal("Completed", afterComplete.Status);
    }

    [Fact]
    public async Task StateChange_WithoutProperTransition_ReturnsBadRequest()
    {
        // V3: Trying to complete a handover from Draft should fail
        // Create a unique handover for this test to avoid race conditions
        var handoverId = await CreateHandoverWithCoverageForTest("lifecycle-invalid-transition");
        
        // Verify initial state is Draft
        var initial = await GetHandover(handoverId);
        Assert.Equal("Draft", initial.Status);
        
        // Try to complete directly from Draft (should fail - needs to be InProgress)
        var completeResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/complete", new { });
        
        // V3: Complete requires InProgress state (not Accepted, which doesn't exist)
        // Should return BadRequest when trying to complete from Draft
        Assert.Equal(HttpStatusCode.BadRequest, completeResponse.StatusCode);
    }

    /// <summary>
    /// Creates a unique handover with coverage for testing.
    /// Uses direct database access to create patient and coverage, then HTTP endpoint to create handover.
    /// </summary>
    private async Task<string> CreateHandoverWithCoverageForTest(string testName)
    {
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;
        var senderUserId = "dr-sender";
        var receiverUserId = "dr-1";
        var patientId = $"pat-{testName}-{testRunId}";

        using var scope = factory.Services.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
        var shiftInstanceRepo = scope.ServiceProvider.GetRequiredService<IShiftInstanceRepository>();
        using var conn = dbFactory.CreateConnection();
        
        // Create patient
        await conn.ExecuteAsync(@"
            INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT)
            VALUES (:Id, :Name, :UnitId, :DateOfBirth, :Gender, :AdmissionDate, :RoomNumber, :Diagnosis, SYSTIMESTAMP, SYSTIMESTAMP)",
            new { Id = patientId, Name = $"Test Patient {testName}", UnitId = unitId, 
                DateOfBirth = new DateTime(2010, 1, 1), Gender = "Male", 
                AdmissionDate = DateTime.Now.AddDays(-1), RoomNumber = "101", Diagnosis = "Test" });

        // Calculate and create shift instances
        var shiftDates = await ShiftInstanceCalculationService.CalculateShiftInstanceDatesFromDbAsync(
            conn, fromShiftId, toShiftId, DateTime.Today);
        if (shiftDates == null) throw new InvalidOperationException("Shift templates not found");

        var (fromStart, fromEnd, toStart, toEnd) = shiftDates.Value;
        var fromInstanceId = await shiftInstanceRepo.GetOrCreateShiftInstanceAsync(fromShiftId, unitId, fromStart, fromEnd);
        var toInstanceId = await shiftInstanceRepo.GetOrCreateShiftInstanceAsync(toShiftId, unitId, toStart, toEnd);

        // Create coverage
        await conn.ExecuteAsync(@"
            INSERT INTO SHIFT_COVERAGE (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, IS_PRIMARY, ASSIGNED_AT)
            VALUES (:Id, :UserId, :PatientId, :ShiftInstanceId, :UnitId, 1, SYSTIMESTAMP)",
            new { Id = $"sc-from-{testRunId}", UserId = senderUserId, PatientId = patientId, 
                ShiftInstanceId = fromInstanceId, UnitId = unitId });
        await conn.ExecuteAsync(@"
            INSERT INTO SHIFT_COVERAGE (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, IS_PRIMARY, ASSIGNED_AT)
            VALUES (:Id, :UserId, :PatientId, :ShiftInstanceId, :UnitId, 0, SYSTIMESTAMP)",
            new { Id = $"sc-to-{testRunId}", UserId = receiverUserId, PatientId = patientId, 
                ShiftInstanceId = toInstanceId, UnitId = unitId });

        // Create handover via HTTP endpoint
        var response = await _client.PostAsJsonAsync("/handovers", new CreateHandoverRequestDto
        {
            PatientId = patientId, FromDoctorId = senderUserId, ToDoctorId = receiverUserId,
            FromShiftId = fromShiftId, ToShiftId = toShiftId, InitiatedBy = senderUserId,
            Notes = $"Test handover for {testName}"
        });
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<CreateHandoverResponse>();
        Assert.NotNull(result?.Id);
        Assert.Equal("Draft", result.Status);
        return result.Id;
    }

    private async Task<GetHandoverByIdResponse> GetHandover(string id)
    {
        var response = await _client.GetAsync($"/handovers/{id}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GetHandoverByIdResponse>();
        Assert.NotNull(result);
        return result;
    }
}
