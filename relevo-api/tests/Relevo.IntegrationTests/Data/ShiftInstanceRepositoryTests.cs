using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Dapper;
using Relevo.Infrastructure.Data;

namespace Relevo.IntegrationTests.Data;

/// <summary>
/// Tests for Shift Instance creation and management
/// V3: SHIFT_INSTANCES represent concrete occurrences of shift templates
/// </summary>
public class ShiftInstanceRepositoryTests : BaseDapperRepoTestFixture
{
    public ShiftInstanceRepositoryTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IShiftInstanceRepository GetShiftInstanceRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IShiftInstanceRepository>();
    }

    [Fact]
    public async Task GetOrCreateShiftInstanceAsync_CreatesNewInstance_WhenNotExists()
    {
        // Arrange
        var repository = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var shiftId = DapperTestSeeder.ShiftDayId;
        var startAt = DateTime.Today.AddHours(7);
        var endAt = DateTime.Today.AddHours(15);

        // Act
        var instanceId = await repository.GetOrCreateShiftInstanceAsync(shiftId, unitId, startAt, endAt);

        // Assert
        Assert.NotNull(instanceId);
        Assert.StartsWith("si-", instanceId);

        // Verify instance was created
        var connectionFactory = _scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
        using var conn = connectionFactory.CreateConnection();
        var instance = await repository.GetShiftInstanceByIdAsync(instanceId);
        
        Assert.NotNull(instance);
        Assert.Equal(shiftId, instance.ShiftId);
        Assert.Equal(unitId, instance.UnitId);
        Assert.Equal(startAt, instance.StartAt);
        Assert.Equal(endAt, instance.EndAt);
    }

    [Fact]
    public async Task GetOrCreateShiftInstanceAsync_ReturnsExistingInstance_WhenExists()
    {
        // Arrange
        var repository = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var shiftId = DapperTestSeeder.ShiftDayId;
        var startAt = DateTime.Today.AddHours(7);
        var endAt = DateTime.Today.AddHours(15);

        // Act: Create first instance
        var firstId = await repository.GetOrCreateShiftInstanceAsync(shiftId, unitId, startAt, endAt);

        // Act: Try to create same instance again
        var secondId = await repository.GetOrCreateShiftInstanceAsync(shiftId, unitId, startAt, endAt);

        // Assert: Should return same ID
        Assert.Equal(firstId, secondId);
    }

    [Fact]
    public async Task GetOrCreateShiftInstanceAsync_CreatesInstanceWithCorrectTimestamps()
    {
        // Arrange
        var repository = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var shiftId = DapperTestSeeder.ShiftDayId;
        var startAt = DateTime.Today.AddHours(7).AddMinutes(30);
        var endAt = DateTime.Today.AddHours(15).AddMinutes(45);

        // Act
        var instanceId = await repository.GetOrCreateShiftInstanceAsync(shiftId, unitId, startAt, endAt);

        // Assert
        var instance = await repository.GetShiftInstanceByIdAsync(instanceId);
        Assert.NotNull(instance);
        Assert.Equal(startAt, instance.StartAt);
        Assert.Equal(endAt, instance.EndAt);
    }

    [Fact]
    public async Task GetOrCreateShiftInstanceAsync_HandlesNightShiftCrossingMidnight()
    {
        // Arrange: Night shift that crosses midnight (e.g., 19:00 to 07:00 next day)
        var repository = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var shiftId = DapperTestSeeder.ShiftNightId;
        var today = DateTime.Today;
        var startAt = today.AddHours(19); // 7 PM today
        var endAt = today.AddDays(1).AddHours(7); // 7 AM next day

        // Act
        var instanceId = await repository.GetOrCreateShiftInstanceAsync(shiftId, unitId, startAt, endAt);

        // Assert
        var instance = await repository.GetShiftInstanceByIdAsync(instanceId);
        Assert.NotNull(instance);
        Assert.Equal(startAt, instance.StartAt);
        Assert.Equal(endAt, instance.EndAt);
        Assert.True(instance.EndAt > instance.StartAt, "End time should be after start time even when crossing midnight");
        Assert.True(instance.EndAt.Date > instance.StartAt.Date, "End date should be next day");
    }

    [Fact]
    public async Task GetOrCreateShiftInstanceAsync_CreatesDifferentInstances_ForDifferentTimes()
    {
        // Arrange
        var repository = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var shiftId = DapperTestSeeder.ShiftDayId;
        var today = DateTime.Today;
        var startAt1 = today.AddHours(7);
        var endAt1 = today.AddHours(15);
        var startAt2 = today.AddDays(1).AddHours(7); // Next day
        var endAt2 = today.AddDays(1).AddHours(15);

        // Act
        var instanceId1 = await repository.GetOrCreateShiftInstanceAsync(shiftId, unitId, startAt1, endAt1);
        var instanceId2 = await repository.GetOrCreateShiftInstanceAsync(shiftId, unitId, startAt2, endAt2);

        // Assert: Should create different instances
        Assert.NotEqual(instanceId1, instanceId2);

        var instance1 = await repository.GetShiftInstanceByIdAsync(instanceId1);
        var instance2 = await repository.GetShiftInstanceByIdAsync(instanceId2);
        
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
        Assert.Equal(startAt1, instance1.StartAt);
        Assert.Equal(startAt2, instance2.StartAt);
    }

    [Fact]
    public async Task GetShiftInstancesAsync_ReturnsInstancesForUnit()
    {
        // Arrange
        var repository = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var shiftId = DapperTestSeeder.ShiftDayId;
        var today = DateTime.Today;
        
        // Create multiple instances
        var instanceId1 = await repository.GetOrCreateShiftInstanceAsync(shiftId, unitId, today.AddHours(7), today.AddHours(15));
        var instanceId2 = await repository.GetOrCreateShiftInstanceAsync(shiftId, unitId, today.AddDays(1).AddHours(7), today.AddDays(1).AddHours(15));

        // Act
        var instances = await repository.GetShiftInstancesAsync(unitId, null, null);

        // Assert: Should include our instances
        Assert.True(instances.Count >= 2, "Should return at least 2 instances");
        var foundIds = instances.Select(i => i.Id).ToList();
        Assert.NotNull(instanceId1);
        Assert.NotNull(instanceId2);
        Assert.Contains(instanceId1, foundIds);
        Assert.Contains(instanceId2, foundIds);
    }

    [Fact]
    public async Task GetShiftInstancesAsync_FiltersByDateRange()
    {
        // Arrange
        var repository = GetShiftInstanceRepository();
        var unitId = DapperTestSeeder.UnitId;
        var shiftId = DapperTestSeeder.ShiftDayId;
        var today = DateTime.Today;
        
        // Create instances on different days
        var instanceId1 = await repository.GetOrCreateShiftInstanceAsync(shiftId, unitId, today.AddHours(7), today.AddHours(15));
        var instanceId2 = await repository.GetOrCreateShiftInstanceAsync(shiftId, unitId, today.AddDays(2).AddHours(7), today.AddDays(2).AddHours(15));

        // Act: Filter to only today
        var instances = await repository.GetShiftInstancesAsync(unitId, today, today.AddDays(1));

        // Assert: Should only include today's instance
        var foundIds = instances.Select(i => i.Id).ToList();
        Assert.Contains(instanceId1, foundIds);
        Assert.DoesNotContain(instanceId2, foundIds);
    }
}

