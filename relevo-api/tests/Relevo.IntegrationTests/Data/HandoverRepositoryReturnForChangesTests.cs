using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Dapper;
using Relevo.Infrastructure.Data;

namespace Relevo.IntegrationTests.Data;

/// <summary>
/// Tests for ReturnForChangesAsync
/// V3_PLAN.md regla #26: ReturnForChanges is not a state, but a regla de app
/// Clears READY_AT (returns to Draft)
/// Used for "soft rejection" (missing information, needs revision)
/// </summary>
public class HandoverRepositoryReturnForChangesTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryReturnForChangesTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task ReturnForChanges_ClearsReadyAtAndReturnsToDraft()
    {
        // Arrange: Create handover in Ready state
        // Business rule: ReturnForChanges clears READY_AT and returns handover to Draft state
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var userId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create patient using helper
        var patientId = await CreateTestPatientAsync($"return-{testRunId}", unitId);

        // Create shift instances using helper
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        // Create coverage for FROM shift (required for Ready)
        await CreateCoverageAsync($"return-{testRunId}", userId, patientId, fromShiftInstanceId, unitId);

        // Create handover using production methods
        var repository = GetHandoverRepository();
        var createRequest = new CreateHandoverRequest(
            patientId,
            userId,
            userId, // receiver
            fromShiftId,
            toShiftId,
            userId, // initiated by
            null
        );

        var handover = await repository.CreateHandoverAsync(createRequest);
        
        // Mark as Ready using production method
        var readySuccess = await repository.MarkAsReadyAsync(handover.Id, userId);
        Assert.True(readySuccess, "MarkAsReadyAsync should succeed");

        // Verify initial state is Ready using public API
        var initial = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(initial);
        Assert.Equal("Ready", initial.Handover.Status);
        Assert.NotNull(initial.Handover.ReadyAt);

        // Act: Return for changes
        var success = await repository.ReturnForChangesAsync(handover.Id, userId);

        // Assert: Should succeed and return to Draft
        Assert.True(success, "ReturnForChangesAsync should succeed when handover is in Ready state");

        var updated = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(updated);
        Assert.Equal("Draft", updated.Handover.Status);
        Assert.Null(updated.Handover.ReadyAt);
        Assert.Null(updated.Handover.ReadyByUserId);
    }

    [Fact]
    public async Task ReturnForChanges_OnlyWorksFromReadyState()
    {
        // Arrange: Create handover in Draft state (not Ready)
        // Business rule: ReturnForChanges only works from Ready state (requires READY_AT to clear)
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var userId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create patient using helper
        var patientId = await CreateTestPatientAsync($"return-draft-{testRunId}", unitId);

        // Create shift instances using helper
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        // Create coverage for FROM shift (required for handover creation)
        await CreateCoverageAsync($"return-draft-{testRunId}", userId, patientId, fromShiftInstanceId, unitId);

        // Create handover using production methods (will be in Draft state)
        var repository = GetHandoverRepository();
        var createRequest = new CreateHandoverRequest(
            patientId,
            userId,
            userId, // receiver
            fromShiftId,
            toShiftId,
            userId, // initiated by
            null
        );

        var handover = await repository.CreateHandoverAsync(createRequest);
        
        // Verify handover is in Draft state (not Ready)
        var draftHandover = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(draftHandover);
        Assert.Equal("Draft", draftHandover.Handover.Status);
        Assert.Null(draftHandover.Handover.ReadyAt);

        // Act: Try to return for changes from Draft
        var success = await repository.ReturnForChangesAsync(handover.Id, userId);

        // Assert: Should fail - business rule: ReturnForChanges only works from Ready state
        Assert.False(success, "ReturnForChangesAsync should fail when handover is not in Ready state");

        var handoverAfterAttempt = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(handoverAfterAttempt);
        Assert.Equal("Draft", handoverAfterAttempt.Handover.Status);
    }

    [Fact]
    public async Task ReturnForChanges_CanReadyAgainAfterReturn()
    {
        // Arrange: Create handover, mark Ready, then ReturnForChanges
        // Business rule: After ReturnForChanges, handover can be marked Ready again if coverage still exists
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var userId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create patient using helper
        var patientId = await CreateTestPatientAsync($"return-ready-{testRunId}", unitId);

        // Create shift instances using helper
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        // Create coverage for FROM shift (required for Ready)
            await CreateCoverageAsync($"return-{testRunId}", userId, patientId, fromShiftInstanceId, unitId, isPrimary: true);

        // Create handover using production methods
        var repository = GetHandoverRepository();
        var createRequest = new CreateHandoverRequest(
            patientId,
            userId,
            userId, // receiver
            fromShiftId,
            toShiftId,
            userId, // initiated by
            null
        );

        var handover = await repository.CreateHandoverAsync(createRequest);
        
        // Mark as Ready using production method
        var readySuccess1 = await repository.MarkAsReadyAsync(handover.Id, userId);
        Assert.True(readySuccess1, "MarkAsReadyAsync should succeed");

        // Verify handover is in Ready state
        var readyHandover = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(readyHandover);
        Assert.Equal("Ready", readyHandover.Handover.Status);
        Assert.NotNull(readyHandover.Handover.ReadyAt);

        // Act 1: Return for changes
        var returnSuccess = await repository.ReturnForChangesAsync(handover.Id, userId);
        Assert.True(returnSuccess, "ReturnForChangesAsync should succeed");

        var afterReturn = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(afterReturn);
        Assert.Equal("Draft", afterReturn.Handover.Status);
        Assert.Null(afterReturn.Handover.ReadyAt);
        Assert.Null(afterReturn.Handover.ReadyByUserId);

        // Verify SENDER_USER_ID is still set (should be preserved after ReturnForChanges)
        Assert.NotNull(afterReturn.Handover.SenderUserId);

        // Act 2: Mark Ready again (coverage should still exist from setup)
        var readySuccess2 = await repository.MarkAsReadyAsync(handover.Id, userId);
        Assert.True(readySuccess2, "MarkAsReadyAsync should succeed after ReturnForChanges if coverage still exists");

        // Assert: Should be Ready again
        var afterReady = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(afterReady);
        Assert.Equal("Ready", afterReady.Handover.Status);
        Assert.NotNull(afterReady.Handover.ReadyAt);
    }

    [Fact]
    public async Task ReturnForChanges_FailsWhenInProgress()
    {
        // Arrange: Create handover in InProgress state
        // Business rule: ReturnForChanges only works from Ready state, not from InProgress
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var senderUserId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create receiver user and patient using helpers
        var receiverUserId = await CreateTestUserAsync($"user-receiver-{testRunId}", null, $"receiver{testRunId}@test.com", "Receiver", "User");
        var patientId = await CreateTestPatientAsync($"return-inprogress-{testRunId}", unitId);

        // Create shift instances using helper
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        // Create coverage for sender (FROM shift) and receiver (TO shift)
        await CreateCoverageAsync($"sender-{testRunId}", senderUserId, patientId, fromShiftInstanceId, unitId);
        await CreateCoverageAsync($"receiver-{testRunId}", receiverUserId, patientId, toShiftInstanceId, unitId);

        // Create handover using production methods
        var repository = GetHandoverRepository();
        var createRequest = new CreateHandoverRequest(
            patientId,
            senderUserId,
            receiverUserId,
            fromShiftId,
            toShiftId,
            senderUserId,
            null
        );

        var handover = await repository.CreateHandoverAsync(createRequest);
        
        // Mark as Ready
        var readySuccess = await repository.MarkAsReadyAsync(handover.Id, senderUserId);
        Assert.True(readySuccess, "MarkAsReadyAsync should succeed");

        // Start handover
        var startSuccess = await repository.StartHandoverAsync(handover.Id, receiverUserId);
        Assert.True(startSuccess, "StartHandoverAsync should succeed");

        // Verify handover is in InProgress state
        var inProgressHandover = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(inProgressHandover);
        Assert.Equal("InProgress", inProgressHandover.Handover.Status);

        // Act: Try to return for changes from InProgress
        var success = await repository.ReturnForChangesAsync(handover.Id, receiverUserId);

        // Assert: Should fail - business rule: ReturnForChanges only works from Ready state
        Assert.False(success, "ReturnForChangesAsync should fail when handover is in InProgress state");

        var handoverAfterAttempt = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(handoverAfterAttempt);
        Assert.Equal("InProgress", handoverAfterAttempt.Handover.Status);
    }
}

