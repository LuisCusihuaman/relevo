using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Dapper;
using Relevo.Infrastructure.Data;

namespace Relevo.IntegrationTests.Data;

/// <summary>
/// Tests for StartHandoverAsync
/// V3_PLAN.md regla #22: quien start debe tener coverage en TO shift, NO puede ser sender
/// Constraint CHK_HO_STARTED_NE_SENDER ensures STARTED_BY_USER_ID <> SENDER_USER_ID
/// Constraint CHK_HO_ST_REQ_RD ensures STARTED_AT requires READY_AT
/// </summary>
public class HandoverRepositoryStartHandoverTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryStartHandoverTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task StartHandover_SetsStartedAtAndStartedByUserId()
    {
        // Arrange: Create handover in Ready state with coverage
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var senderUserId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create receiver user and patient using helpers
        var receiverUserId = await CreateTestUserAsync($"user-receiver-{testRunId}", null, $"receiver{testRunId}@test.com", "Receiver", "User");
        var patientId = await CreateTestPatientAsync($"start-{testRunId}", unitId);

        // Create shift instances using helper
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        // Create coverage using helpers
        await CreateCoverageAsync($"sender-{testRunId}", senderUserId, patientId, fromShiftInstanceId, unitId);
        await CreateCoverageAsync($"receiver-{testRunId}", receiverUserId, patientId, toShiftInstanceId, unitId);

        // Create handover using production methods
        var repository = GetHandoverRepository();
        var createRequest = new Relevo.Core.Models.CreateHandoverRequest(
            patientId,
            senderUserId,
            receiverUserId,
            fromShiftId,
            toShiftId,
            senderUserId,
            null
        );

        var handover = await repository.CreateHandoverAsync(createRequest);
        
        // Mark as Ready using production method
        var readySuccess = await repository.MarkAsReadyAsync(handover.Id, senderUserId);
        Assert.True(readySuccess, "MarkAsReadyAsync should succeed");

        // Verify handover is in Ready state using public API
        var readyHandover = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(readyHandover);
        Assert.Equal("Ready", readyHandover.Handover.Status);
        Assert.NotNull(readyHandover.Handover.ReadyAt);

        // Act: Start handover
        var success = await repository.StartHandoverAsync(handover.Id, receiverUserId);

        // Assert: Should succeed and update state
        Assert.True(success, "StartHandoverAsync should succeed when handover is Ready and user has coverage in TO shift");

        var updated = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(updated);
        Assert.Equal("InProgress", updated.Handover.Status);
        Assert.NotNull(updated.Handover.StartedAt);
        Assert.Equal(receiverUserId, updated.Handover.StartedByUserId);
    }

    [Fact]
    public async Task StartHandover_FailsWhenNotReady()
    {
        // Arrange: Create handover in Draft state (not Ready)
        // Note: We create handover but don't mark it as Ready to test the business rule
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var senderUserId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create receiver user and patient using helpers
        var receiverUserId = await CreateTestUserAsync($"user-receiver-{testRunId}", null, $"receiver{testRunId}@test.com", "Receiver", "User");
        var patientId = await CreateTestPatientAsync($"start-draft-{testRunId}", unitId);

        // Create shift instances using helper
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        // Create coverage for receiver (TO shift) - required for start
        await CreateCoverageAsync($"receiver-{testRunId}", receiverUserId, patientId, toShiftInstanceId, unitId);
        
        // Create coverage for sender (FROM shift) - required for handover creation
        await CreateCoverageAsync($"sender-{testRunId}", senderUserId, patientId, fromShiftInstanceId, unitId);

        // Create handover using production method (will be in Draft state)
        var repository = GetHandoverRepository();
        var createRequest = new Relevo.Core.Models.CreateHandoverRequest(
            patientId,
            senderUserId,
            receiverUserId,
            fromShiftId,
            toShiftId,
            senderUserId,
            null
        );

        var handover = await repository.CreateHandoverAsync(createRequest);
        
        // Verify handover is in Draft state (not Ready)
        var draftHandover = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(draftHandover);
        Assert.Equal("Draft", draftHandover.Handover.Status);
        Assert.Null(draftHandover.Handover.ReadyAt);

        // Act: Try to start handover that is not Ready
        var success = await repository.StartHandoverAsync(handover.Id, receiverUserId);

        // Assert: Should fail - business rule: cannot start handover that is not Ready
        Assert.False(success, "StartHandoverAsync should fail when handover is not in Ready state");

        var handoverAfterAttempt = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(handoverAfterAttempt);
        Assert.Equal("Draft", handoverAfterAttempt.Handover.Status);
        Assert.Null(handoverAfterAttempt.Handover.StartedAt);
    }

    [Fact]
    public async Task StartHandover_FailsWhenSenderTriesToStart()
    {
        // Arrange: Create handover in Ready state, sender tries to start
        // Business rule: Only receiver (user with coverage in TO shift) can start, not sender
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var senderUserId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create receiver user and patient using helpers
        var receiverUserId = await CreateTestUserAsync($"user-receiver-{testRunId}", null, $"receiver{testRunId}@test.com", "Receiver", "User");
        var patientId = await CreateTestPatientAsync($"start-sender-{testRunId}", unitId);

        // Create shift instances using helper
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        // Create coverage for sender (FROM shift) and receiver (TO shift)
        await CreateCoverageAsync($"sender-{testRunId}", senderUserId, patientId, fromShiftInstanceId, unitId);
        await CreateCoverageAsync($"receiver-{testRunId}", receiverUserId, patientId, toShiftInstanceId, unitId);

        // Create handover using production methods
        var repository = GetHandoverRepository();
        var createRequest = new Relevo.Core.Models.CreateHandoverRequest(
            patientId,
            senderUserId,
            receiverUserId,
            fromShiftId,
            toShiftId,
            senderUserId,
            null
        );

        var handover = await repository.CreateHandoverAsync(createRequest);
        
        // Mark as Ready using production method
        var readySuccess = await repository.MarkAsReadyAsync(handover.Id, senderUserId);
        Assert.True(readySuccess, "MarkAsReadyAsync should succeed");

        // Verify handover is in Ready state
        var readyHandover = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(readyHandover);
        Assert.Equal("Ready", readyHandover.Handover.Status);

        // Act: Sender tries to start (should fail - business rule: sender cannot start)
        var success = await repository.StartHandoverAsync(handover.Id, senderUserId);

        // Assert: Should fail - business rule: sender cannot start handover
        Assert.False(success, "StartHandoverAsync should fail when sender tries to start");

        var handoverAfterAttempt = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(handoverAfterAttempt);
        Assert.Equal("Ready", handoverAfterAttempt.Handover.Status);
        Assert.Null(handoverAfterAttempt.Handover.StartedAt);
    }

    [Fact]
    public async Task StartHandover_FailsWhenAlreadyStarted()
    {
        // Arrange: Create handover and start it, then try to start again
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var senderUserId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create receiver user and patient using helpers
        var receiverUserId = await CreateTestUserAsync($"user-receiver-{testRunId}", null, $"receiver{testRunId}@test.com", "Receiver", "User");
        var patientId = await CreateTestPatientAsync($"start-already-{testRunId}", unitId);

        // Create shift instances using helper
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        // Create coverage for sender (FROM shift) and receiver (TO shift)
        await CreateCoverageAsync($"sender-{testRunId}", senderUserId, patientId, fromShiftInstanceId, unitId);
        await CreateCoverageAsync($"receiver-{testRunId}", receiverUserId, patientId, toShiftInstanceId, unitId);

        // Create handover using production methods
        var repository = GetHandoverRepository();
        var createRequest = new Relevo.Core.Models.CreateHandoverRequest(
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
        Assert.NotNull(inProgressHandover.Handover.StartedAt);

        // Act: Try to start again
        var success = await repository.StartHandoverAsync(handover.Id, receiverUserId);

        // Assert: Should fail - business rule: cannot start handover that is already started
        Assert.False(success, "StartHandoverAsync should fail when handover is already started");

        var handoverAfterAttempt = await repository.GetHandoverByIdAsync(handover.Id);
        Assert.NotNull(handoverAfterAttempt);
        Assert.Equal("InProgress", handoverAfterAttempt.Handover.Status);
    }
}

