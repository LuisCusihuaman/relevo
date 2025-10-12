using System.Net;
using System.Net.Http.Json;
using Xunit;
using Dapper;
using Relevo.Web.Setup;

namespace Relevo.FunctionalTests.ApiEndpoints;

/// <summary>
/// E2E tests for database constraints, particularly the UQ_ACTIVE_HANDOVER_WINDOW unique constraint.
/// Tests verify that business rules are enforced at the database level.
/// </summary>
[Collection("Sequential")]
public class HandoverConstraintTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Parallel_Creation_Same_Patient_Fails_With_Conflict()
    {
        // Arrange: Test doctor IDs
        const string doctorAId = "user_constA12345678901234567890123";
        const string doctorBId = "user_constB12345678901234567890123";

        // Cleanup and create test doctors
        CleanupTestDoctorsFromDatabase(doctorAId, doctorBId);
        CreateTestDoctorsInDatabase(doctorAId, doctorBId);

        try
        {
            var (fromShiftId, toShiftId, patientId) = await GetTestDataAsync();

            var request = new CreateHandoverRequestDto
            {
                PatientId = patientId,
                FromDoctorId = doctorAId,
                ToDoctorId = doctorBId,
                FromShiftId = fromShiftId,
                ToShiftId = toShiftId,
                InitiatedBy = doctorAId,
                Notes = "First handover for same patient/shift/day"
            };

            // Act: Create first handover
            var response1 = await _client.PostAsJsonAsync("/handovers", request);
            response1.EnsureSuccessStatusCode();

            var createdHandover1 = await response1.Content.ReadFromJsonAsync<CreateHandoverResponse>();
            Assert.NotNull(createdHandover1);

            // Try to create duplicate immediately (same patient, same shift transition, same day)
            var response2 = await _client.PostAsJsonAsync("/handovers", request);

            // Assert: Should fail with constraint violation
            // Currently returns 500 (InternalServerError) due to ORA-00001 unique constraint violation
            // After error normalization sprint, this should return 409 (Conflict)
            Assert.Equal(HttpStatusCode.InternalServerError, response2.StatusCode);

            // Verify the error message indicates a constraint violation
            var errorContent = await response2.Content.ReadAsStringAsync();
            Assert.Contains("ORA-00001", errorContent); // Oracle unique constraint error

            // TODO: After error normalization sprint, assert:
            // Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
            // var error = await response2.Content.ReadFromJsonAsync<ErrorResponse>();
            // Assert.Equal("UniqueConstraintViolation", error.Code);
        }
        finally
        {
            CleanupTestDoctorsFromDatabase(doctorAId, doctorBId);
        }
    }

    [Fact]
    public async Task Can_Create_Second_Handover_After_Completing_First()
    {
        // Arrange: Test doctor IDs
        const string doctorAId = "user_complete12345678901234567890123";
        const string doctorBId = "user_complete23456789012345678901234";

        CleanupTestDoctorsFromDatabase(doctorAId, doctorBId);
        CreateTestDoctorsInDatabase(doctorAId, doctorBId);

        try
        {
            var (fromShiftId, toShiftId, patientId) = await GetTestDataAsync();

            // Create first handover
            var request1 = new CreateHandoverRequestDto
            {
                PatientId = patientId,
                FromDoctorId = doctorAId,
                ToDoctorId = doctorBId,
                FromShiftId = fromShiftId,
                ToShiftId = toShiftId,
                InitiatedBy = doctorAId,
                Notes = "First handover"
            };

            var createResponse1 = await _client.PostAsJsonAsync("/handovers", request1);
            createResponse1.EnsureSuccessStatusCode();
            var handover1 = await createResponse1.Content.ReadFromJsonAsync<CreateHandoverResponse>();
            Assert.NotNull(handover1);

            // Complete the first handover (Ready -> Start -> Accept -> Complete)
            await _client.PostAsJsonAsync($"/handovers/{handover1.Id}/ready", new { });
            await _client.PostAsJsonAsync($"/handovers/{handover1.Id}/start", new { HandoverId = handover1.Id });
            await _client.PostAsJsonAsync($"/handovers/{handover1.Id}/accept", new { HandoverId = handover1.Id });
            var completeResponse = await _client.PostAsJsonAsync($"/handovers/{handover1.Id}/complete", new { HandoverId = handover1.Id });
            completeResponse.EnsureSuccessStatusCode();

            // Verify first handover is completed
            var getResponse1 = await _client.GetAsync($"/handovers/{handover1.Id}");
            var completedHandover = await getResponse1.Content.ReadFromJsonAsync<GetHandoverByIdResponse>();
            Assert.Equal("Completed", completedHandover!.StateName);

            // Act: Create second handover for same patient/shift/day - should succeed
            var request2 = new CreateHandoverRequestDto
            {
                PatientId = patientId,
                FromDoctorId = doctorAId,
                ToDoctorId = doctorBId,
                FromShiftId = fromShiftId,
                ToShiftId = toShiftId,
                InitiatedBy = doctorAId,
                Notes = "Second handover after completing first"
            };

            var createResponse2 = await _client.PostAsJsonAsync("/handovers", request2);

            // Assert: Should succeed (first handover is completed, so not "active")
            createResponse2.EnsureSuccessStatusCode();
            var handover2 = await createResponse2.Content.ReadFromJsonAsync<CreateHandoverResponse>();
            Assert.NotNull(handover2);
            Assert.NotEqual(handover1.Id, handover2.Id);
        }
        finally
        {
            CleanupTestDoctorsFromDatabase(doctorAId, doctorBId);
        }
    }

    [Fact]
    public async Task Can_Create_Different_Shift_Transitions_Same_Patient()
    {
        // Arrange: Test doctor IDs
        const string doctorAId = "user_shifts1234567890123456789012";
        const string doctorBId = "user_shifts2345678901234567890123";

        CleanupTestDoctorsFromDatabase(doctorAId, doctorBId);
        CreateTestDoctorsInDatabase(doctorAId, doctorBId);

        try
        {
            var (shift1Id, shift2Id, patientId) = await GetTestDataAsync();
            var shifts = await GetAllShiftsAsync();

            if (shifts.Count < 3)
            {
                // Skip test if not enough shifts
                return;
            }

            var shift3Id = shifts[2].Id;

            // Create Day->Evening handover
            var request1 = new CreateHandoverRequestDto
            {
                PatientId = patientId,
                FromDoctorId = doctorAId,
                ToDoctorId = doctorBId,
                FromShiftId = shift1Id,
                ToShiftId = shift2Id,
                InitiatedBy = doctorAId,
                Notes = "Day to Evening transition"
            };

            var createResponse1 = await _client.PostAsJsonAsync("/handovers", request1);
            createResponse1.EnsureSuccessStatusCode();
            var handover1 = await createResponse1.Content.ReadFromJsonAsync<CreateHandoverResponse>();

            // Act: Create Evening->Night handover (different shift pair)
            var request2 = new CreateHandoverRequestDto
            {
                PatientId = patientId,
                FromDoctorId = doctorAId,
                ToDoctorId = doctorBId,
                FromShiftId = shift2Id,
                ToShiftId = shift3Id,
                InitiatedBy = doctorAId,
                Notes = "Evening to Night transition"
            };

            var createResponse2 = await _client.PostAsJsonAsync("/handovers", request2);

            // Assert: Should succeed (different shift transition)
            createResponse2.EnsureSuccessStatusCode();
            var handover2 = await createResponse2.Content.ReadFromJsonAsync<CreateHandoverResponse>();
            Assert.NotNull(handover2);
            Assert.NotEqual(handover1!.Id, handover2.Id);
        }
        finally
        {
            CleanupTestDoctorsFromDatabase(doctorAId, doctorBId);
        }
    }

    #region Helper Methods

    private async Task<(string fromShiftId, string toShiftId, string patientId)> GetTestDataAsync()
    {
        // Get units with patients
        var unitsResponse = await _client.GetAsync("/setup/units");
        unitsResponse.EnsureSuccessStatusCode();
        var units = await unitsResponse.Content.ReadFromJsonAsync<UnitListResponse>();
        Assert.NotNull(units);
        Assert.NotNull(units.Units);
        Assert.NotEmpty(units.Units);

        string? testPatientId = null;

        // Find a unit with patients
        foreach (var unit in units.Units)
        {
            var patientsResponse = await _client.GetAsync($"/units/{unit.Id}/patients?page=1&pageSize=10");
            if (patientsResponse.IsSuccessStatusCode)
            {
                var patientsResult = await patientsResponse.Content.ReadFromJsonAsync<PatientListResponse>();
                if (patientsResult?.Patients?.Any() == true)
                {
                    testPatientId = patientsResult.Patients[0].Id;
                    break;
                }
            }
        }

        Assert.NotNull(testPatientId);

        // Get shifts
        var shiftsResponse = await _client.GetAsync("/setup/shifts");
        shiftsResponse.EnsureSuccessStatusCode();
        var shifts = await shiftsResponse.Content.ReadFromJsonAsync<ShiftListResponse>();
        Assert.NotNull(shifts);
        Assert.NotNull(shifts.Shifts);
        Assert.True(shifts.Shifts.Count >= 2, "Need at least 2 shifts for testing");

        return (shifts.Shifts[0].Id, shifts.Shifts[1].Id, testPatientId);
    }

    private async Task<List<ShiftItem>> GetAllShiftsAsync()
    {
        var shiftsResponse = await _client.GetAsync("/setup/shifts");
        shiftsResponse.EnsureSuccessStatusCode();
        var shiftsResult = await shiftsResponse.Content.ReadFromJsonAsync<ShiftListResponse>();
        return shiftsResult?.Shifts ?? new List<ShiftItem>();
    }

    private void CreateTestDoctorsInDatabase(string doctorAId, string doctorBId)
    {
        try
        {
            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(
                "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15");
            connection.Open();

            connection.Execute(@"
                MERGE INTO USERS u
                USING (
                    SELECT :DoctorAId AS ID, 'Doctor A' AS NAME, 'doctorA@e2etest.com' AS EMAIL FROM DUAL
                    UNION ALL
                    SELECT :DoctorBId AS ID, 'Doctor B' AS NAME, 'doctorB@e2etest.com' AS EMAIL FROM DUAL
                ) src
                ON (u.ID = src.ID)
                WHEN NOT MATCHED THEN
                    INSERT (ID, NAME, EMAIL, CREATED_AT)
                    VALUES (src.ID, src.NAME, src.EMAIL, SYSTIMESTAMP)",
                new { DoctorAId = doctorAId, DoctorBId = doctorBId });
        }
        catch
        {
            // Ignore if already exists
        }
    }

    private void CleanupTestDoctorsFromDatabase(string doctorAId, string doctorBId)
    {
        try
        {
            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(
                "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15");
            connection.Open();

            void SafeDelete(string table)
            {
                try
                {
                    connection.Execute($"DELETE FROM {table} WHERE HANDOVER_ID IN (SELECT ID FROM HANDOVERS WHERE FROM_DOCTOR_ID IN (:DoctorAId, :DoctorBId) OR TO_DOCTOR_ID IN (:DoctorAId, :DoctorBId))", new { DoctorAId = doctorAId, DoctorBId = doctorBId });
                }
                catch (Oracle.ManagedDataAccess.Client.OracleException) { /* Table doesn't exist, skip */ }
            }

            SafeDelete("HANDOVER_SYNC");
            SafeDelete("HANDOVER_PATIENT_DATA");
            SafeDelete("HANDOVER_SITUATION_AWARENESS");
            SafeDelete("HANDOVER_SYNTHESIS");
            SafeDelete("HANDOVER_PARTICIPANTS");
            SafeDelete("HANDOVER_MESSAGES");
            SafeDelete("HANDOVER_ACTIVITY_LOG");
            SafeDelete("HANDOVER_ACTION_ITEMS");

            try
            {
                connection.Execute("DELETE FROM HANDOVERS WHERE FROM_DOCTOR_ID IN (:DoctorAId, :DoctorBId) OR TO_DOCTOR_ID IN (:DoctorAId, :DoctorBId)", new { DoctorAId = doctorAId, DoctorBId = doctorBId });
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException) { /* Table doesn't exist, skip */ }

            try
            {
                connection.Execute("DELETE FROM USERS WHERE ID IN (:DoctorAId, :DoctorBId) AND EMAIL LIKE '%@e2etest.com'", new { DoctorAId = doctorAId, DoctorBId = doctorBId });
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException) { /* Table doesn't exist, skip */ }
        }
        catch
        {
            // Silently ignore connection errors during cleanup
        }
    }

    #endregion

    #region Local DTOs

    private class CreateHandoverRequestDto
    {
        public string PatientId { get; set; } = string.Empty;
        public string FromDoctorId { get; set; } = string.Empty;
        public string ToDoctorId { get; set; } = string.Empty;
        public string FromShiftId { get; set; } = string.Empty;
        public string ToShiftId { get; set; } = string.Empty;
        public string InitiatedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    private class CreateHandoverResponse
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    private class GetHandoverByIdResponse
    {
        public string Id { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
    }

    private class UnitDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    private class ShiftDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    private class PatientListResponse
    {
        public List<PatientDto> Patients { get; set; } = new();
        public int TotalCount { get; set; }
    }

    private class PatientDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}

