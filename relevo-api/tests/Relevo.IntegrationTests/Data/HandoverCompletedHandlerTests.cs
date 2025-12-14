using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Relevo.Core.Events;
using Relevo.Core.Handlers;
using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Xunit;

namespace Relevo.IntegrationTests.Data;

/// <summary>
/// Tests for automatic next handover creation when a handover is completed.
/// V3_PLAN.md Regla #15: "al completar, el receptor 'toma' el pase y el próximo handover lo tendrá como emisor"
/// V3_PLAN.md Regla #52: "El patient summary se copia/arrastra del handover previo al nuevo"
/// </summary>
public class HandoverCompletedHandlerTests : BaseDapperRepoTestFixture
{
    public HandoverCompletedHandlerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private HandoverCompletedHandler GetHandler()
    {
        var handoverRepo = _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
        var shiftTransitionService = _scope.ServiceProvider.GetRequiredService<IShiftTransitionService>();
        var logger = _scope.ServiceProvider.GetRequiredService<ILogger<HandoverCompletedHandler>>();
        return new HandoverCompletedHandler(handoverRepo, shiftTransitionService, logger);
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    private System.Data.IDbConnection GetConnection()
    {
        var connectionFactory = _scope.ServiceProvider.GetRequiredService<Relevo.Infrastructure.Data.DapperConnectionFactory>();
        return connectionFactory.CreateConnection();
    }

    [Fact]
    public async Task Handle_CompletedHandover_CreatesNextHandoverAutomatically()
    {
        // Arrange
        var handler = GetHandler();
        var handoverRepo = GetHandoverRepository();
        var conn = GetConnection();
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var shiftDayId = DapperTestSeeder.ShiftDayId;
        var shiftNightId = DapperTestSeeder.ShiftNightId;

        try
        {
            // Create test data
            var patientId = await CreateTestPatientAsync(testRunId, unitId);
            var senderUserId = await CreateTestUserAsync(testRunId + "-sender");
            var receiverUserId = await CreateTestUserAsync(testRunId + "-receiver");
            
            // Create shift instances for today
            var (dayInstanceId, nightInstanceId) = await CreateShiftInstancesForTodayAsync(shiftDayId, shiftNightId, unitId);
            
            // Create coverage for sender in day shift (FROM shift)
            await CreateCoverageAsync(testRunId + "-s", senderUserId, patientId, dayInstanceId, unitId, isPrimary: true);
            
            // Create coverage for receiver in night shift (TO shift)
            await CreateCoverageAsync(testRunId + "-r", receiverUserId, patientId, nightInstanceId, unitId, isPrimary: true);
            
            // Create a handover from Day -> Night
            var createRequest = new Relevo.Core.Models.CreateHandoverRequest(
                patientId, senderUserId, null, shiftDayId, shiftNightId, senderUserId, "Test handover"
            );
            var originalHandover = await handoverRepo.CreateHandoverAsync(createRequest);
            
            // Set patient summary content to verify copy
            await handoverRepo.UpdateClinicalDataAsync(originalHandover.Id, "Stable", "Test patient summary content", senderUserId);
            
            // Mark as ready and start (transitions needed before complete)
            await handoverRepo.MarkAsReadyAsync(originalHandover.Id, senderUserId);
            await handoverRepo.StartHandoverAsync(originalHandover.Id, receiverUserId);
            await handoverRepo.CompleteHandoverAsync(originalHandover.Id, receiverUserId);

            // Create event simulating completion
            var domainEvent = new HandoverCompletedEvent(
                originalHandover.Id,
                patientId,
                receiverUserId,
                shiftNightId, // Night shift was TO shift, now becomes FROM shift
                unitId
            );

            // Act
            await handler.Handle(domainEvent, CancellationToken.None);

            // Assert: Verify next handover was created
            // Give it a moment for async processing
            await Task.Delay(100);
            
            var handovers = await conn.QueryAsync<dynamic>(@"
                SELECT h.ID, h.PATIENT_ID, h.CURRENT_STATE, h.SENDER_USER_ID, h.PREVIOUS_HANDOVER_ID
                FROM HANDOVERS h
                WHERE h.PATIENT_ID = :PatientId
                ORDER BY h.CREATED_AT DESC",
                new { PatientId = patientId });

            var handoverList = handovers.ToList();
            
            // Should have 2 handovers: original (Completed) + new (Draft)
            Assert.Equal(2, handoverList.Count);
            
            var newHandover = handoverList.First();
            Assert.Equal("Draft", (string)newHandover.CURRENT_STATE);
            Assert.Equal(receiverUserId, (string?)newHandover.SENDER_USER_ID); // Receiver becomes sender
            Assert.Equal(originalHandover.Id, (string?)newHandover.PREVIOUS_HANDOVER_ID); // Linked to previous
            
            // Verify patient summary was copied
            var newHandoverContent = await conn.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT PATIENT_SUMMARY FROM HANDOVER_CONTENTS WHERE HANDOVER_ID = :HandoverId",
                new { HandoverId = (string)newHandover.ID });
            
            Assert.NotNull(newHandoverContent);
            Assert.Equal("Test patient summary content", (string?)newHandoverContent!.PATIENT_SUMMARY);
        }
        finally
        {
            conn?.Dispose();
        }
    }

