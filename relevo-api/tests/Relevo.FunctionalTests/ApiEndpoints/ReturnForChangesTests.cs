using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Handovers;
using Relevo.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Dapper;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

/// <summary>
/// Tests for ReturnForChanges endpoint
/// V3_PLAN.md regla #21: ReturnForChanges vuelve a Draft limpiando READY_AT
/// Each test creates its own handover to avoid race conditions.
/// </summary>
[Collection("Sequential")]
public class ReturnForChangesTests(CustomWebApplicationFactory<Program> factory) 
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateAuthenticatedClient();

    [Fact]
    public async Task ReturnForChanges_WhenHandoverIsReady_ReturnsToDraft()
    {
        // Arrange: Create unique handover and mark it as Ready
        var handoverId = await CreateHandoverWithCoverageForTest("return-ready");
        
        // Mark as Ready
        var readyResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/ready", new { });
        readyResponse.EnsureSuccessStatusCode();
        
        var afterReady = await GetHandover(handoverId);
        Assert.Equal("Ready", afterReady.Status);

        // Act: Return for changes
        var returnResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/return-for-changes", new { });
        returnResponse.EnsureSuccessStatusCode();

        // Assert: Should be back to Draft
        var afterReturn = await GetHandover(handoverId);
        Assert.Equal("Draft", afterReturn.Status);
        Assert.Null(afterReturn.ReadyAt);
    }

    [Fact]
    public async Task ReturnForChanges_WhenHandoverIsDraft_ReturnsBadRequest()
    {
        // Arrange: Create unique handover in Draft state
        var handoverId = await CreateHandoverWithCoverageForTest("return-draft");
        
        var initial = await GetHandover(handoverId);
        Assert.Equal("Draft", initial.Status);

        // Act: Try to return for changes from Draft
        var returnResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/return-for-changes", new { });

        // Assert: Should fail (can only return from Ready state)
        Assert.Equal(HttpStatusCode.BadRequest, returnResponse.StatusCode);
    }

    [Fact]
    public async Task ReturnForChanges_WhenHandoverIsCompleted_ReturnsBadRequest()
    {
        // Arrange: Create unique handover and complete it
        var handoverId = await CreateHandoverWithCoverageForTest("return-completed");
        
        // Mark as Ready
        var readyResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/ready", new { });
        readyResponse.EnsureSuccessStatusCode();
        
        // Start
        var startResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/start", new { });
        startResponse.EnsureSuccessStatusCode();
        
        // Complete
        var completeResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/complete", new { });
        completeResponse.EnsureSuccessStatusCode();

        var completed = await GetHandover(handoverId);
        Assert.Equal("Completed", completed.Status);

        // Act: Try to return for changes from Completed
        var returnResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/return-for-changes", new { });

        // Assert: Should fail (cannot return from Completed state)
        Assert.Equal(HttpStatusCode.BadRequest, returnResponse.StatusCode);
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
        var senderUserId = "dr-sender"; // Use hardcoded sender from seeder
        var receiverUserId = "dr-1"; // Use hardcoded receiver from seeder
        var patientId = $"pat-{testName}-{testRunId}";

        // Create patient and coverage using direct DB access (required for handover creation)
        using (var scope = factory.Services.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
            using var conn = dbFactory.CreateConnection();
            
            // Create patient
            await conn.ExecuteAsync(@"
                INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :Name, :UnitId, :DateOfBirth, :Gender, :AdmissionDate, :RoomNumber, :Diagnosis, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { 
                    Id = patientId, 
                    Name = $"Test Patient {testName}", 
                    UnitId = unitId, 
                    DateOfBirth = new DateTime(2010, 1, 1), 
                    Gender = "Male", 
                    AdmissionDate = DateTime.Now.AddDays(-1), 
                    RoomNumber = "101", 
                    Diagnosis = "Test" 
                });

            // Get shift templates to create shift instances
            var fromShift = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT START_TIME, END_TIME FROM SHIFTS WHERE ID = :shiftId",
                new { shiftId = fromShiftId });
            var toShift = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT START_TIME, END_TIME FROM SHIFTS WHERE ID = :shiftId",
                new { shiftId = toShiftId });

            if (fromShift != null && toShift != null)
            {
                var today = DateTime.Today;
                var fromStartTime = TimeSpan.Parse((string)fromShift!.START_TIME);
                var fromEndTime = TimeSpan.Parse((string)fromShift.END_TIME);
                var toStartTime = TimeSpan.Parse((string)toShift!.START_TIME);
                var toEndTime = TimeSpan.Parse((string)toShift.END_TIME);

                var fromShiftStartAt = today.Add(fromStartTime);
                var fromShiftEndAt = fromEndTime < fromStartTime 
                    ? today.AddDays(1).Add(fromEndTime) 
                    : today.Add(fromEndTime);
                
                var toShiftStartAt = toStartTime < fromEndTime 
                    ? today.AddDays(1).Add(toStartTime) 
                    : today.Add(toStartTime);
                var toShiftEndAt = toEndTime < toStartTime 
                    ? toShiftStartAt.AddDays(1).Add(toEndTime - TimeSpan.FromDays(1)) 
                    : toShiftStartAt.Add(toEndTime - toStartTime);

                // Get or create shift instances
                var fromShiftInstanceId = await conn.ExecuteScalarAsync<string>(@"
                    SELECT ID FROM SHIFT_INSTANCES 
                    WHERE UNIT_ID = :unitId AND SHIFT_ID = :shiftId AND START_AT = :startAt",
                    new { unitId, shiftId = fromShiftId, startAt = fromShiftStartAt });
                
                if (string.IsNullOrEmpty(fromShiftInstanceId))
                {
                    fromShiftInstanceId = $"si-{Guid.NewGuid().ToString()[..8]}";
                    await conn.ExecuteAsync(@"
                        INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT, CREATED_AT, UPDATED_AT)
                        VALUES (:id, :unitId, :shiftId, :startAt, :endAt, SYSTIMESTAMP, SYSTIMESTAMP)",
                        new { id = fromShiftInstanceId, unitId, shiftId = fromShiftId, startAt = fromShiftStartAt, endAt = fromShiftEndAt });
                }

                var toShiftInstanceId = await conn.ExecuteScalarAsync<string>(@"
                    SELECT ID FROM SHIFT_INSTANCES 
                    WHERE UNIT_ID = :unitId AND SHIFT_ID = :shiftId AND START_AT = :startAt",
                    new { unitId, shiftId = toShiftId, startAt = toShiftStartAt });
                
                if (string.IsNullOrEmpty(toShiftInstanceId))
                {
                    toShiftInstanceId = $"si-{Guid.NewGuid().ToString()[..8]}";
                    await conn.ExecuteAsync(@"
                        INSERT INTO SHIFT_INSTANCES (ID, UNIT_ID, SHIFT_ID, START_AT, END_AT, CREATED_AT, UPDATED_AT)
                        VALUES (:id, :unitId, :shiftId, :startAt, :endAt, SYSTIMESTAMP, SYSTIMESTAMP)",
                        new { id = toShiftInstanceId, unitId, shiftId = toShiftId, startAt = toShiftStartAt, endAt = toShiftEndAt });
                }

                // Create coverage for FROM shift (required for handover creation and Ready)
                var coverageFromId = $"sc-from-{testRunId}";
                await conn.ExecuteAsync(@"
                    MERGE INTO SHIFT_COVERAGE sc
                    USING (SELECT :Id AS ID FROM DUAL) src ON (sc.ID = src.ID)
                    WHEN NOT MATCHED THEN
                    INSERT (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, ASSIGNED_AT, IS_PRIMARY)
                    VALUES (:Id, :UserId, :PatientId, :ShiftInstanceId, :UnitId, SYSTIMESTAMP, 1)",
                    new { Id = coverageFromId, UserId = senderUserId, PatientId = patientId, ShiftInstanceId = fromShiftInstanceId, UnitId = unitId });

                // Create coverage for TO shift (required for Start and Complete)
                var coverageToId = $"sc-to-{testRunId}";
                await conn.ExecuteAsync(@"
                    MERGE INTO SHIFT_COVERAGE sc
                    USING (SELECT :Id AS ID FROM DUAL) src ON (sc.ID = src.ID)
                    WHEN NOT MATCHED THEN
                    INSERT (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, ASSIGNED_AT, IS_PRIMARY)
                    VALUES (:Id, :UserId, :PatientId, :ShiftInstanceId, :UnitId, SYSTIMESTAMP, 0)",
                    new { Id = coverageToId, UserId = receiverUserId, PatientId = patientId, ShiftInstanceId = toShiftInstanceId, UnitId = unitId });
            }
        }

        // Create handover using HTTP endpoint
        var createRequest = new CreateHandoverRequestDto
        {
            PatientId = patientId,
            FromDoctorId = senderUserId,
            ToDoctorId = receiverUserId,
            FromShiftId = fromShiftId,
            ToShiftId = toShiftId,
            InitiatedBy = senderUserId,
            Notes = $"Test handover for {testName}"
        };

        var createResponse = await _client.PostAsJsonAsync("/handovers", createRequest);
        createResponse.EnsureSuccessStatusCode();
        
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateHandoverResponse>();
        Assert.NotNull(createResult);
        Assert.NotNull(createResult.Id);
        Assert.Equal("Draft", createResult.Status);

        return createResult.Id;
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

