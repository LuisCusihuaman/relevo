using System.Data;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace Relevo.IntegrationTests.Database;

/// <summary>
/// Integration tests to verify database constraints enforce handover state machine integrity.
/// These tests directly manipulate the database to ensure constraints prevent invalid state transitions.
/// </summary>
[Collection("Sequential")]
public class HandoverConstraintsTests : IDisposable
{
    private readonly IDbConnection _connection;
    private readonly List<string> _testHandoverIds = new();

    public HandoverConstraintsTests()
    {
        // Create direct Oracle connection for integration testing
        _connection = new OracleConnection(
            "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15");
        _connection.Open();
    }

    [Fact]
    public async Task Cannot_Complete_Without_Accept()
    {
        // Arrange: Create handover with all required timestamps EXCEPT ACCEPTED_AT
        var handoverId = $"test-constraint-{Guid.NewGuid():N}";
        _testHandoverIds.Add(handoverId);

        var assignmentId = $"assign-{Guid.NewGuid():N}";
        var patientId = await GetTestPatientIdAsync();
        var (fromShiftId, toShiftId) = await GetTestShiftIdsAsync();
        const string doctorId = "user_test123456789012345678901234567";

        // Ensure user exists
        await _connection.ExecuteAsync(@"
            MERGE INTO USERS u
            USING (SELECT :userId AS ID FROM DUAL) src
            ON (u.ID = src.ID)
            WHEN NOT MATCHED THEN
              INSERT (ID, FULL_NAME, EMAIL)
              VALUES (:userId, 'Test Doctor', 'test@example.com')",
            new { userId = doctorId });

        // Create valid user assignment first
        await _connection.ExecuteAsync(@"
            MERGE INTO USER_ASSIGNMENTS ua
            USING (SELECT :assignmentId AS ASSIGNMENT_ID FROM DUAL) src
            ON (ua.ASSIGNMENT_ID = src.ASSIGNMENT_ID)
            WHEN NOT MATCHED THEN
              INSERT (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
              VALUES (:assignmentId, :userId, :shiftId, :patientId, SYSTIMESTAMP)",
            new { assignmentId, userId = doctorId, shiftId = fromShiftId, patientId });

        // Act & Assert: Try to insert handover with COMPLETED_AT but no ACCEPTED_AT
        var exception = await Assert.ThrowsAsync<OracleException>(async () =>
            await _connection.ExecuteAsync(@"
                INSERT INTO HANDOVERS (
                    ID, ASSIGNMENT_ID, PATIENT_ID, STATUS,
                    FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID,
                    CREATED_BY, CREATED_AT, HANDOVER_TYPE, HANDOVER_WINDOW_DATE,
                    READY_AT, STARTED_AT, COMPLETED_AT
                ) VALUES (
                    :id, :assignmentId, :patientId, 'Completed',
                    :fromShiftId, :toShiftId, :doctorId, :doctorId,
                    :doctorId, SYSTIMESTAMP, 'ShiftToShift', SYSTIMESTAMP,
                    SYSTIMESTAMP, SYSTIMESTAMP, SYSTIMESTAMP
                )",
                new
                {
                    id = handoverId,
                    assignmentId,
                    patientId,
                    fromShiftId,
                    toShiftId,
                    doctorId
                })
        );

        // Should violate CHK_COMPLETED_REQ_ACCEPTED constraint
        Assert.Contains("CHK_COMPLETED_REQ_ACCEPTED", exception.Message);
    }

    [Fact]
    public async Task Cannot_Accept_Without_Start()
    {
        // Arrange: Create handover with ACCEPTED_AT but no STARTED_AT
        var handoverId = $"test-constraint-{Guid.NewGuid():N}";
        _testHandoverIds.Add(handoverId);

        var assignmentId = $"assign-{Guid.NewGuid():N}";
        var patientId = await GetTestPatientIdAsync();
        var (fromShiftId, toShiftId) = await GetTestShiftIdsAsync();
        const string doctorId = "user_test123456789012345678901234567";

        // Ensure user exists
        await _connection.ExecuteAsync(@"
            MERGE INTO USERS u
            USING (SELECT :userId AS ID FROM DUAL) src
            ON (u.ID = src.ID)
            WHEN NOT MATCHED THEN
              INSERT (ID, FULL_NAME, EMAIL)
              VALUES (:userId, 'Test Doctor', 'test@example.com')",
            new { userId = doctorId });

        await _connection.ExecuteAsync(@"
            MERGE INTO USER_ASSIGNMENTS ua
            USING (SELECT :assignmentId AS ASSIGNMENT_ID FROM DUAL) src
            ON (ua.ASSIGNMENT_ID = src.ASSIGNMENT_ID)
            WHEN NOT MATCHED THEN
              INSERT (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
              VALUES (:assignmentId, :userId, :shiftId, :patientId, SYSTIMESTAMP)",
            new { assignmentId, userId = doctorId, shiftId = fromShiftId, patientId });

        // Act & Assert: Try to insert handover with ACCEPTED_AT but no STARTED_AT
        var exception = await Assert.ThrowsAsync<OracleException>(async () =>
            await _connection.ExecuteAsync(@"
                INSERT INTO HANDOVERS (
                    ID, ASSIGNMENT_ID, PATIENT_ID, STATUS,
                    FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID,
                    CREATED_BY, CREATED_AT, HANDOVER_TYPE, HANDOVER_WINDOW_DATE,
                    READY_AT, ACCEPTED_AT
                ) VALUES (
                    :id, :assignmentId, :patientId, 'Accepted',
                    :fromShiftId, :toShiftId, :doctorId, :doctorId,
                    :doctorId, SYSTIMESTAMP, 'ShiftToShift', SYSTIMESTAMP,
                    SYSTIMESTAMP, SYSTIMESTAMP
                )",
                new
                {
                    id = handoverId,
                    assignmentId,
                    patientId,
                    fromShiftId,
                    toShiftId,
                    doctorId
                })
        );

        // Should violate CHK_ACCEPTED_REQ_STARTED constraint
        Assert.Contains("CHK_ACCEPTED_REQ_STARTED", exception.Message);
    }

    [Fact]
    public async Task Cannot_Start_Without_Ready()
    {
        // Arrange: Create handover with STARTED_AT but no READY_AT
        var handoverId = $"test-constraint-{Guid.NewGuid():N}";
        _testHandoverIds.Add(handoverId);

        var assignmentId = $"assign-{Guid.NewGuid():N}";
        var patientId = await GetTestPatientIdAsync();
        var (fromShiftId, toShiftId) = await GetTestShiftIdsAsync();
        const string doctorId = "user_test123456789012345678901234567";

        // Ensure user exists
        await _connection.ExecuteAsync(@"
            MERGE INTO USERS u
            USING (SELECT :userId AS ID FROM DUAL) src
            ON (u.ID = src.ID)
            WHEN NOT MATCHED THEN
              INSERT (ID, FULL_NAME, EMAIL)
              VALUES (:userId, 'Test Doctor', 'test@example.com')",
            new { userId = doctorId });

        await _connection.ExecuteAsync(@"
            MERGE INTO USER_ASSIGNMENTS ua
            USING (SELECT :assignmentId AS ASSIGNMENT_ID FROM DUAL) src
            ON (ua.ASSIGNMENT_ID = src.ASSIGNMENT_ID)
            WHEN NOT MATCHED THEN
              INSERT (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
              VALUES (:assignmentId, :userId, :shiftId, :patientId, SYSTIMESTAMP)",
            new { assignmentId, userId = doctorId, shiftId = fromShiftId, patientId });

        // Act & Assert: Try to insert handover with STARTED_AT but no READY_AT
        var exception = await Assert.ThrowsAsync<OracleException>(async () =>
            await _connection.ExecuteAsync(@"
                INSERT INTO HANDOVERS (
                    ID, ASSIGNMENT_ID, PATIENT_ID, STATUS,
                    FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID,
                    CREATED_BY, CREATED_AT, HANDOVER_TYPE, HANDOVER_WINDOW_DATE,
                    STARTED_AT
                ) VALUES (
                    :id, :assignmentId, :patientId, 'InProgress',
                    :fromShiftId, :toShiftId, :doctorId, :doctorId,
                    :doctorId, SYSTIMESTAMP, 'ShiftToShift', SYSTIMESTAMP,
                    SYSTIMESTAMP
                )",
                new
                {
                    id = handoverId,
                    assignmentId,
                    patientId,
                    fromShiftId,
                    toShiftId,
                    doctorId
                })
        );

        // Should violate CHK_STARTED_REQUIRES_READY constraint
        Assert.Contains("CHK_STARTED_REQUIRES_READY", exception.Message);
    }

    [Fact]
    public async Task Cannot_Have_Multiple_Terminal_States()
    {
        // Arrange: Create a completed handover
        var handoverId = await CreateCompletedHandoverAsync();
        _testHandoverIds.Add(handoverId);

        // Act & Assert: Try to also mark it as cancelled
        var exception = await Assert.ThrowsAsync<OracleException>(async () =>
            await _connection.ExecuteAsync(@"
                UPDATE HANDOVERS 
                SET CANCELLED_AT = SYSTIMESTAMP 
                WHERE ID = :id",
                new { id = handoverId })
        );

        // Should violate CHK_SINGLE_TERMINAL_STATE constraint
        Assert.Contains("CHK_SINGLE_TERMINAL_STATE", exception.Message);
    }

    [Fact]
    public async Task Cannot_Have_Completed_And_Rejected()
    {
        // Arrange: Create a completed handover
        var handoverId = await CreateCompletedHandoverAsync();
        _testHandoverIds.Add(handoverId);

        // Act & Assert: Try to also mark it as rejected
        var exception = await Assert.ThrowsAsync<OracleException>(async () =>
            await _connection.ExecuteAsync(@"
                UPDATE HANDOVERS 
                SET REJECTED_AT = SYSTIMESTAMP,
                    REJECTION_REASON = 'Test rejection'
                WHERE ID = :id",
                new { id = handoverId })
        );

        // Should violate CHK_SINGLE_TERMINAL_STATE constraint
        Assert.Contains("CHK_SINGLE_TERMINAL_STATE", exception.Message);
    }

    [Fact]
    public async Task Cannot_Have_Cancelled_And_Expired()
    {
        // Arrange: Create a cancelled handover
        var handoverId = await CreateCancelledHandoverAsync();
        _testHandoverIds.Add(handoverId);

        // Act & Assert: Try to also mark it as expired
        var exception = await Assert.ThrowsAsync<OracleException>(async () =>
            await _connection.ExecuteAsync(@"
                UPDATE HANDOVERS 
                SET EXPIRED_AT = SYSTIMESTAMP 
                WHERE ID = :id",
                new { id = handoverId })
        );

        // Should violate CHK_SINGLE_TERMINAL_STATE constraint
        Assert.Contains("CHK_SINGLE_TERMINAL_STATE", exception.Message);
    }

    #region Helper Methods

    private async Task<string> CreateCompletedHandoverAsync()
    {
        var handoverId = $"test-completed-{Guid.NewGuid():N}";
        var assignmentId = $"assign-{Guid.NewGuid():N}";
        var patientId = await GetTestPatientIdAsync();
        var (fromShiftId, toShiftId) = await GetTestShiftIdsAsync();
        const string doctorId = "user_test123456789012345678901234567";

        // Ensure user exists
        await _connection.ExecuteAsync(@"
            MERGE INTO USERS u
            USING (SELECT :userId AS ID FROM DUAL) src
            ON (u.ID = src.ID)
            WHEN NOT MATCHED THEN
              INSERT (ID, FULL_NAME, EMAIL)
              VALUES (:userId, 'Test Doctor', 'test@example.com')",
            new { userId = doctorId });

        // Create user assignment
        await _connection.ExecuteAsync(@"
            MERGE INTO USER_ASSIGNMENTS ua
            USING (SELECT :assignmentId AS ASSIGNMENT_ID FROM DUAL) src
            ON (ua.ASSIGNMENT_ID = src.ASSIGNMENT_ID)
            WHEN NOT MATCHED THEN
              INSERT (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
              VALUES (:assignmentId, :userId, :shiftId, :patientId, SYSTIMESTAMP)",
            new { assignmentId, userId = doctorId, shiftId = fromShiftId, patientId });

        // Create completed handover (with proper state progression)
        await _connection.ExecuteAsync(@"
            INSERT INTO HANDOVERS (
                ID, ASSIGNMENT_ID, PATIENT_ID, STATUS,
                FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID,
                CREATED_BY, CREATED_AT, HANDOVER_TYPE, HANDOVER_WINDOW_DATE,
                READY_AT, STARTED_AT, ACCEPTED_AT, COMPLETED_AT
            ) VALUES (
                :id, :assignmentId, :patientId, 'Completed',
                :fromShiftId, :toShiftId, :doctorId, :doctorId,
                :doctorId, SYSTIMESTAMP, 'ShiftToShift', SYSTIMESTAMP,
                SYSTIMESTAMP, SYSTIMESTAMP, SYSTIMESTAMP, SYSTIMESTAMP
            )",
            new
            {
                id = handoverId,
                assignmentId,
                patientId,
                fromShiftId,
                toShiftId,
                doctorId
            });

        return handoverId;
    }

    private async Task<string> CreateCancelledHandoverAsync()
    {
        var handoverId = $"test-cancelled-{Guid.NewGuid():N}";
        var assignmentId = $"assign-{Guid.NewGuid():N}";
        var patientId = await GetTestPatientIdAsync();
        var (fromShiftId, toShiftId) = await GetTestShiftIdsAsync();
        const string doctorId = "user_test123456789012345678901234567";

        // Ensure user exists
        await _connection.ExecuteAsync(@"
            MERGE INTO USERS u
            USING (SELECT :userId AS ID FROM DUAL) src
            ON (u.ID = src.ID)
            WHEN NOT MATCHED THEN
              INSERT (ID, FULL_NAME, EMAIL)
              VALUES (:userId, 'Test Doctor', 'test@example.com')",
            new { userId = doctorId });

        // Create user assignment
        await _connection.ExecuteAsync(@"
            MERGE INTO USER_ASSIGNMENTS ua
            USING (SELECT :assignmentId AS ASSIGNMENT_ID FROM DUAL) src
            ON (ua.ASSIGNMENT_ID = src.ASSIGNMENT_ID)
            WHEN NOT MATCHED THEN
              INSERT (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
              VALUES (:assignmentId, :userId, :shiftId, :patientId, SYSTIMESTAMP)",
            new { assignmentId, userId = doctorId, shiftId = fromShiftId, patientId });

        // Create cancelled handover
        await _connection.ExecuteAsync(@"
            INSERT INTO HANDOVERS (
                ID, ASSIGNMENT_ID, PATIENT_ID, STATUS,
                FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID,
                CREATED_BY, CREATED_AT, HANDOVER_TYPE, HANDOVER_WINDOW_DATE,
                CANCELLED_AT
            ) VALUES (
                :id, :assignmentId, :patientId, 'Cancelled',
                :fromShiftId, :toShiftId, :doctorId, :doctorId,
                :doctorId, SYSTIMESTAMP, 'ShiftToShift', SYSTIMESTAMP,
                SYSTIMESTAMP
            )",
            new
            {
                id = handoverId,
                assignmentId,
                patientId,
                fromShiftId,
                toShiftId,
                doctorId
            });

        return handoverId;
    }

    private async Task<string> GetTestPatientIdAsync()
    {
        var patientId = await _connection.QueryFirstOrDefaultAsync<string>(
            "SELECT ID FROM PATIENTS WHERE ROWNUM = 1");
        
        if (string.IsNullOrEmpty(patientId))
        {
            throw new InvalidOperationException("No test patients found in database");
        }

        return patientId;
    }

    private async Task<(string fromShiftId, string toShiftId)> GetTestShiftIdsAsync()
    {
        var shifts = (await _connection.QueryAsync<string>(
            "SELECT ID FROM SHIFTS WHERE ROWNUM <= 2 ORDER BY ID")).ToList();
        
        if (shifts.Count < 2)
        {
            throw new InvalidOperationException("Not enough shifts found in database");
        }

        return (shifts[0], shifts[1]);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup test handovers
        if (_testHandoverIds.Any())
        {
            try
            {
                _connection.Execute(
                    "DELETE FROM HANDOVERS WHERE ID IN (" +
                    string.Join(",", _testHandoverIds.Select((_, i) => $":id{i}")) + ")",
                    _testHandoverIds.Select((id, i) => new KeyValuePair<string, object>($"id{i}", id))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

                _connection.Execute(
                    "DELETE FROM USER_ASSIGNMENTS WHERE ASSIGNMENT_ID LIKE 'assign-%'");
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        _connection?.Dispose();
    }
}

