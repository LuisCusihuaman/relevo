using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Relevo.Infrastructure.Data;

namespace Relevo.IntegrationTests.Data;

/// <summary>
/// Tests for Shift Window creation and management
/// V3: SHIFT_WINDOWS represent transitions between shift instances
/// </summary>
public class ShiftWindowRepositoryTests : BaseDapperRepoTestFixture
{
    public ShiftWindowRepositoryTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IShiftWindowRepository GetShiftWindowRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IShiftWindowRepository>();
    }

    private IShiftInstanceRepository GetShiftInstanceRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IShiftInstanceRepository>();
    }

    [Fact]
    public async Task GetOrCreateShiftWindowAsync_CreatesNewWindow_WhenNotExists()
    {
        // Arrange
        var windowRepo = GetShiftWindowRepository();
        var instanceRepo = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;
        var today = DateTime.Today;

        // Create shift instances
        var fromInstanceId = await instanceRepo.GetOrCreateShiftInstanceAsync(
            fromShiftId, unitId, today.AddHours(7), today.AddHours(15));
        var toInstanceId = await instanceRepo.GetOrCreateShiftInstanceAsync(
            toShiftId, unitId, today.AddHours(19), today.AddDays(1).AddHours(7));

        // Act
        var windowId = await windowRepo.GetOrCreateShiftWindowAsync(fromInstanceId, toInstanceId, unitId);

        // Assert
        Assert.NotNull(windowId);
        Assert.StartsWith("sw-", windowId);

        // Verify window was created
        var window = await windowRepo.GetShiftWindowByIdAsync(windowId);
        Assert.NotNull(window);
        Assert.Equal(fromInstanceId, window.FromShiftInstanceId);
        Assert.Equal(toInstanceId, window.ToShiftInstanceId);
        Assert.Equal(unitId, window.UnitId);
    }

    [Fact]
    public async Task GetOrCreateShiftWindowAsync_ReturnsExistingWindow_WhenExists()
    {
        // Arrange
        var windowRepo = GetShiftWindowRepository();
        var instanceRepo = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;
        var today = DateTime.Today;

        // Create shift instances
        var fromInstanceId = await instanceRepo.GetOrCreateShiftInstanceAsync(
            fromShiftId, unitId, today.AddHours(7), today.AddHours(15));
        var toInstanceId = await instanceRepo.GetOrCreateShiftInstanceAsync(
            toShiftId, unitId, today.AddHours(19), today.AddDays(1).AddHours(7));

        // Act: Create first window
        var firstWindowId = await windowRepo.GetOrCreateShiftWindowAsync(fromInstanceId, toInstanceId, unitId);

        // Act: Try to create same window again
        var secondWindowId = await windowRepo.GetOrCreateShiftWindowAsync(fromInstanceId, toInstanceId, unitId);

        // Assert: Should return same ID (reuses existing window)
        Assert.Equal(firstWindowId, secondWindowId);
    }

    [Fact]
    public async Task GetOrCreateShiftWindowAsync_EnforcesUniqueness_ForSamePair()
    {
        // Arrange
        var windowRepo = GetShiftWindowRepository();
        var instanceRepo = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;
        var today = DateTime.Today;

        // Create shift instances
        var fromInstanceId = await instanceRepo.GetOrCreateShiftInstanceAsync(
            fromShiftId, unitId, today.AddHours(7), today.AddHours(15));
        var toInstanceId = await instanceRepo.GetOrCreateShiftInstanceAsync(
            toShiftId, unitId, today.AddHours(19), today.AddDays(1).AddHours(7));

        // Act: Create window multiple times
        var windowId1 = await windowRepo.GetOrCreateShiftWindowAsync(fromInstanceId, toInstanceId, unitId);
        var windowId2 = await windowRepo.GetOrCreateShiftWindowAsync(fromInstanceId, toInstanceId, unitId);
        var windowId3 = await windowRepo.GetOrCreateShiftWindowAsync(fromInstanceId, toInstanceId, unitId);

        // Assert: All should return same ID (enforced by unique constraint)
        Assert.Equal(windowId1, windowId2);
        Assert.Equal(windowId2, windowId3);
    }

    [Fact]
    public async Task GetOrCreateShiftWindowAsync_CreatesDifferentWindows_ForDifferentPairs()
    {
        // Arrange
        var windowRepo = GetShiftWindowRepository();
        var instanceRepo = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;
        var today = DateTime.Today;

        // Create first pair of instances
        var fromInstanceId1 = await instanceRepo.GetOrCreateShiftInstanceAsync(
            fromShiftId, unitId, today.AddHours(7), today.AddHours(15));
        var toInstanceId1 = await instanceRepo.GetOrCreateShiftInstanceAsync(
            toShiftId, unitId, today.AddHours(19), today.AddDays(1).AddHours(7));

        // Create second pair (next day)
        var fromInstanceId2 = await instanceRepo.GetOrCreateShiftInstanceAsync(
            fromShiftId, unitId, today.AddDays(1).AddHours(7), today.AddDays(1).AddHours(15));
        var toInstanceId2 = await instanceRepo.GetOrCreateShiftInstanceAsync(
            toShiftId, unitId, today.AddDays(1).AddHours(19), today.AddDays(2).AddHours(7));

        // Act
        var windowId1 = await windowRepo.GetOrCreateShiftWindowAsync(fromInstanceId1, toInstanceId1, unitId);
        var windowId2 = await windowRepo.GetOrCreateShiftWindowAsync(fromInstanceId2, toInstanceId2, unitId);

        // Assert: Should create different windows
        Assert.NotEqual(windowId1, windowId2);

        var window1 = await windowRepo.GetShiftWindowByIdAsync(windowId1);
        var window2 = await windowRepo.GetShiftWindowByIdAsync(windowId2);
        
        Assert.NotNull(window1);
        Assert.NotNull(window2);
        Assert.Equal(fromInstanceId1, window1.FromShiftInstanceId);
        Assert.Equal(fromInstanceId2, window2.FromShiftInstanceId);
    }

    [Fact]
    public async Task GetOrCreateShiftWindowAsync_HandlesSameDayTransitions()
    {
        // Arrange: Day → Day transition (same day)
        var windowRepo = GetShiftWindowRepository();
        var instanceRepo = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var shiftId = DapperTestSeeder.ShiftDayId;
        var today = DateTime.Today;

        // Create two day shift instances on same day (e.g., morning and afternoon)
        var fromInstanceId = await instanceRepo.GetOrCreateShiftInstanceAsync(
            shiftId, unitId, today.AddHours(7), today.AddHours(15));
        var toInstanceId = await instanceRepo.GetOrCreateShiftInstanceAsync(
            shiftId, unitId, today.AddHours(15), today.AddHours(23));

        // Act
        var windowId = await windowRepo.GetOrCreateShiftWindowAsync(fromInstanceId, toInstanceId, unitId);

        // Assert
        Assert.NotNull(windowId);
        var window = await windowRepo.GetShiftWindowByIdAsync(windowId);
        Assert.NotNull(window);
        Assert.Equal(fromInstanceId, window.FromShiftInstanceId);
        Assert.Equal(toInstanceId, window.ToShiftInstanceId);
    }

    [Fact]
    public async Task GetShiftWindowsAsync_ReturnsWindowsForUnit()
    {
        // Arrange
        var windowRepo = GetShiftWindowRepository();
        var instanceRepo = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;
        var today = DateTime.Today;

        // Create windows
        var fromInstanceId1 = await instanceRepo.GetOrCreateShiftInstanceAsync(
            fromShiftId, unitId, today.AddHours(7), today.AddHours(15));
        var toInstanceId1 = await instanceRepo.GetOrCreateShiftInstanceAsync(
            toShiftId, unitId, today.AddHours(19), today.AddDays(1).AddHours(7));
        var windowId1 = await windowRepo.GetOrCreateShiftWindowAsync(fromInstanceId1, toInstanceId1, unitId);

        var fromInstanceId2 = await instanceRepo.GetOrCreateShiftInstanceAsync(
            fromShiftId, unitId, today.AddDays(1).AddHours(7), today.AddDays(1).AddHours(15));
        var toInstanceId2 = await instanceRepo.GetOrCreateShiftInstanceAsync(
            toShiftId, unitId, today.AddDays(1).AddHours(19), today.AddDays(2).AddHours(7));
        var windowId2 = await windowRepo.GetOrCreateShiftWindowAsync(fromInstanceId2, toInstanceId2, unitId);

        // Act
        var windows = await windowRepo.GetShiftWindowsAsync(unitId, null, null);

        // Assert: Should include our windows
        Assert.True(windows.Count >= 2, "Should return at least 2 windows");
        var foundIds = windows.Select(w => w.Id).ToList();
        Assert.Contains(windowId1, foundIds);
        Assert.Contains(windowId2, foundIds);
    }
}

