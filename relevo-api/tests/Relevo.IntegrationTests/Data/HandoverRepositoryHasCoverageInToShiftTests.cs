using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Dapper;
using Relevo.Infrastructure.Data;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryHasCoverageInToShiftTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryHasCoverageInToShiftTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task HasCoverageInToShift_ReturnsTrue_WhenUserHasCoverage()
    {
        // Arrange
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;
        // Use "dr-1" which has coverage created by seeder in TO shift
        var userId = "dr-1";
        var patientId = DapperTestSeeder.PatientId1;

        // Get handover to find TO_SHIFT_INSTANCE_ID
        var handover = await repository.GetHandoverByIdAsync(handoverId);
        Assert.NotNull(handover);
        Assert.NotNull(handover.Handover.ShiftWindowId);

        // Get TO_SHIFT_INSTANCE_ID from SHIFT_WINDOWS
        var connectionFactory = _scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
        using var conn = connectionFactory.CreateConnection();
        var toShiftInstanceId = await conn.ExecuteScalarAsync<string>(
            "SELECT TO_SHIFT_INSTANCE_ID FROM SHIFT_WINDOWS WHERE ID = :shiftWindowId",
            new { shiftWindowId = handover.Handover.ShiftWindowId });
        Assert.NotNull(toShiftInstanceId);

        // Get UNIT_ID from patient
        var unitId = await conn.ExecuteScalarAsync<string>(
            "SELECT UNIT_ID FROM PATIENTS WHERE ID = :patientId",
            new { patientId });
        Assert.NotNull(unitId);

        // Verify coverage exists (created by seeder for "dr-1" in TO shift)
        var existingCoverage = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM SHIFT_COVERAGE 
              WHERE RESPONSIBLE_USER_ID = :UserId 
                AND PATIENT_ID = :PatientId 
                AND SHIFT_INSTANCE_ID = :ShiftInstanceId",
            new { UserId = userId, PatientId = patientId, ShiftInstanceId = toShiftInstanceId });
        
        Assert.True(existingCoverage > 0, "Coverage should exist for dr-1 in TO shift (created by seeder)");

        // Act
        var hasCoverage = await repository.HasCoverageInToShiftAsync(handoverId, userId);

        // Assert
        Assert.True(hasCoverage);
    }

    [Fact]
    public async Task HasCoverageInToShift_ReturnsFalse_WhenUserDoesNotHaveCoverage()
    {
        // Arrange
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;
        var userIdWithoutCoverage = "user-without-coverage";

        // Ensure this user doesn't exist or doesn't have coverage
        var connectionFactory = _scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
        using var conn = connectionFactory.CreateConnection();
        var userExists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM USERS WHERE ID = :userId",
            new { userId = userIdWithoutCoverage });
        
        if (userExists == 0)
        {
            // Create user but don't assign coverage using helper
            await CreateTestUserAsync(userIdWithoutCoverage, null, "nocoverage@test.com", "No", "Coverage");
        }

        // Act
        var hasCoverage = await repository.HasCoverageInToShiftAsync(handoverId, userIdWithoutCoverage);

        // Assert
        Assert.False(hasCoverage);
    }

    [Fact]
    public async Task HasCoverageInToShift_ReturnsFalse_WhenHandoverDoesNotExist()
    {
        // Arrange
        var repository = GetHandoverRepository();
        var nonExistentHandoverId = "non-existent-handover";
        var userId = DapperTestSeeder.UserId;

        // Act
        var hasCoverage = await repository.HasCoverageInToShiftAsync(nonExistentHandoverId, userId);

        // Assert
        Assert.False(hasCoverage);
    }
}

