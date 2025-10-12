using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Relevo.Infrastructure.Data.Oracle;
using Relevo.Infrastructure.BackgroundJobs;
using Xunit;

namespace Relevo.IntegrationTests.BackgroundJobs;

/// <summary>
/// Integration tests for the ExpireHandoversJob background job.
/// Tests verify that old handovers are correctly marked as expired.
/// </summary>
[Collection("Sequential")]
public class ExpireHandoversJobTests : IDisposable
{
    private readonly IDbConnection _connection;
    private readonly ExpireHandoversJob _job;
    private readonly List<string> _testHandoverIds = new();

    public ExpireHandoversJobTests()
    {
        const string connectionString = "User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15";
        
        _connection = new OracleConnection(connectionString);
        _connection.Open();

        // Create mock configuration
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Oracle:ConnectionString"] = connectionString
        });
        var configuration = configBuilder.Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var factoryLogger = loggerFactory.CreateLogger<OracleConnectionFactory>();
        var factory = new OracleConnectionFactory(configuration, factoryLogger);
        
        var logger = loggerFactory.CreateLogger<ExpireHandoversJob>();
        _job = new ExpireHandoversJob(factory, logger);
    }

    [Fact]
    public async Task Expires_Old_Draft_Handovers()
    {
        // Arrange: Create handover with old window date (3 days ago)
        var handoverId = await CreateHandoverAsync(
            windowDateDaysAgo: 3,
            status: "Draft",
            readyAt: null,
            startedAt: null,
            acceptedAt: null);

        // Act: Run expiration job
        var expiredCount = await _job.ExecuteAsync(CancellationToken.None);

        // Assert: Should have expired at least one handover (ours)
        Assert.True(expiredCount >= 1, $"Expected at least 1 expired, got {expiredCount}");

        // Verify our handover is now expired
        var handover = await GetHandoverAsync(handoverId);
        Assert.NotNull(handover.ExpiredAt);
        Assert.Equal("Expired", handover.StateName);
        Assert.Null(handover.CompletedAt);
        Assert.Null(handover.CancelledAt);
        Assert.Null(handover.RejectedAt);
    }

    [Fact]
    public async Task Expires_Old_Ready_Handovers()
    {
        // Arrange: Create ready handover with old window date (2 days ago)
        var handoverId = await CreateHandoverAsync(
            windowDateDaysAgo: 2,
            status: "Ready",
            readyAt: DateTime.UtcNow.AddDays(-2),
            startedAt: null,
            acceptedAt: null);

        // Act: Run expiration job
        var expiredCount = await _job.ExecuteAsync(CancellationToken.None);

        // Assert: Should have expired
        Assert.True(expiredCount >= 1);

        var handover = await GetHandoverAsync(handoverId);
        Assert.NotNull(handover.ExpiredAt);
        Assert.Equal("Expired", handover.StateName);
    }

    [Fact]
    public async Task Does_Not_Expire_Accepted_Handovers()
    {
        // Arrange: Create and accept handover with old window date (3 days ago)
        var handoverId = await CreateHandoverAsync(
            windowDateDaysAgo: 3,
            status: "Accepted",
            readyAt: DateTime.UtcNow.AddDays(-3),
            startedAt: DateTime.UtcNow.AddDays(-3),
            acceptedAt: DateTime.UtcNow.AddDays(-3));

        // Act: Run expiration job
        await _job.ExecuteAsync(CancellationToken.None);

        // Assert: Should NOT be expired (accepted handovers are protected)
        var handover = await GetHandoverAsync(handoverId);
        Assert.Null(handover.ExpiredAt);
        Assert.Equal("Accepted", handover.StateName);
    }

    [Fact]
    public async Task Does_Not_Expire_InProgress_Handovers()
    {
        // Arrange: Create in-progress handover with old window date
        var handoverId = await CreateHandoverAsync(
            windowDateDaysAgo: 2,
            status: "InProgress",
            readyAt: DateTime.UtcNow.AddDays(-2),
            startedAt: DateTime.UtcNow.AddDays(-2),
            acceptedAt: null);

        // Act: Run expiration job
        await _job.ExecuteAsync(CancellationToken.None);

        // Assert: Should NOT be expired (in-progress handovers are protected)
        var handover = await GetHandoverAsync(handoverId);
        Assert.Null(handover.ExpiredAt);
        Assert.Equal("InProgress", handover.StateName);
    }

    [Fact]
    public async Task Does_Not_Expire_Completed_Handovers()
    {
        // Arrange: Create completed handover with old window date
        var handoverId = await CreateHandoverAsync(
            windowDateDaysAgo: 5,
            status: "Completed",
            readyAt: DateTime.UtcNow.AddDays(-5),
            startedAt: DateTime.UtcNow.AddDays(-5),
            acceptedAt: DateTime.UtcNow.AddDays(-5),
            completedAt: DateTime.UtcNow.AddDays(-4));

        // Act: Run expiration job
        await _job.ExecuteAsync(CancellationToken.None);

        // Assert: Should NOT be expired (already in terminal state)
        var handover = await GetHandoverAsync(handoverId);
        Assert.Null(handover.ExpiredAt);
        Assert.Equal("Completed", handover.StateName);
    }

    [Fact]
    public async Task Does_Not_Expire_Recent_Draft_Handovers()
    {
        // Arrange: Create handover with today's window date
        var handoverId = await CreateHandoverAsync(
            windowDateDaysAgo: 0,
            status: "Draft",
            readyAt: null,
            startedAt: null,
            acceptedAt: null);

        // Act: Run expiration job
        await _job.ExecuteAsync(CancellationToken.None);

        // Assert: Should NOT be expired (still within window)
        var handover = await GetHandoverAsync(handoverId);
        Assert.Null(handover.ExpiredAt);
        Assert.Equal("Draft", handover.StateName);
    }

    [Fact]
    public async Task Idempotent_Does_Not_Re_Expire_Already_Expired()
    {
        // Arrange: Create expired handover
        var handoverId = await CreateHandoverAsync(
            windowDateDaysAgo: 3,
            status: "Expired",
            readyAt: null,
            startedAt: null,
            acceptedAt: null,
            expiredAt: DateTime.UtcNow.AddDays(-1));

        var firstExpiredAt = (await GetHandoverAsync(handoverId)).ExpiredAt;

        // Act: Run expiration job
        await _job.ExecuteAsync(CancellationToken.None);

        // Assert: Expired timestamp should NOT change
        var handover = await GetHandoverAsync(handoverId);
        Assert.Equal(firstExpiredAt, handover.ExpiredAt);
    }

    #region Helper Methods

    private async Task<string> CreateHandoverAsync(
        int windowDateDaysAgo,
        string status,
        DateTime? readyAt = null,
        DateTime? startedAt = null,
        DateTime? acceptedAt = null,
        DateTime? completedAt = null,
        DateTime? expiredAt = null)
    {
        var handoverId = "h_exp_test_" + Guid.NewGuid().ToString("N")[..8];
        _testHandoverIds.Add(handoverId);

        // Ensure unique window tuple for UQ_ACTIVE_HANDOVER_WINDOW: (PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, HANDOVER_WINDOW_DATE)
        // Create unique patient to avoid collisions with seeded data
        var assignmentId = $"assign-{Guid.NewGuid():N}";
        var patientId = await CreateTestPatientAsync();
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

        // Calculate window date as a DATE (not TIMESTAMP) in the past
        var windowDate = DateTime.Today.AddDays(-windowDateDaysAgo);

        // Create user assignment
        await _connection.ExecuteAsync(@"
            MERGE INTO USER_ASSIGNMENTS ua
            USING (SELECT :assignmentId AS ASSIGNMENT_ID FROM DUAL) src
            ON (ua.ASSIGNMENT_ID = src.ASSIGNMENT_ID)
            WHEN NOT MATCHED THEN
              INSERT (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
              VALUES (:assignmentId, :userId, :shiftId, :patientId, SYSTIMESTAMP)",
            new { assignmentId, userId = doctorId, shiftId = fromShiftId, patientId });

        // Create handover with specified timestamps
        await _connection.ExecuteAsync(@"
            INSERT INTO HANDOVERS (
                ID, ASSIGNMENT_ID, PATIENT_ID, STATUS,
                FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID,
                CREATED_BY, CREATED_AT, HANDOVER_TYPE, HANDOVER_WINDOW_DATE,
                READY_AT, STARTED_AT, ACCEPTED_AT, COMPLETED_AT, EXPIRED_AT
            ) VALUES (
                :id, :assignmentId, :patientId, :status,
                :fromShiftId, :toShiftId, :doctorId, :doctorId,
                :doctorId, SYSTIMESTAMP, 'ShiftToShift', TO_DATE(:windowDate, 'YYYY-MM-DD'),
                :readyAt, :startedAt, :acceptedAt, :completedAt, :expiredAt
            )",
            new
            {
                id = handoverId,
                assignmentId,
                patientId,
                status,
                fromShiftId,
                toShiftId,
                doctorId,
                windowDate = windowDate.ToString("yyyy-MM-dd"),
                readyAt,
                startedAt,
                acceptedAt,
                completedAt,
                expiredAt
            });

        return handoverId;
    }

    private async Task<HandoverDto> GetHandoverAsync(string handoverId)
    {
        var handover = await _connection.QuerySingleAsync<HandoverDto>(@"
            SELECT h.ID, h.STATUS, h.EXPIRED_AT, h.COMPLETED_AT, h.CANCELLED_AT, h.REJECTED_AT,
                   s.StateName
            FROM HANDOVERS h
            LEFT JOIN VW_HANDOVERS_STATE s ON h.ID = s.HandoverId
            WHERE h.ID = :id",
            new { id = handoverId });

        return handover;
    }

    private async Task<string> CreateTestPatientAsync()
    {
        var patientId = $"pat_test_{Guid.NewGuid():N}";
        // Ensure at least one unit exists and pick one
        var unitId = await _connection.QueryFirstOrDefaultAsync<string>("SELECT ID FROM UNITS WHERE ROWNUM = 1");
        if (string.IsNullOrEmpty(unitId))
        {
            // Create a fallback unit
            unitId = "unit_test";
            await _connection.ExecuteAsync(@"INSERT INTO UNITS (ID, NAME, DESCRIPTION, CREATED_AT, UPDATED_AT)
                                             VALUES (:Id, 'Test Unit', 'Integration Test Unit', SYSTIMESTAMP, SYSTIMESTAMP)",
                                             new { Id = unitId });
        }

        await _connection.ExecuteAsync(@"INSERT INTO PATIENTS (
                                            ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT)
                                          VALUES (
                                            :Id, 'Integration Test Patient', :UnitId, TO_DATE('2010-01-01','YYYY-MM-DD'), 'Unknown', SYSTIMESTAMP, '999', 'Test', SYSTIMESTAMP, SYSTIMESTAMP)",
                                          new { Id = patientId, UnitId = unitId });

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

    private class HandoverDto
    {
        public string ID { get; set; } = string.Empty;
        public string STATUS { get; set; } = string.Empty;
        public DateTime? EXPIRED_AT { get; set; }
        public DateTime? COMPLETED_AT { get; set; }
        public DateTime? CANCELLED_AT { get; set; }
        public DateTime? REJECTED_AT { get; set; }
        public string StateName { get; set; } = string.Empty;

        public string? ExpiredAt => EXPIRED_AT?.ToString("yyyy-MM-dd HH:mm:ss");
        public string? CompletedAt => COMPLETED_AT?.ToString("yyyy-MM-dd HH:mm:ss");
        public string? CancelledAt => CANCELLED_AT?.ToString("yyyy-MM-dd HH:mm:ss");
        public string? RejectedAt => REJECTED_AT?.ToString("yyyy-MM-dd HH:mm:ss");
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

                _connection.Execute("DELETE FROM PATIENTS WHERE ID LIKE 'pat_test_%'");
                _connection.Execute("DELETE FROM UNITS WHERE ID = 'unit_test'");
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        _connection?.Dispose();
    }
}

