using Microsoft.Extensions.DependencyInjection;
using Relevo.Core.Events;
using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Relevo.Infrastructure.Data;
using Xunit;
using Dapper;
using System.Data;

namespace Relevo.IntegrationTests.Data;

public abstract class BaseDapperRepoTestFixture : IClassFixture<CustomWebApplicationFactory<Program>>
{
    protected readonly CustomWebApplicationFactory<Program> _factory;
    protected readonly IServiceScope _scope;

    protected BaseDapperRepoTestFixture(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _scope = _factory.Services.CreateScope();
    }

    protected ContributorRepository GetRepository()
    {
        return (ContributorRepository)_scope.ServiceProvider.GetRequiredService<IContributorRepository>();
    }

    /// <summary>
    /// Creates a test patient in the database.
    /// </summary>
    protected async Task<string> CreateTestPatientAsync(string testRunId, string? unitId = null, string? patientId = null)
    {
        var connectionFactory = _scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
        using var conn = connectionFactory.CreateConnection();

        unitId ??= DapperTestSeeder.UnitId;
        patientId ??= $"pat-{testRunId}";

        await conn.ExecuteAsync(@"
            INSERT INTO PATIENTS (ID, NAME, UNIT_ID, CREATED_AT, UPDATED_AT)
            VALUES (:Id, :Name, :UnitId, SYSTIMESTAMP, SYSTIMESTAMP)",
            new { Id = patientId, Name = $"Test Patient {testRunId}", UnitId = unitId });

        return patientId;
    }

    /// <summary>
    /// Creates a test user in the database.
    /// </summary>
    protected async Task<string> CreateTestUserAsync(string testRunId, string? userId = null, string? email = null, string? firstName = null, string? lastName = null)
    {
        var connectionFactory = _scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
        using var conn = connectionFactory.CreateConnection();

        userId ??= $"user-{testRunId}";
        email ??= $"user{testRunId}@test.com";
        firstName ??= "Test";
        lastName ??= "User";

        await conn.ExecuteAsync(@"
            INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME)
            VALUES (:Id, :Email, :FirstName, :LastName, :FullName)",
            new { Id = userId, Email = email, FirstName = firstName, LastName = lastName, FullName = $"{firstName} {lastName}" });

