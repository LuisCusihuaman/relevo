using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Dapper;

namespace Relevo.IntegrationTests.Data;

/// <summary>
/// Tests for PATIENT_SUMMARY copy from previous handover (V3_PLAN.md Regla #36)
/// When creating a new handover, the PATIENT_SUMMARY should be copied from the most recent completed handover
/// </summary>
public class HandoverRepositoryPatientSummaryCopyTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryPatientSummaryCopyTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    private IPatientRepository GetPatientRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IPatientRepository>();
    }

    [Fact]
    public async Task CreateHandover_FirstHandover_HasNullPatientSummary()
    {
        // Arrange: Create first handover (no previous handover exists)
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var userId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        // Create patient using helper
        var patientId = await CreateTestPatientAsync($"ps-first-{testRunId}", unitId);

        // Create shift instances using helper
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        // Create coverage using helper
        await CreateCoverageAsync(testRunId, userId, patientId, fromShiftInstanceId, unitId);

        // Act: Create first handover
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

        var firstHandover = await repository.CreateHandoverAsync(createRequest);

        // Assert: First handover should have NULL or empty PATIENT_SUMMARY (no previous handover to copy from)
        var handoverDetail = await repository.GetHandoverByIdAsync(firstHandover.Id);
        Assert.NotNull(handoverDetail);
        Assert.Null(firstHandover.PreviousHandoverId); // No previous handover
        // PATIENT_SUMMARY should be NULL or empty since there's no previous handover
        Assert.True(string.IsNullOrEmpty(handoverDetail.Handover.PatientSummary));
    }

    [Fact]
    public async Task CreateHandover_CopiesPatientSummary_FromPreviousCompletedHandover()
    {
        // Arrange: Create and complete first handover with PATIENT_SUMMARY
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var senderUserId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        var receiverUserId = await CreateTestUserAsync($"receiver-{testRunId}", null, $"receiver{testRunId}@test.com", "Receiver", "User");
        var patientId = await CreateTestPatientAsync($"ps-copy-{testRunId}", unitId);

        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);

        await CreateCoverageAsync($"from-{testRunId}", senderUserId, patientId, fromShiftInstanceId, unitId);
        await CreateCoverageAsync($"to-{testRunId}", receiverUserId, patientId, toShiftInstanceId, unitId);

        var repository = GetHandoverRepository();
        var patientRepository = GetPatientRepository();
        
        // Create first handover
        var createRequest1 = new CreateHandoverRequest(
            patientId, senderUserId, receiverUserId, fromShiftId, toShiftId, senderUserId, null);
        var firstHandover = await repository.CreateHandoverAsync(createRequest1);

        // Set PATIENT_SUMMARY in first handover
        const string originalSummary = "Patient has been stable. No significant changes. Monitoring continues.";
        await patientRepository.CreatePatientSummaryAsync(firstHandover.Id, originalSummary, senderUserId);

        // Complete first handover
        await repository.MarkAsReadyAsync(firstHandover.Id, senderUserId);
        await repository.StartHandoverAsync(firstHandover.Id, receiverUserId);
        await repository.CompleteHandoverAsync(firstHandover.Id, receiverUserId);

        // Verify first handover is completed with summary
        var completedDetail = await repository.GetHandoverByIdAsync(firstHandover.Id);
        Assert.NotNull(completedDetail);
        Assert.Equal("Completed", completedDetail.Handover.Status);
        Assert.Equal(originalSummary, completedDetail.Handover.PatientSummary);

        // Act: Create a completed handover for YESTERDAY using helper
        // This simulates a previous handover that would be found by CreateHandoverAsync
        var yesterday = DateTime.Today.AddDays(-1);
        var (yesterdayWindowId, yesterdayFromInstanceId, yesterdayToInstanceId) = 
            await CreateShiftWindowForDateAsync(fromShiftId, toShiftId, unitId, yesterday);

        // Create coverage for yesterday
        await CreateCoverageAsync($"yesterday-from-{testRunId}", senderUserId, patientId, yesterdayFromInstanceId, unitId);
        await CreateCoverageAsync($"yesterday-to-{testRunId}", receiverUserId, patientId, yesterdayToInstanceId, unitId);

        // Create completed handover with PATIENT_SUMMARY
        var previousHandoverId = await CreateCompletedHandoverDirectlyAsync(
            testRunId,
            patientId,
            senderUserId,
            receiverUserId,
            yesterdayWindowId,
            unitId,
            previousHandoverId: null,
            patientSummary: originalSummary);

        // Delete the first handover we created (for today) so we can create a new one
        // that will reference the completed handover from yesterday
        var connectionFactory = GetConnectionFactory();
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM HANDOVER_CONTENTS WHERE HANDOVER_ID = :handoverId",
            new { handoverId = firstHandover.Id });
        await conn.ExecuteAsync("DELETE FROM HANDOVERS WHERE ID = :handoverId",
            new { handoverId = firstHandover.Id });

        // Create new handover for today - should copy PATIENT_SUMMARY from previousHandoverId
        var createRequest2 = new CreateHandoverRequest(
            patientId, senderUserId, receiverUserId, fromShiftId, toShiftId, senderUserId, null);
        var newHandover = await repository.CreateHandoverAsync(createRequest2);

        // Assert: New handover should have PATIENT_SUMMARY copied from previous handover
        var newHandoverDetail = await repository.GetHandoverByIdAsync(newHandover.Id);
        Assert.NotNull(newHandoverDetail);
        Assert.Equal(previousHandoverId, newHandoverDetail.Handover.PreviousHandoverId);
        Assert.Equal(originalSummary, newHandoverDetail.Handover.PatientSummary);
    }

    [Fact]
    public async Task CreateHandover_DoesNotCopyPatientSummary_WhenPreviousHandoverHasNoSummary()
    {
        // Arrange: Create completed handover WITHOUT PATIENT_SUMMARY
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var senderUserId = DapperTestSeeder.UserId;
        var fromShiftId = DapperTestSeeder.ShiftDayId;
        var toShiftId = DapperTestSeeder.ShiftNightId;

        var receiverUserId = await CreateTestUserAsync($"receiver-no-{testRunId}", null, $"receiver{testRunId}@test.com", "Receiver", "User");
        var patientId = await CreateTestPatientAsync($"ps-no-summary-{testRunId}", unitId);

        // Create shift window for yesterday
        var yesterday = DateTime.Today.AddDays(-1);
        var (yesterdayWindowId, yesterdayFromInstanceId, yesterdayToInstanceId) = 
            await CreateShiftWindowForDateAsync(fromShiftId, toShiftId, unitId, yesterday);

        await CreateCoverageAsync($"yesterday-from-{testRunId}", senderUserId, patientId, yesterdayFromInstanceId, unitId);
        await CreateCoverageAsync($"yesterday-to-{testRunId}", receiverUserId, patientId, yesterdayToInstanceId, unitId);

        // Create completed handover WITHOUT PATIENT_SUMMARY
        var previousHandoverId = await CreateCompletedHandoverDirectlyAsync(
            $"no-{testRunId}",
            patientId,
            senderUserId,
            receiverUserId,
            yesterdayWindowId,
            unitId,
            previousHandoverId: null,
            patientSummary: null);

        // Create shift instances for today
        var (fromShiftInstanceId, toShiftInstanceId) = await CreateShiftInstancesForTodayAsync(
            fromShiftId, toShiftId, unitId);
        await CreateCoverageAsync($"today-from-{testRunId}", senderUserId, patientId, fromShiftInstanceId, unitId);
        await CreateCoverageAsync($"today-to-{testRunId}", receiverUserId, patientId, toShiftInstanceId, unitId);

        // Act: Create new handover for today
        var repository = GetHandoverRepository();
        var createRequest = new CreateHandoverRequest(
            patientId, senderUserId, receiverUserId, fromShiftId, toShiftId, senderUserId, null);
        var newHandover = await repository.CreateHandoverAsync(createRequest);

        // Assert: New handover should have NULL or empty PATIENT_SUMMARY (previous had none)
        var newHandoverDetail = await repository.GetHandoverByIdAsync(newHandover.Id);
        Assert.NotNull(newHandoverDetail);
        Assert.Equal(previousHandoverId, newHandoverDetail.Handover.PreviousHandoverId);
        Assert.True(string.IsNullOrEmpty(newHandoverDetail.Handover.PatientSummary));
    }
}

