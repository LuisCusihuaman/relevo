using System.Net;
using System.Net.Http.Json;
using Xunit;
using Relevo.Web.Handovers;
using Dapper;

namespace Relevo.FunctionalTests.ApiEndpoints;

/// <summary>
/// E2E tests for the complete handover lifecycle between two doctors.
/// Tests the workflow: Create -> Start -> Accept -> Complete
/// </summary>
[Collection("Sequential")]
public class HandoverLifecycleEndpoints(CustomWebApplicationFactory<Program> factory) 
  : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task HandoverLifecycle_DoctorA_To_DoctorB_Should_Succeed()
  {
    // ============================================================
    // ARRANGE: Setup test data
    // ============================================================

    // Test user IDs (simulating two different doctors)
    const string doctorAId = "user_doctorA123456789012345678901";
    const string doctorBId = "user_doctorB123456789012345678901";

    // Cleanup any existing handovers from previous test runs
    CleanupTestDoctorsFromDatabase(doctorAId, doctorBId);
    
    // Create test doctors in the database
    CreateTestDoctorsInDatabase(doctorAId, doctorBId);

    try
    {
      // Get test data (shifts and patient)
      var (fromShiftId, toShiftId, testPatientId) = await GetTestDataAsync();

    // ============================================================
    // ACT & ASSERT: Execute the handover lifecycle
    // ============================================================

    // STEP 1: Create Handover
    // Doctor A initiates a handover to Doctor B
    var createRequest = new CreateHandoverRequestDto
    {
      PatientId = testPatientId,
      FromDoctorId = doctorAId,
      ToDoctorId = doctorBId,
      FromShiftId = fromShiftId,
      ToShiftId = toShiftId,
      InitiatedBy = doctorAId,
      Notes = "E2E Test: Complete handover lifecycle"
    };

    var createResponse = await _client.PostAsJsonAsync("/handovers", createRequest);
    Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
    
    var createdHandover = await createResponse.Content.ReadFromJsonAsync<CreateHandoverResponse>();
    Assert.NotNull(createdHandover);
    Assert.NotEmpty(createdHandover.Id);
    Assert.Equal(testPatientId, createdHandover.PatientId);
    Assert.Equal("Draft", createdHandover.Status); // Initial status should be Draft
    
    var handoverId = createdHandover.Id;

    // Verify handover was created correctly
    var getAfterCreateResponse = await _client.GetAsync($"/handovers/{handoverId}");
    getAfterCreateResponse.EnsureSuccessStatusCode();
    var handoverAfterCreate = await getAfterCreateResponse.Content.ReadFromJsonAsync<GetHandoverByIdResponse>();
    Assert.NotNull(handoverAfterCreate);
    Assert.Equal(handoverId, handoverAfterCreate.Id);
    Assert.Equal(testPatientId, handoverAfterCreate.PatientId);
    Assert.NotEmpty(handoverAfterCreate.CreatedBy);
    Assert.Null(handoverAfterCreate.StartedAt); // Should not be started yet
    Assert.Null(handoverAfterCreate.AcceptedAt); // Should not be accepted yet
    Assert.Null(handoverAfterCreate.CompletedAt); // Should not be completed yet

    // STEP 2: Ready Handover
    var readyResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/ready", new { Id = handoverId });
    Assert.Equal(HttpStatusCode.OK, readyResponse.StatusCode);

    // STEP 3: Start Handover  
    var startRequest = new StartHandoverRequest { HandoverId = handoverId };
    var startResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/start", startRequest);
    Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);

    // Verify handover is now in InProgress state
    var getAfterStartResponse = await _client.GetAsync($"/handovers/{handoverId}");
    getAfterStartResponse.EnsureSuccessStatusCode();
    var handoverAfterStart = await getAfterStartResponse.Content.ReadFromJsonAsync<GetHandoverByIdResponse>();
    Assert.NotNull(handoverAfterStart);
    Assert.Equal("InProgress", handoverAfterStart.StateName);
    Assert.Null(handoverAfterStart.AcceptedAt); // Should not be accepted yet
    Assert.Null(handoverAfterStart.CompletedAt); // Should not be completed yet

    // STEP 4: Accept Handover
    // Doctor B accepts the handover
    var acceptRequest = new AcceptHandoverRequest { HandoverId = handoverId };
    var acceptResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/accept", acceptRequest);
    Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);
    
    var acceptResult = await acceptResponse.Content.ReadFromJsonAsync<AcceptHandoverResponse>();
    Assert.NotNull(acceptResult);
    Assert.True(acceptResult.Success);
    Assert.Equal(handoverId, acceptResult.HandoverId);

    // Verify handover was accepted
    var getAfterAcceptResponse = await _client.GetAsync($"/handovers/{handoverId}");
    getAfterAcceptResponse.EnsureSuccessStatusCode();
    var handoverAfterAccept = await getAfterAcceptResponse.Content.ReadFromJsonAsync<GetHandoverByIdResponse>();
    Assert.NotNull(handoverAfterAccept);
    Assert.NotNull(handoverAfterAccept.StartedAt); // Should still have started timestamp
    Assert.NotNull(handoverAfterAccept.AcceptedAt); // Should now have accepted timestamp
    Assert.Null(handoverAfterAccept.CompletedAt); // Should not be completed yet

    // STEP 5: Complete Handover
    // Verify state before completing
    var beforeCompleteResponse = await _client.GetAsync($"/handovers/{handoverId}");
    beforeCompleteResponse.EnsureSuccessStatusCode();
    var beforeComplete = await beforeCompleteResponse.Content.ReadFromJsonAsync<GetHandoverByIdResponse>();
    Assert.NotNull(beforeComplete);
    // Log for debugging: state should be "Accepted" at this point
    Console.WriteLine($"State before complete: {beforeComplete.StateName}, AcceptedAt: {beforeComplete.AcceptedAt}, CompletedAt: {beforeComplete.CompletedAt}");

    // Doctor B (or Doctor A) completes the handover
    var completeRequest = new CompleteHandoverRequest { HandoverId = handoverId };
    var completeResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/complete", completeRequest);
    Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
    
    var completeResult = await completeResponse.Content.ReadFromJsonAsync<CompleteHandoverResponse>();
    Assert.NotNull(completeResult);
    Assert.True(completeResult.Success);
    Assert.Equal(handoverId, completeResult.HandoverId);

    // STEP 6: Final Verification
    // Verify the handover is in completed state with all timestamps set
    var finalResponse = await _client.GetAsync($"/handovers/{handoverId}");
    finalResponse.EnsureSuccessStatusCode();
    var finalHandover = await finalResponse.Content.ReadFromJsonAsync<GetHandoverByIdResponse>();
    Assert.NotNull(finalHandover);
    Assert.Equal(handoverId, finalHandover.Id);
    Assert.Equal(testPatientId, finalHandover.PatientId);
    Assert.NotEmpty(finalHandover.CreatedBy);
    Assert.NotNull(finalHandover.StartedAt);
    Assert.NotNull(finalHandover.AcceptedAt);
    Assert.NotNull(finalHandover.CompletedAt); // Should now be completed
    Assert.Equal("Completed", finalHandover.StateName); // State should be Completed

      // Verify the patient data is still accessible
      var patientResponse = await _client.GetAsync($"/handovers/{handoverId}/patient");
      patientResponse.EnsureSuccessStatusCode();
      var patientData = await patientResponse.Content.ReadFromJsonAsync<GetPatientHandoverDataResponse>();
      Assert.NotNull(patientData);
      Assert.Equal(testPatientId, patientData.id);
      Assert.NotEmpty(patientData.name);
    }
    finally
    {
      // Cleanup test doctors
      CleanupTestDoctorsFromDatabase(doctorAId, doctorBId);
    }
  }

  [Fact]
  public async Task HandoverLifecycle_Cannot_Accept_Before_Start()
  {
    // ============================================================
    // ARRANGE: Setup test data
    // ============================================================

    const string doctorAId = "user_doctorA223456789012345678901";
    const string doctorBId = "user_doctorB223456789012345678901";

    // Cleanup any existing handovers from previous test runs
    CleanupTestDoctorsFromDatabase(doctorAId, doctorBId);
    
    // Create test doctors in the database
    CreateTestDoctorsInDatabase(doctorAId, doctorBId);

    try
    {
      // Get test data (shifts and patient)
      var (fromShiftId, toShiftId, testPatientId) = await GetTestDataAsync();

    // ============================================================
    // ACT: Create handover and try to accept without starting
    // ============================================================

    var createRequest = new CreateHandoverRequestDto
    {
      PatientId = testPatientId,
      FromDoctorId = doctorAId,
      ToDoctorId = doctorBId,
      FromShiftId = fromShiftId,
      ToShiftId = toShiftId,
      InitiatedBy = doctorAId,
      Notes = "E2E Test: Try to accept without starting"
    };

    var createResponse = await _client.PostAsJsonAsync("/handovers", createRequest);
    createResponse.EnsureSuccessStatusCode();
    var createdHandover = await createResponse.Content.ReadFromJsonAsync<CreateHandoverResponse>();
    Assert.NotNull(createdHandover);
    var handoverId = createdHandover.Id;

    // Try to accept handover without starting it first
    var acceptRequest = new AcceptHandoverRequest { HandoverId = handoverId };
    var acceptResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/accept", acceptRequest);
    
      // ============================================================
      // ASSERT: Should fail or return false
      // ============================================================
      // Note: The exact behavior depends on implementation
      // It might return 400 Bad Request, 404 Not Found, or 200 OK with Success=false
      if (acceptResponse.StatusCode == HttpStatusCode.OK)
      {
        var acceptResult = await acceptResponse.Content.ReadFromJsonAsync<AcceptHandoverResponse>();
        Assert.NotNull(acceptResult);
        // If it returns OK, success should be false
        Assert.False(acceptResult.Success);
      }
      else
      {
        // Or it should return an error status code
        Assert.True(
          acceptResponse.StatusCode == HttpStatusCode.BadRequest ||
          acceptResponse.StatusCode == HttpStatusCode.NotFound,
          $"Expected 400 or 404, got {acceptResponse.StatusCode}"
        );
      }
    }
    finally
    {
      // Cleanup test doctors
      CleanupTestDoctorsFromDatabase(doctorAId, doctorBId);
    }
  }

  [Fact]
  public async Task HandoverLifecycle_Cannot_Complete_Before_Accept()
  {
    // ============================================================
    // ARRANGE: Setup test data
    // ============================================================

    const string doctorAId = "user_doctorA323456789012345678901";
    const string doctorBId = "user_doctorB323456789012345678901";

    // Cleanup any existing handovers from previous test runs
    CleanupTestDoctorsFromDatabase(doctorAId, doctorBId);
    
    // Create test doctors in the database
    CreateTestDoctorsInDatabase(doctorAId, doctorBId);

    try
    {
      // Get test data (shifts and patient)
      var (fromShiftId, toShiftId, testPatientId) = await GetTestDataAsync();

    // ============================================================
    // ACT: Create and start handover, try to complete without accepting
    // ============================================================

    var createRequest = new CreateHandoverRequestDto
    {
      PatientId = testPatientId,
      FromDoctorId = doctorAId,
      ToDoctorId = doctorBId,
      FromShiftId = fromShiftId,
      ToShiftId = toShiftId,
      InitiatedBy = doctorAId,
      Notes = "E2E Test: Try to complete without accepting"
    };

    var createResponse = await _client.PostAsJsonAsync("/handovers", createRequest);
    createResponse.EnsureSuccessStatusCode();
    var createdHandover = await createResponse.Content.ReadFromJsonAsync<CreateHandoverResponse>();
    Assert.NotNull(createdHandover);
    var handoverId = createdHandover.Id;

    // Ready the handover
    var readyResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/ready", new { Id = handoverId });
    readyResponse.EnsureSuccessStatusCode();

    // Start the handover
    var startRequest = new StartHandoverRequest { HandoverId = handoverId };
    var startResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/start", startRequest);
    startResponse.EnsureSuccessStatusCode();

    // Try to complete handover without accepting it first
    var completeRequest = new CompleteHandoverRequest { HandoverId = handoverId };
    var completeResponse = await _client.PostAsJsonAsync($"/handovers/{handoverId}/complete", completeRequest);
    
      // ============================================================
      // ASSERT: Should fail or return false
      // ============================================================
      if (completeResponse.StatusCode == HttpStatusCode.OK)
      {
        var completeResult = await completeResponse.Content.ReadFromJsonAsync<CompleteHandoverResponse>();
        Assert.NotNull(completeResult);
        // If it returns OK, success should be false
        Assert.False(completeResult.Success);
      }
      else
      {
        // Or it should return an error status code
        Assert.True(
          completeResponse.StatusCode == HttpStatusCode.BadRequest ||
          completeResponse.StatusCode == HttpStatusCode.NotFound,
          $"Expected 400 or 404, got {completeResponse.StatusCode}"
        );
      }
    }
    finally
    {
      // Cleanup test doctors
      CleanupTestDoctorsFromDatabase(doctorAId, doctorBId);
    }
  }

  // ============================================================
  // Helper Methods
  // ============================================================

  private async Task<(string fromShiftId, string toShiftId, string patientId)> GetTestDataAsync()
  {
    // Get units
    var unitsResponse = await _client.GetAsync("/setup/units");
    unitsResponse.EnsureSuccessStatusCode();
    var units = await unitsResponse.Content.ReadFromJsonAsync<UnitsResponse>();
    Assert.NotNull(units);
    Assert.NotEmpty(units.Units);

    // Get shifts
    var shiftsResponse = await _client.GetAsync("/setup/shifts");
    shiftsResponse.EnsureSuccessStatusCode();
    var shifts = await shiftsResponse.Content.ReadFromJsonAsync<ShiftsResponse>();
    Assert.NotNull(shifts);
    Assert.NotEmpty(shifts.Shifts);
    var fromShiftId = shifts.Shifts[0].Id;
    var toShiftId = shifts.Shifts.Count > 1 ? shifts.Shifts[1].Id : shifts.Shifts[0].Id;

    // Find a unit with patients
    string? testPatientId = null;
    foreach (var unit in units.Units)
    {
      var patientsResponse = await _client.GetAsync($"/units/{unit.Id}/patients?page=1&pageSize=1");
      if (patientsResponse.IsSuccessStatusCode)
      {
        var patientsData = await patientsResponse.Content.ReadFromJsonAsync<PatientsResponse>();
        if (patientsData?.Patients != null && patientsData.Patients.Count > 0)
        {
          testPatientId = patientsData.Patients[0].Id;
          break;
        }
      }
    }
    
    Assert.NotNull(testPatientId); // Ensure we found a patient

    return (fromShiftId, toShiftId, testPatientId);
  }

  /// <summary>
  /// Creates test doctor users directly in the database for E2E testing.
  /// Uses Dapper to insert test records that satisfy foreign key constraints.
  /// </summary>
  private void CreateTestDoctorsInDatabase(string doctorAId, string doctorBId)
  {
    using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(
      "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15");
    connection.Open();

    // Insert Doctor A
    connection.Execute(@"
      MERGE INTO USERS u
      USING (SELECT :Id AS ID FROM DUAL) src
      ON (u.ID = src.ID)
      WHEN NOT MATCHED THEN
        INSERT (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, CREATED_AT, UPDATED_AT)
        VALUES (:Id, :Email, :FirstName, :LastName, :FullName, SYSTIMESTAMP, SYSTIMESTAMP)",
      new
      {
        Id = doctorAId,
        Email = "doctorA@e2etest.com",
        FirstName = "Doctor",
        LastName = "A",
        FullName = "Doctor A"
      });

    // Insert Doctor B
    connection.Execute(@"
      MERGE INTO USERS u
      USING (SELECT :Id AS ID FROM DUAL) src
      ON (u.ID = src.ID)
      WHEN NOT MATCHED THEN
        INSERT (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, CREATED_AT, UPDATED_AT)
        VALUES (:Id, :Email, :FirstName, :LastName, :FullName, SYSTIMESTAMP, SYSTIMESTAMP)",
      new
      {
        Id = doctorBId,
        Email = "doctorB@e2etest.com",
        FirstName = "Doctor",
        LastName = "B",
        FullName = "Doctor B"
      });
  }

  /// <summary>
  /// Cleans up test doctor users from the database after test completion.
  /// Best-effort cleanup to keep test database clean.
  /// Deletes all handovers and related data for test doctors.
  /// </summary>
  private void CleanupTestDoctorsFromDatabase(string doctorAId, string doctorBId)
  {
    try
    {
      using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(
        "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15");
      connection.Open();

      // Delete all handover-related records in dependency order for test doctors
      // This ensures a completely clean state for each test
      // Each delete is wrapped in try-catch to handle missing tables gracefully
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

      // Delete test doctors
      try
      {
        connection.Execute("DELETE FROM USERS WHERE ID IN (:DoctorAId, :DoctorBId) AND EMAIL LIKE '%@e2etest.com'", new { DoctorAId = doctorAId, DoctorBId = doctorBId });
      }
      catch (Oracle.ManagedDataAccess.Client.OracleException) { /* Table doesn't exist, skip */ }
    }
    catch (Exception)
    {
      // Silently ignore connection errors during cleanup
    }
  }

  // ============================================================
  // Helper DTOs for test responses
  // ============================================================

  private class UnitsResponse
  {
    public List<UnitDto> Units { get; set; } = new();
  }

  private class UnitDto
  {
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
  }

  private class ShiftsResponse
  {
    public List<ShiftDto> Shifts { get; set; } = new();
  }

  private class ShiftDto
  {
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
  }

  private class PatientsResponse
  {
    public List<PatientDto> Patients { get; set; } = new();
    public int TotalCount { get; set; }
  }

  private class PatientDto
  {
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
  }

  // Local copy of GetHandoverByIdResponse for testing
  private class GetHandoverByIdResponse
  {
    public string Id { get; set; } = string.Empty;
    public string AssignmentId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string? PatientName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ResponsiblePhysicianId { get; set; } = string.Empty;
    public string ResponsiblePhysicianName { get; set; } = string.Empty;
    public string ShiftName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string? ReceiverUserId { get; set; }
    public string? CreatedAt { get; set; }
    public string? ReadyAt { get; set; }
    public string? StartedAt { get; set; }
    public string? AcknowledgedAt { get; set; }
    public string? AcceptedAt { get; set; }
    public string? CompletedAt { get; set; }
    public string? CancelledAt { get; set; }
    public string? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? ExpiredAt { get; set; }
    public string? HandoverType { get; set; }
    public string? HandoverWindowDate { get; set; }
    public string? FromShiftId { get; set; }
    public string? ToShiftId { get; set; }
    public string? ToDoctorId { get; set; }
    public string StateName { get; set; } = string.Empty;
  }

  // Local copy of GetPatientHandoverDataResponse for testing
  private class GetPatientHandoverDataResponse
  {
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string dob { get; set; } = string.Empty;
    public string mrn { get; set; } = string.Empty;
  }
}