    [Fact]
    public async Task Handle_ExistingNextHandover_DoesNotCreateDuplicate()
    {
        // Arrange
        var handler = GetHandler();
        var handoverRepo = GetHandoverRepository();
        var shiftTransitionService = _scope.ServiceProvider.GetRequiredService<IShiftTransitionService>();
        var conn = GetConnection();
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var shiftDayId = DapperTestSeeder.ShiftDayId;
        var shiftNightId = DapperTestSeeder.ShiftNightId;

        try
        {
            // Create test data
            var patientId = await CreateTestPatientAsync(testRunId, unitId);
            var senderUserId = await CreateTestUserAsync(testRunId + "-sender");
            var receiverUserId = await CreateTestUserAsync(testRunId + "-receiver");
            
            // Create shift instances for today
            var (dayInstanceId, nightInstanceId) = await CreateShiftInstancesForTodayAsync(shiftDayId, shiftNightId, unitId);
            
            // Create coverage for both shifts
            await CreateCoverageAsync(testRunId + "-s", senderUserId, patientId, dayInstanceId, unitId, isPrimary: true);
            await CreateCoverageAsync(testRunId + "-r", receiverUserId, patientId, nightInstanceId, unitId, isPrimary: true);
            
            // Create first handover (Day -> Night) - this one will be "completed"
            var createRequest1 = new Relevo.Core.Models.CreateHandoverRequest(
                patientId, senderUserId, null, shiftDayId, shiftNightId, senderUserId, "First handover"
            );
            var originalHandover = await handoverRepo.CreateHandoverAsync(createRequest1);
            
            // Mark as ready, start, complete
            await handoverRepo.MarkAsReadyAsync(originalHandover.Id, senderUserId);
            await handoverRepo.StartHandoverAsync(originalHandover.Id, receiverUserId);
            await handoverRepo.CompleteHandoverAsync(originalHandover.Id, receiverUserId);
            
            // Manually create the next handover (Night -> Day) BEFORE the event fires
            // This simulates an existing handover created by assignment
            var nextShiftId = await shiftTransitionService.GetNextShiftIdAsync(shiftNightId);
            Assert.NotNull(nextShiftId);
            
            var createRequest2 = new Relevo.Core.Models.CreateHandoverRequest(
                patientId, receiverUserId, null, shiftNightId, nextShiftId!, receiverUserId, "Pre-existing next handover"
            );
            var preExistingHandover = await handoverRepo.CreateHandoverAsync(createRequest2);

            // Create event simulating completion
            var domainEvent = new HandoverCompletedEvent(
                originalHandover.Id,
                patientId,
                receiverUserId,
                shiftNightId,
                unitId
            );

            // Act
            await handler.Handle(domainEvent, CancellationToken.None);

            // Assert: Still only 2 handovers (no duplicate created)
            await Task.Delay(100);
            
            var handoverCount = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM HANDOVERS WHERE PATIENT_ID = :PatientId",
                new { PatientId = patientId });

            Assert.Equal(2, handoverCount);
        }
        finally
        {
            conn?.Dispose();
        }
    }

    [Fact]
    public async Task Handle_ReceiverHasCoverageInToShift_CreatesNextHandover()
    {
        // This test verifies that when the receiver has coverage in the TO shift (Night),
        // and that shift becomes the FROM shift of the next handover (Night -> Day),
        // the next handover IS created because coverage exists.
        // 
        // This is the expected behavior: the receiver taking responsibility
        // means they have coverage in that shift, which enables creating the next handover.
        
        // Arrange
        var handler = GetHandler();
        var handoverRepo = GetHandoverRepository();
        var conn = GetConnection();
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var shiftDayId = DapperTestSeeder.ShiftDayId;
        var shiftNightId = DapperTestSeeder.ShiftNightId;

        try
        {
            // Create test data
            var patientId = await CreateTestPatientAsync(testRunId, unitId);
            var senderUserId = await CreateTestUserAsync(testRunId + "-sender");
            var receiverUserId = await CreateTestUserAsync(testRunId + "-receiver");
            
            // Create shift instances for today
            var (dayInstanceId, nightInstanceId) = await CreateShiftInstancesForTodayAsync(shiftDayId, shiftNightId, unitId);
            
            // Create coverage for sender in Day (FROM of first handover)
            // Create coverage for receiver in Night (TO of first handover, becomes FROM of next)
            await CreateCoverageAsync(testRunId + "-s", senderUserId, patientId, dayInstanceId, unitId, isPrimary: true);
            await CreateCoverageAsync(testRunId + "-r", receiverUserId, patientId, nightInstanceId, unitId, isPrimary: true);
            
            // Create first handover (Day -> Night)
            var createRequest = new Relevo.Core.Models.CreateHandoverRequest(
                patientId, senderUserId, null, shiftDayId, shiftNightId, senderUserId, "Test handover"
            );
            var originalHandover = await handoverRepo.CreateHandoverAsync(createRequest);
            
            // Mark as ready, start, complete
            await handoverRepo.MarkAsReadyAsync(originalHandover.Id, senderUserId);
            await handoverRepo.StartHandoverAsync(originalHandover.Id, receiverUserId);
            await handoverRepo.CompleteHandoverAsync(originalHandover.Id, receiverUserId);

            // Create event simulating completion
            var domainEvent = new HandoverCompletedEvent(
                originalHandover.Id,
                patientId,
                receiverUserId,
                shiftNightId, // Night was TO, now becomes FROM of next handover
                unitId
            );

            // Act
            await handler.Handle(domainEvent, CancellationToken.None);

            // Assert: 2 handovers - the receiver had coverage in Night (now FROM shift of next),
            // so the next handover (Night -> Day) was created
            await Task.Delay(100);
            
            var handoverCount = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM HANDOVERS WHERE PATIENT_ID = :PatientId",
                new { PatientId = patientId });

            // Both handovers should exist: original (Completed) + next (Draft)
            Assert.Equal(2, handoverCount);
        }
        finally
        {
            conn?.Dispose();
        }
    }
}
