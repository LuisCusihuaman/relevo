using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Dapper;
using Relevo.Infrastructure.Data;

namespace Relevo.IntegrationTests.Data;

/// <summary>
/// Tests for shift window reuse in handover creation
/// V3: Multiple handovers for the same shift window should reuse the same SHIFT_WINDOW_ID
/// </summary>
public class HandoverRepositoryWindowReuseTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryWindowReuseTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    private IShiftWindowRepository GetShiftWindowRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IShiftWindowRepository>();
    }

    [Fact]
    public async Task CreateHandover_ReusesExistingWindow_WhenWindowExists()
    {
        // Arrange: Create two handovers for different patients but same shift window
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var userId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create patients using helper
        var patientId1 = await CreateTestPatientAsync($"window1-{testRunId}", unitId);
        var patientId2 = await CreateTestPatientAsync($"window2-{testRunId}", unitId);

        // Create shift instances using helper
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        // Create coverage for both patients using helper
        await CreateCoverageAsync($"1-{testRunId}", userId, patientId1, fromShiftInstanceId, unitId);
        await CreateCoverageAsync($"2-{testRunId}", userId, patientId2, fromShiftInstanceId, unitId);

        // Act: Create first handover
        var repository = GetHandoverRepository();
        var createRequest1 = new CreateHandoverRequest(
            patientId1,
            userId,
            userId,
            fromShiftId,
            toShiftId,
            userId,
            null
        );
        var handover1 = await repository.CreateHandoverAsync(createRequest1);

        // Act: Create second handover (same shift window, different patient)
        var createRequest2 = new CreateHandoverRequest(
            patientId2,
            userId,
            userId,
            fromShiftId,
            toShiftId,
            userId,
            null
        );
        var handover2 = await repository.CreateHandoverAsync(createRequest2);

        // Assert: Both handovers should use the same shift window ID
        Assert.NotNull(handover1.ShiftWindowId);
        Assert.NotNull(handover2.ShiftWindowId);
        Assert.Equal(handover1.ShiftWindowId, handover2.ShiftWindowId);

        // Verify window exists and is reused
        var windowRepo = GetShiftWindowRepository();
        var window = await windowRepo.GetShiftWindowByIdAsync(handover1.ShiftWindowId);
        Assert.NotNull(window);
        Assert.Equal(fromShiftInstanceId, window.FromShiftInstanceId);
        Assert.Equal(toShiftInstanceId, window.ToShiftInstanceId);
    }

    [Fact]
    public async Task CreateHandover_CreatesNewWindow_WhenWindowDoesNotExist()
    {
        // Arrange: Create handover with unique shift instances
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var userId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create patient using helper
        var patientId = await CreateTestPatientAsync($"newwindow-{testRunId}", unitId);

        // Create shift instances using helper
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        // Create coverage using helper
        await CreateCoverageAsync(testRunId, userId, patientId, fromShiftInstanceId, unitId);

        // Act: Create handover (will create new window)
        var repository = GetHandoverRepository();
        var createRequest = new CreateHandoverRequest(
            patientId,
            userId,
            userId,
            fromShiftId,
            toShiftId,
            userId,
            null
        );

        var handover = await repository.CreateHandoverAsync(createRequest);

        // Assert: Handover should have a shift window ID
        Assert.NotNull(handover.ShiftWindowId);
        Assert.StartsWith("sw-", handover.ShiftWindowId);

        // Verify window exists
        var windowRepo = GetShiftWindowRepository();
        var window = await windowRepo.GetShiftWindowByIdAsync(handover.ShiftWindowId);
        Assert.NotNull(window);
    }
}