        return userId;
    }

    /// <summary>
    /// Calculates shift instance dates for today using the shared calculation service.
    /// </summary>
    protected async Task<(string fromInstanceId, string toInstanceId)> CreateShiftInstancesForTodayAsync(
        string fromShiftId,
        string toShiftId,
        string unitId,
        DateTime? baseDate = null)
    {
        var connectionFactory = _scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
        using var conn = connectionFactory.CreateConnection();

        var baseDateValue = baseDate ?? DateTime.Today;
        var shiftDates = await ShiftInstanceCalculationService.CalculateShiftInstanceDatesFromDbAsync(
            conn, fromShiftId, toShiftId, baseDateValue);

        if (shiftDates == null)
        {
            throw new InvalidOperationException($"Shift templates not found: FromShiftId={fromShiftId}, ToShiftId={toShiftId}");
        }

        var (fromShiftStartAt, fromShiftEndAt, toShiftStartAt, toShiftEndAt) = shiftDates.Value;

        var shiftInstanceRepo = _scope.ServiceProvider.GetRequiredService<IShiftInstanceRepository>();
        var fromInstanceId = await shiftInstanceRepo.GetOrCreateShiftInstanceAsync(
            fromShiftId, unitId, fromShiftStartAt, fromShiftEndAt);
        var toInstanceId = await shiftInstanceRepo.GetOrCreateShiftInstanceAsync(
            toShiftId, unitId, toShiftStartAt, toShiftEndAt);

        return (fromInstanceId, toInstanceId);
    }

    /// <summary>
    /// Creates shift coverage for a patient.
    /// </summary>
    /// <param name="testRunId">Unique test run ID</param>
    /// <param name="userId">User ID for coverage</param>
    /// <param name="patientId">Patient ID</param>
    /// <param name="shiftInstanceId">Shift instance ID</param>
    /// <param name="unitId">Unit ID</param>
    /// <param name="isPrimary">Whether this is primary coverage</param>
    /// <param name="clearExisting">If true, deletes existing coverage for this patient/shift instance before inserting</param>
    protected async Task<string> CreateCoverageAsync(
        string testRunId,
        string userId,
        string patientId,
        string shiftInstanceId,
        string unitId,
        bool isPrimary = true,
        bool clearExisting = true)
    {
        var connectionFactory = _scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
        using var conn = connectionFactory.CreateConnection();

        var coverageId = $"sc-{testRunId}";

        // Delete any existing coverage first to avoid constraint violations (only if clearExisting is true)
        if (clearExisting)
        {
            await conn.ExecuteAsync(@"
                DELETE FROM SHIFT_COVERAGE 
                WHERE PATIENT_ID = :PatientId AND SHIFT_INSTANCE_ID = :ShiftInstanceId",
                new { PatientId = patientId, ShiftInstanceId = shiftInstanceId });
        }

        await conn.ExecuteAsync(@"
            INSERT INTO SHIFT_COVERAGE (ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, UNIT_ID, IS_PRIMARY, ASSIGNED_AT)
            VALUES (:Id, :UserId, :PatientId, :ShiftInstanceId, :UnitId, :IsPrimary, SYSTIMESTAMP)",
            new
            {
                Id = coverageId,
                UserId = userId,
                PatientId = patientId,
                ShiftInstanceId = shiftInstanceId,
                UnitId = unitId,
                IsPrimary = isPrimary ? 1 : 0
            });

        return coverageId;
    }

    /// <summary>
    /// Gets a connection factory for direct database access.
    /// </summary>
    protected DapperConnectionFactory GetConnectionFactory()
    {
        return _scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
    }

    /// <summary>
    /// Generates a unique date for tests to avoid conflicts.
    /// Uses testRunId to create deterministic but unique dates.
    /// </summary>
    protected DateTime GetUniqueTestDate(string testRunId, int additionalDaysOffset = 0)
    {
        if (string.IsNullOrEmpty(testRunId) || testRunId.Length < 2)
        {
            return DateTime.Today.AddDays(additionalDaysOffset);
        }

        var daysOffset = int.Parse(
            testRunId.Substring(0, Math.Min(2, testRunId.Length)), 
            System.Globalization.NumberStyles.HexNumber) % 365;
        
        return DateTime.Today.AddDays(-daysOffset + additionalDaysOffset);
    }

    /// <summary>
    /// Creates multiple coverages for testing sender selection logic.
    /// Clears existing coverage first, then inserts all coverages in order.
    /// Useful for testing scenarios where multiple non-primary coverages exist.
    /// </summary>
    /// <param name="testRunId">Unique test run ID</param>
    /// <param name="patientId">Patient ID</param>
    /// <param name="shiftInstanceId">Shift instance ID</param>
    /// <param name="unitId">Unit ID</param>
    /// <param name="coverages">Array of coverage definitions (userId, isPrimary, assignedAtOffset)</param>
    /// <returns>List of created coverage IDs</returns>
    protected async Task<List<string>> CreateMultipleCoveragesForTestAsync(
        string testRunId,
        string patientId,
        string shiftInstanceId,
        string unitId,
        params (string userId, bool isPrimary, TimeSpan? assignedAtOffset)[] coverages)
    {
        var connectionFactory = GetConnectionFactory();
        using var conn = connectionFactory.CreateConnection();

        // Clear all existing coverage first
        await conn.ExecuteAsync(@"
            DELETE FROM SHIFT_COVERAGE 
            WHERE PATIENT_ID = :PatientId AND SHIFT_INSTANCE_ID = :ShiftInstanceId",
            new { PatientId = patientId, ShiftInstanceId = shiftInstanceId });

        var coverageIds = new List<string>();

        for (int i = 0; i < coverages.Length; i++)
        {
            var (userId, isPrimary, offset) = coverages[i];
            var coverageId = $"sc-{testRunId}-{i}";
            
            // Build ASSIGNED_AT expression based on offset
            string assignedAtExpression;
            if (offset.HasValue && offset.Value.TotalHours < 0)
            {
                // Negative offset means in the past
                var hours = Math.Abs((int)offset.Value.TotalHours);
                var minutes = Math.Abs(offset.Value.Minutes);
                if (hours > 0 && minutes > 0)
                {
                    assignedAtExpression = $"SYSTIMESTAMP - INTERVAL '{hours}' HOUR - INTERVAL '{minutes}' MINUTE";
                }
                else if (hours > 0)
                {
                    assignedAtExpression = $"SYSTIMESTAMP - INTERVAL '{hours}' HOUR";
                }
                else if (minutes > 0)
                {
                    assignedAtExpression = $"SYSTIMESTAMP - INTERVAL '{minutes}' MINUTE";
                }
                else
                {
                    assignedAtExpression = "SYSTIMESTAMP";
                }
            }
            else
            {
                assignedAtExpression = "SYSTIMESTAMP";
            }

            // Use raw SQL execution with string interpolation for Oracle expressions
            var sql = $@"
                INSERT INTO SHIFT_COVERAGE (
                    ID, RESPONSIBLE_USER_ID, PATIENT_ID, SHIFT_INSTANCE_ID, 
                    UNIT_ID, IS_PRIMARY, ASSIGNED_AT
                ) VALUES (
                    :Id, :UserId, :PatientId, :ShiftInstanceId, 
                    :UnitId, :IsPrimary, {assignedAtExpression}
                )";

            try
            {
                var rowsAffected = await conn.ExecuteAsync(sql,
                    new
                    {
                        Id = coverageId,
                        UserId = userId,
                        PatientId = patientId,
                        ShiftInstanceId = shiftInstanceId,
                        UnitId = unitId,
                        IsPrimary = isPrimary ? 1 : 0
                    });
                
                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException($"Coverage {coverageId} insert affected 0 rows");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to insert coverage {coverageId} for patient {patientId}, shift instance {shiftInstanceId}. SQL: {sql}. Error: {ex.Message}", ex);
            }

            coverageIds.Add(coverageId);
        }

        // Final verification
        var finalCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM SHIFT_COVERAGE WHERE PATIENT_ID = :PatientId AND SHIFT_INSTANCE_ID = :ShiftInstanceId",
            new { PatientId = patientId, ShiftInstanceId = shiftInstanceId });
        
        if (finalCount != coverages.Length)
        {
            throw new InvalidOperationException(
                $"Expected {coverages.Length} coverages but found {finalCount} for patient {patientId}, shift instance {shiftInstanceId}");
        }

        return coverageIds;
    }

    /// <summary>
    /// Creates a PatientAssignedToShiftEvent for testing.
    /// Gets shift instance dates automatically from the created shift instance.
    /// </summary>
    protected async Task<PatientAssignedToShiftEvent> CreatePatientAssignedToShiftEventAsync(
        string patientId,
        string userId,
        string shiftId,
        string shiftInstanceId,
        string unitId,
        bool isPrimary)
    {
        var shiftInstanceRepo = _scope.ServiceProvider.GetRequiredService<IShiftInstanceRepository>();
        var shiftInstance = await shiftInstanceRepo.GetShiftInstanceByIdAsync(shiftInstanceId);
        
        if (shiftInstance == null)
        {
            throw new InvalidOperationException($"Shift instance {shiftInstanceId} not found");
        }

        return new PatientAssignedToShiftEvent(
            patientId: patientId,
            userId: userId,
            shiftId: shiftId,
            shiftInstanceId: shiftInstanceId,
            unitId: unitId,
            shiftStartAt: shiftInstance.StartAt,
            shiftEndAt: shiftInstance.EndAt,
            isPrimary: isPrimary
        );
    }

    /// <summary>
    /// Creates shift instances and shift window for a specific date.
    /// Returns the shift window ID and instance IDs.
    /// </summary>
    protected async Task<(string shiftWindowId, string fromInstanceId, string toInstanceId)> CreateShiftWindowForDateAsync(
        string fromShiftId,
        string toShiftId,
        string unitId,
        DateTime baseDate)
    {
        var connectionFactory = GetConnectionFactory();
        using var conn = connectionFactory.CreateConnection();

        var shiftDates = await ShiftInstanceCalculationService.CalculateShiftInstanceDatesFromDbAsync(
            conn, fromShiftId, toShiftId, baseDate);

        if (shiftDates == null)
        {
            throw new InvalidOperationException($"Shift templates not found: FromShiftId={fromShiftId}, ToShiftId={toShiftId}");
        }

        var (fromShiftStartAt, fromShiftEndAt, toShiftStartAt, toShiftEndAt) = shiftDates.Value;

        var shiftInstanceRepo = _scope.ServiceProvider.GetRequiredService<IShiftInstanceRepository>();
        var fromInstanceId = await shiftInstanceRepo.GetOrCreateShiftInstanceAsync(
            fromShiftId, unitId, fromShiftStartAt, fromShiftEndAt);
        var toInstanceId = await shiftInstanceRepo.GetOrCreateShiftInstanceAsync(
            toShiftId, unitId, toShiftStartAt, toShiftEndAt);

        var shiftWindowRepo = _scope.ServiceProvider.GetRequiredService<IShiftWindowRepository>();
        var shiftWindowId = await shiftWindowRepo.GetOrCreateShiftWindowAsync(
            fromInstanceId, toInstanceId, unitId);

        return (shiftWindowId, fromInstanceId, toInstanceId);
    }

    /// <summary>
    /// Creates a completed handover directly in the database using Dapper.
    /// Useful for testing scenarios where you need a handover in a specific state.
    /// </summary>
    protected async Task<string> CreateCompletedHandoverDirectlyAsync(
        string testRunId,
        string patientId,
        string senderUserId,
        string receiverUserId,
        string shiftWindowId,
        string unitId,
        string? previousHandoverId = null,
        string? patientSummary = null,
        int daysOffset = 1)
    {
        var connectionFactory = GetConnectionFactory();
        using var conn = connectionFactory.CreateConnection();

        var handoverId = $"prev-{testRunId}";

        // Insert HANDOVERS
        // Note: CURRENT_STATE is a virtual column (GENERATED ALWAYS AS ... VIRTUAL) and cannot be inserted
        // It will be automatically calculated as 'Completed' when COMPLETED_AT IS NOT NULL
        // Constraints require: READY_AT >= CREATED_AT, STARTED_AT >= READY_AT, COMPLETED_AT >= STARTED_AT
        // We use intervals to ensure proper chronological order
        await conn.ExecuteAsync(@"
            INSERT INTO HANDOVERS (
                ID, PATIENT_ID, SHIFT_WINDOW_ID, UNIT_ID,
                PREVIOUS_HANDOVER_ID, SENDER_USER_ID, RECEIVER_USER_ID, CREATED_BY_USER_ID,
                CREATED_AT, UPDATED_AT, READY_AT, READY_BY_USER_ID,
                STARTED_AT, STARTED_BY_USER_ID, COMPLETED_AT, COMPLETED_BY_USER_ID
            ) VALUES (
                :id, :patientId, :shiftWindowId, :unitId,
                :previousHandoverId, :senderUserId, :receiverUserId, :createdByUserId,
                LOCALTIMESTAMP - :days, LOCALTIMESTAMP - :days,
                LOCALTIMESTAMP - :days + INTERVAL '1' HOUR, :senderUserId,
                LOCALTIMESTAMP - :days + INTERVAL '2' HOUR, :receiverUserId,
                LOCALTIMESTAMP - :days + INTERVAL '3' HOUR, :receiverUserId
            )",
            new
            {
                id = handoverId,
                patientId,
                shiftWindowId,
                unitId,
                previousHandoverId,
                senderUserId,
                receiverUserId,
                createdByUserId = senderUserId,
                days = daysOffset
            });

        // Insert HANDOVER_CONTENTS
        await conn.ExecuteAsync(@"
            INSERT INTO HANDOVER_CONTENTS (
                HANDOVER_ID, ILLNESS_SEVERITY, PATIENT_SUMMARY, PATIENT_SUMMARY_STATUS,
                SITUATION_AWARENESS, SYNTHESIS, SA_STATUS, SYNTHESIS_STATUS,
                LAST_EDITED_BY, UPDATED_AT
            ) VALUES (
                :handoverId, 'Stable', :patientSummary, :patientSummaryStatus,
                NULL, NULL, 'Draft', 'Draft',
                :lastEditedBy, LOCALTIMESTAMP - :days
            )",
            new
            {
                handoverId,
                patientSummary,
                patientSummaryStatus = patientSummary != null ? "Completed" : (string?)null,
                lastEditedBy = senderUserId,
                days = daysOffset
            });

        return handoverId;
    }
}
