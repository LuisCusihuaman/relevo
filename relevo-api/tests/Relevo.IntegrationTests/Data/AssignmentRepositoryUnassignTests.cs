using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Dapper;
using Relevo.Infrastructure.Data;

namespace Relevo.IntegrationTests.Data;

/// <summary>
/// Tests for UnassignPatientAsync with primary promotion
/// V3_PLAN.md regla #15: Si el primary se desasigna, promover al siguiente (más antiguo por ASSIGNED_AT)
/// </summary>
public class AssignmentRepositoryUnassignTests : BaseDapperRepoTestFixture
{
    public AssignmentRepositoryUnassignTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IAssignmentRepository GetAssignmentRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IAssignmentRepository>();
    }

    [Fact]
    public async Task UnassignPatient_PromotesPrimary_WhenPrimaryIsUnassigned()
    {
        // Arrange: Create test data
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var shiftId = DapperTestSeeder.ShiftDayId;

        // Create users and patient using helpers
        var primaryUserId = await CreateTestUserAsync($"user-primary-{testRunId}", null, $"primary{testRunId}@test.com", "Primary", "User");
        var secondaryUserId = await CreateTestUserAsync($"user-secondary-{testRunId}", null, $"secondary{testRunId}@test.com", "Secondary", "User");
        var patientId = await CreateTestPatientAsync($"unassign-{testRunId}", unitId);

        // Create shift instance using helper (need a dummy toShiftId for the helper, but we only need fromShiftInstanceId)
        var shiftInstanceRepo = _scope.ServiceProvider.GetRequiredService<IShiftInstanceRepository>();
        var connectionFactory = GetConnectionFactory();
        using var conn = connectionFactory.CreateConnection();
        
        // Get shift template to calculate dates
        var shift = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT START_TIME, END_TIME FROM SHIFTS WHERE ID = :shiftId",
            new { shiftId });
        
        if (shift == null)
        {
            throw new InvalidOperationException($"Shift template {shiftId} not found");
        }

        var today = DateTime.Today;
        var startTime = TimeSpan.Parse((string)shift.START_TIME);
        var endTime = TimeSpan.Parse((string)shift.END_TIME);
        var startAt = today.Add(startTime);
        var endAt = today.Add(endTime);
        if (endAt < startAt) endAt = endAt.AddDays(1);

        var shiftInstanceId = await shiftInstanceRepo.GetOrCreateShiftInstanceAsync(
            shiftId, unitId, startAt, endAt);

        // Create coverage: primary user first (will be primary), secondary second (non-primary)
        await CreateMultipleCoveragesForTestAsync(
            testRunId,
            patientId,
            shiftInstanceId,
            unitId,
            (primaryUserId, isPrimary: true, TimeSpan.FromHours(-1)),  // Assigned 1 hour ago, primary
            (secondaryUserId, isPrimary: false, null)                   // Assigned now, non-primary
        );

        // Verify primary is set correctly
        var primaryBefore = await conn.ExecuteScalarAsync<string>(@"
            SELECT RESPONSIBLE_USER_ID FROM SHIFT_COVERAGE
            WHERE PATIENT_ID = :patientId AND SHIFT_INSTANCE_ID = :shiftInstanceId AND IS_PRIMARY = 1",
            new { patientId, shiftInstanceId });
        Assert.Equal(primaryUserId, primaryBefore);

        // Act: Unassign primary user
        var repository = GetAssignmentRepository();
        var success = await repository.UnassignPatientAsync(primaryUserId, shiftInstanceId, patientId);

        // Assert: Should succeed
        Assert.True(success);

        // Verify primary was promoted to secondary user
        var primaryAfter = await conn.ExecuteScalarAsync<string>(@"
            SELECT RESPONSIBLE_USER_ID FROM SHIFT_COVERAGE
            WHERE PATIENT_ID = :patientId AND SHIFT_INSTANCE_ID = :shiftInstanceId AND IS_PRIMARY = 1",
            new { patientId, shiftInstanceId });
        Assert.Equal(secondaryUserId, primaryAfter);

        // Verify primary user's coverage was deleted
        var primaryCoverageExists = await conn.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM SHIFT_COVERAGE
            WHERE RESPONSIBLE_USER_ID = :userId AND PATIENT_ID = :patientId AND SHIFT_INSTANCE_ID = :shiftInstanceId",
            new { userId = primaryUserId, patientId, shiftInstanceId });
        Assert.Equal(0, primaryCoverageExists);
    }

    [Fact]
    public async Task UnassignPatient_DoesNotPromote_WhenNonPrimaryIsUnassigned()
    {
        // Arrange: Similar setup but unassign non-primary
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var shiftId = DapperTestSeeder.ShiftDayId;

        // Create users and patient using helpers
        var primaryUserId = await CreateTestUserAsync($"user-p-{testRunId}", null, $"primary{testRunId}@test.com", "Primary", "User");
        var secondaryUserId = await CreateTestUserAsync($"user-s-{testRunId}", null, $"secondary{testRunId}@test.com", "Secondary", "User");
        var patientId = await CreateTestPatientAsync($"unassign-np-{testRunId}", unitId);

        // Create shift instance using helper
        var shiftInstanceRepo = _scope.ServiceProvider.GetRequiredService<IShiftInstanceRepository>();
        var connectionFactory = GetConnectionFactory();
        using var conn = connectionFactory.CreateConnection();
        
        // Get shift template to calculate dates
        var shift = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT START_TIME, END_TIME FROM SHIFTS WHERE ID = :shiftId",
            new { shiftId });
        
        if (shift == null)
        {
            throw new InvalidOperationException($"Shift template {shiftId} not found");
        }

        var today = DateTime.Today;
        var startTime = TimeSpan.Parse((string)shift.START_TIME);
        var endTime = TimeSpan.Parse((string)shift.END_TIME);
        var startAt = today.Add(startTime);
        var endAt = today.Add(endTime);
        if (endAt < startAt) endAt = endAt.AddDays(1);

        var shiftInstanceId = await shiftInstanceRepo.GetOrCreateShiftInstanceAsync(
            shiftId, unitId, startAt, endAt);

        // Create coverage: primary user first, secondary second (non-primary)
        await CreateMultipleCoveragesForTestAsync(
            testRunId,
            patientId,
            shiftInstanceId,
            unitId,
            (primaryUserId, isPrimary: true, TimeSpan.FromHours(-1)),  // Assigned 1 hour ago, primary
            (secondaryUserId, isPrimary: false, null)                   // Assigned now, non-primary
        );

        // Verify primary is set correctly
        var primaryBefore = await conn.ExecuteScalarAsync<string>(@"
            SELECT RESPONSIBLE_USER_ID FROM SHIFT_COVERAGE
            WHERE PATIENT_ID = :patientId AND SHIFT_INSTANCE_ID = :shiftInstanceId AND IS_PRIMARY = 1",
            new { patientId, shiftInstanceId });
        Assert.Equal(primaryUserId, primaryBefore);

        // Act: Unassign secondary (non-primary) user
        var repository = GetAssignmentRepository();
        var success = await repository.UnassignPatientAsync(secondaryUserId, shiftInstanceId, patientId);

        // Assert: Should succeed
        Assert.True(success);

        // Verify primary did NOT change (still primary user)
        var primaryAfter = await conn.ExecuteScalarAsync<string>(@"
            SELECT RESPONSIBLE_USER_ID FROM SHIFT_COVERAGE
            WHERE PATIENT_ID = :patientId AND SHIFT_INSTANCE_ID = :shiftInstanceId AND IS_PRIMARY = 1",
            new { patientId, shiftInstanceId });
        Assert.Equal(primaryUserId, primaryAfter);

        // Verify secondary user's coverage was deleted
        var secondaryCoverageExists = await conn.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM SHIFT_COVERAGE
            WHERE RESPONSIBLE_USER_ID = :userId AND PATIENT_ID = :patientId AND SHIFT_INSTANCE_ID = :shiftInstanceId",
            new { userId = secondaryUserId, patientId, shiftInstanceId });
        Assert.Equal(0, secondaryCoverageExists);
    }
}

