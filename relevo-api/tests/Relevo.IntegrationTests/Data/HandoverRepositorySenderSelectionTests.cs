using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Dapper;
using Relevo.Infrastructure.Data;

namespace Relevo.IntegrationTests.Data;

/// <summary>
/// Tests for sender selection logic in handover creation and Ready
/// V3_PLAN.md regla #10: Sender is selected from SHIFT_COVERAGE (primary or first by ASSIGNED_AT)
/// V3_PLAN.md regla #21: Ready requires SENDER_USER_ID is set
/// </summary>
public class HandoverRepositorySenderSelectionTests : BaseDapperRepoTestFixture
{
    public HandoverRepositorySenderSelectionTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task CreateHandover_SelectsPrimaryAsSender()
    {
        // Arrange: Create patient with primary coverage
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create users and patient using helpers
        var primaryUserId = await CreateTestUserAsync($"user-primary-{testRunId}", null, $"primary{testRunId}@test.com", "Primary", "User");
        var secondaryUserId = await CreateTestUserAsync($"user-secondary-{testRunId}", null, $"secondary{testRunId}@test.com", "Secondary", "User");
        var patientId = await CreateTestPatientAsync($"sender-primary-{testRunId}", unitId);

        // Create shift instances using helper
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        // Create coverage: primary first, then secondary (don't clear existing when adding second)
        await CreateCoverageAsync($"primary-{testRunId}", primaryUserId, patientId, fromShiftInstanceId, unitId, isPrimary: true, clearExisting: true);
        await CreateCoverageAsync($"secondary-{testRunId}", secondaryUserId, patientId, fromShiftInstanceId, unitId, isPrimary: false, clearExisting: false);

        // Act: Create handover
        var repository = GetHandoverRepository();
        var createRequest = new CreateHandoverRequest(
            patientId,
            primaryUserId, // FromDoctorId
            string.Empty, // ToDoctorId (optional)
            fromShiftId,
            toShiftId,
            primaryUserId, // InitiatedBy
            null // Notes
        );

        var handover = await repository.CreateHandoverAsync(createRequest);

        // Assert: Sender should be primary user
        Assert.NotNull(handover);
        Assert.Equal(primaryUserId, handover.SenderUserId);
        Assert.NotEqual(secondaryUserId, handover.SenderUserId);
    }

    [Fact]
    public async Task CreateHandover_SelectsFirstByAssignedAt_WhenNoPrimary()
    {
        // Arrange: Create patient with coverage but no primary
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create users and patient using helpers
        var firstUserId = await CreateTestUserAsync($"user-first-{testRunId}", null, $"first{testRunId}@test.com", "First", "User");
        var secondUserId = await CreateTestUserAsync($"user-second-{testRunId}", null, $"second{testRunId}@test.com", "Second", "User");
        var patientId = await CreateTestPatientAsync($"sender-first-{testRunId}", unitId);

        // Create shift instances using DateTime.Today (same as CreateHandoverAsync uses)
        // NOTE: CreateHandoverAsync always uses DateTime.Today, so we must use the same date
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId, DateTime.Today);

        // Create TWO coverages: both non-primary, first one assigned earlier
        // NOTE: This test requires the fixed UQ_SC_PRIMARY_ACTIVE index that uses ID when IS_PRIMARY=0
        await CreateMultipleCoveragesForTestAsync(
            testRunId,
            patientId,
            fromShiftInstanceId,
            unitId,
            (firstUserId, isPrimary: false, TimeSpan.FromHours(-1)),  // Assigned 1 hour ago
            (secondUserId, isPrimary: false, null)                      // Assigned now
        );

        // Act: Create handover
        var repository = GetHandoverRepository();
        var createRequest = new CreateHandoverRequest(
            patientId,
            firstUserId, // FromDoctorId
            string.Empty, // ToDoctorId (optional)
            fromShiftId,
            toShiftId,
            firstUserId, // InitiatedBy
            null // Notes
        );

        var handover = await repository.CreateHandoverAsync(createRequest);

