using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Dapper;
using Relevo.Infrastructure.Data;

namespace Relevo.IntegrationTests.Data;

/// <summary>
/// Tests for CompleteHandoverAsync
/// V3_PLAN.md regla #24: quien completa debe tener coverage en TO shift, NO puede ser sender
/// Constraint CHK_HO_COMPLETED_NE_SENDER ensures COMPLETED_BY_USER_ID <> SENDER_USER_ID
/// Constraint CHK_HO_CO_REQ_ST ensures COMPLETED_AT requires STARTED_AT
/// </summary>
public class HandoverRepositoryCompleteHandoverTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryCompleteHandoverTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task CompleteHandover_SetsCompletedAtAndCompletedByUserId()
    {
        // Arrange: Create handover in InProgress state with coverage
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var senderUserId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create receiver user and patient using helpers
        var receiverUserId = await CreateTestUserAsync($"user-receiver-{testRunId}", null, $"receiver{testRunId}@test.com", "Receiver", "User");
        var patientId = await CreateTestPatientAsync($"complete-{testRunId}", unitId);

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

        // Verify handover is in InProgress state using public API
        var inProgressHandover = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(inProgressHandover);
        Assert.Equal("InProgress", inProgressHandover.Handover.Status);
        Assert.NotNull(inProgressHandover.Handover.StartedAt);

        // Act: Complete handover
        var success = await repository.CompleteHandoverAsync(handover.Id, receiverUserId);

        // Assert: Should succeed and update state
        Assert.True(success, "CompleteHandoverAsync should succeed when handover is InProgress and user has coverage in TO shift");

        var updated = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(updated);
        Assert.Equal("Completed", updated.Handover.Status);
        Assert.NotNull(updated.Handover.CompletedAt);
        Assert.Equal(receiverUserId, updated.Handover.CompletedByUserId);
    }

    [Fact]
    public async Task CompleteHandover_FailsWhenNotStarted()
    {
        // Arrange: Create handover in Ready state (not started)
        // Business rule: Cannot complete handover that is not in InProgress state
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var senderUserId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create receiver user and patient using helpers
        var receiverUserId = await CreateTestUserAsync($"user-receiver-{testRunId}", null, $"receiver{testRunId}@test.com", "Receiver", "User");
        var patientId = await CreateTestPatientAsync($"complete-notstarted-{testRunId}", unitId);

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
        
        // Mark as Ready (but don't start)
        var readySuccess = await repository.MarkAsReadyAsync(handover.Id, senderUserId);
        Assert.True(readySuccess, "MarkAsReadyAsync should succeed");

        // Verify handover is in Ready state (not started)
        var readyHandover = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(readyHandover);
        Assert.Equal("Ready", readyHandover.Handover.Status);
        Assert.Null(readyHandover.Handover.StartedAt);

        // Act: Try to complete handover that is not started
        var success = await repository.CompleteHandoverAsync(handover.Id, receiverUserId);

        // Assert: Should fail - business rule: cannot complete handover that is not in InProgress state
        Assert.False(success, "CompleteHandoverAsync should fail when handover is not in InProgress state");

        var handoverAfterAttempt = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(handoverAfterAttempt);
        Assert.Equal("Ready", handoverAfterAttempt.Handover.Status);
        Assert.Null(handoverAfterAttempt.Handover.CompletedAt);
    }

    [Fact]
    public async Task CompleteHandover_FailsWhenSenderTriesToComplete()
    {
        // Arrange: Create handover in InProgress state, sender tries to complete
        // Business rule: Only receiver (user with coverage in TO shift) can complete, not sender
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var senderUserId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create receiver user and patient using helpers
        var receiverUserId = await CreateTestUserAsync($"user-receiver-{testRunId}", null, $"receiver{testRunId}@test.com", "Receiver", "User");
        var patientId = await CreateTestPatientAsync($"complete-sender-{testRunId}", unitId);

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

        // Act: Sender tries to complete (should fail - business rule: sender cannot complete)
        var success = await repository.CompleteHandoverAsync(handover.Id, senderUserId);

        // Assert: Should fail - business rule: sender cannot complete handover
        Assert.False(success, "CompleteHandoverAsync should fail when sender tries to complete");

        var handoverAfterAttempt = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(handoverAfterAttempt);
        Assert.Equal("InProgress", handoverAfterAttempt.Handover.Status);
        Assert.Null(handoverAfterAttempt.Handover.CompletedAt);
    }

    [Fact]
    public async Task CompleteHandover_FailsWhenAlreadyCompleted()
    {
        // Arrange: Create handover and complete it, then try to complete again
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var senderUserId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create receiver user and patient using helpers
        var receiverUserId = await CreateTestUserAsync($"user-receiver-{testRunId}", null, $"receiver{testRunId}@test.com", "Receiver", "User");
        var patientId = await CreateTestPatientAsync($"complete-already-{testRunId}", unitId);

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

        // Complete handover
        var completeSuccess = await repository.CompleteHandoverAsync(handover.Id, receiverUserId);
        Assert.True(completeSuccess, "CompleteHandoverAsync should succeed");

        // Verify handover is completed
        var completedHandover = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(completedHandover);
        Assert.Equal("Completed", completedHandover.Handover.Status);
        Assert.NotNull(completedHandover.Handover.CompletedAt);

        // Act: Try to complete again
        var success = await repository.CompleteHandoverAsync(handover.Id, receiverUserId);

        // Assert: Should fail - business rule: cannot complete handover that is already completed
        Assert.False(success, "CompleteHandoverAsync should fail when handover is already completed");

        var handoverAfterAttempt = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(handoverAfterAttempt);
        Assert.Equal("Completed", handoverAfterAttempt.Handover.Status);
    }

    [Fact]
    public async Task CompleteHandover_FailsWhenCancelled()
    {
        // Arrange: Create handover, start it, then cancel it
        // Business rule: Cannot complete handover that is cancelled
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var senderUserId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create receiver user and patient using helpers
        var receiverUserId = await CreateTestUserAsync($"user-receiver-{testRunId}", null, $"receiver{testRunId}@test.com", "Receiver", "User");
        var patientId = await CreateTestPatientAsync($"complete-cancelled-{testRunId}", unitId);

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

        // Cancel handover
        var cancelSuccess = await repository.CancelHandoverAsync(handover.Id, "TestCancel", senderUserId);
        Assert.True(cancelSuccess, "CancelHandoverAsync should succeed");

        // Verify handover is cancelled
        var cancelledHandover = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(cancelledHandover);
        Assert.Equal("Cancelled", cancelledHandover.Handover.Status);
        Assert.NotNull(cancelledHandover.Handover.CancelledAt);

        // Act: Try to complete cancelled handover
        var success = await repository.CompleteHandoverAsync(handover.Id, receiverUserId);

        // Assert: Should fail - business rule: cannot complete handover that is cancelled
        Assert.False(success, "CompleteHandoverAsync should fail when handover is cancelled");

        var handoverAfterAttempt = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(handoverAfterAttempt);
        Assert.Equal("Cancelled", handoverAfterAttempt.Handover.Status);
    }
}