        // Assert: Sender should be first user (assigned earlier)
        Assert.NotNull(handover);
        Assert.Equal(firstUserId, handover.SenderUserId);
        Assert.NotEqual(secondUserId, handover.SenderUserId);
    }

    [Fact]
    public async Task MarkAsReady_SelectsSenderFromCoverage_WhenNotSet()
    {
        // Arrange: Create handover without SENDER_USER_ID, with coverage
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var handoverId = $"hvo-ready-sender-{testRunId}";

        // Create user and patient using helpers
        var primaryUserId = await CreateTestUserAsync($"user-primary-{testRunId}", null, $"primary{testRunId}@test.com", "Primary", "User");
        var patientId = await CreateTestPatientAsync($"ready-sender-{testRunId}", unitId);

        // Get shift window
        var connectionFactory = GetConnectionFactory();
        using var conn = connectionFactory.CreateConnection();
        var shiftWindowId = await conn.ExecuteScalarAsync<string>(@"
            SELECT ID FROM (
                SELECT ID FROM SHIFT_WINDOWS 
                WHERE UNIT_ID = :unitId 
                ORDER BY CREATED_AT DESC
            ) WHERE ROWNUM <= 1",
            new { unitId });

        if (string.IsNullOrEmpty(shiftWindowId))
        {
            return; // Skip if no shift windows
        }

        // Get FROM_SHIFT_INSTANCE_ID
        var fromShiftInstanceId = await conn.ExecuteScalarAsync<string>(@"
            SELECT FROM_SHIFT_INSTANCE_ID FROM SHIFT_WINDOWS WHERE ID = :shiftWindowId",
            new { shiftWindowId });

        if (string.IsNullOrEmpty(fromShiftInstanceId))
        {
            return; // Skip if no shift instance
        }

        // Create coverage using helper (it already handles deletion of existing coverage)
        await CreateCoverageAsync($"ready-{testRunId}", primaryUserId, patientId, fromShiftInstanceId, unitId, isPrimary: true);

        // Create handover WITHOUT SENDER_USER_ID
        await conn.ExecuteAsync(@"
            INSERT INTO HANDOVERS (ID, PATIENT_ID, SHIFT_WINDOW_ID, UNIT_ID, CREATED_BY_USER_ID, CREATED_AT, UPDATED_AT)
            VALUES (:Id, :PatientId, :ShiftWindowId, :UnitId, :CreatedBy, SYSTIMESTAMP, SYSTIMESTAMP)",
            new { Id = handoverId, PatientId = patientId, ShiftWindowId = shiftWindowId, UnitId = unitId, CreatedBy = primaryUserId });

        // Verify SENDER_USER_ID is NULL
        var beforeReady = await conn.ExecuteScalarAsync<string>(
            "SELECT SENDER_USER_ID FROM HANDOVERS WHERE ID = :HandoverId",
            new { HandoverId = handoverId });
        Assert.Null(beforeReady);

        // Verify coverage exists (required for Ready)
        var coverageCount = await conn.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM SHIFT_COVERAGE 
            WHERE PATIENT_ID = :PatientId AND SHIFT_INSTANCE_ID = :ShiftInstanceId",
            new { PatientId = patientId, ShiftInstanceId = fromShiftInstanceId });
        Assert.True(coverageCount > 0, "Coverage must exist for Ready to work");

        // Verify handover is in Draft state
        var stateBefore = await conn.ExecuteScalarAsync<string>(
            "SELECT CURRENT_STATE FROM HANDOVERS WHERE ID = :HandoverId",
            new { HandoverId = handoverId });
        Assert.Equal("Draft", stateBefore);

        // Act: Mark as Ready
        var repository = GetHandoverRepository();
        var success = await repository.MarkAsReadyAsync(handoverId, primaryUserId);

        // Assert: Should succeed and set SENDER_USER_ID
        Assert.True(success, $"MarkAsReadyAsync should succeed. Coverage exists: {coverageCount}, State: {stateBefore}");

        var afterReady = await repository.GetHandoverByIdAsync(handoverId);
        Assert.NotNull(afterReady);
        Assert.Equal("Ready", afterReady.Handover.Status);
        Assert.Equal(primaryUserId, afterReady.Handover.SenderUserId);
    }

    [Fact]
    public async Task CreateHandover_FailsWhenNoCoverage()
    {
        // Arrange: Create patient WITHOUT coverage
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;

        // Create user and patient using helpers
        var userId = await CreateTestUserAsync($"user-nocoverage-{testRunId}", null, $"nocoverage{testRunId}@test.com", "No", "Coverage");
        var patientId = await CreateTestPatientAsync($"nocoverage-{testRunId}", unitId);

        // Do NOT create coverage

        // Act: Try to create handover
        var repository = GetHandoverRepository();
        var createRequest = new CreateHandoverRequest(
            patientId,
            userId, // FromDoctorId
            string.Empty, // ToDoctorId (optional)
            DapperTestSeeder.ShiftDayId,
            DapperTestSeeder.ShiftNightId,
            userId, // InitiatedBy
            null // Notes
        );

        // Assert: Should throw InvalidOperationException (V3_PLAN.md regla #10)
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await repository.CreateHandoverAsync(createRequest));
    }
}

