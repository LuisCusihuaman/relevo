using Dapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Relevo.Core.Events;
using Relevo.Core.Handlers;
using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Relevo.Infrastructure.Data;
using System.Data;
using Xunit;

namespace Relevo.IntegrationTests.Data;

/// <summary>
/// Tests for automatic handover creation when patients are assigned to shifts.
/// V3_PLAN.md Regla #14: Handovers are created as side effects of domain commands.
/// </summary>
public class PatientAssignedToShiftHandlerTests : BaseDapperRepoTestFixture
{
    public PatientAssignedToShiftHandlerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private PatientAssignedToShiftHandler GetHandler()
    {
        var handoverRepo = _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
        var shiftTransitionService = _scope.ServiceProvider.GetRequiredService<IShiftTransitionService>();
        var logger = _scope.ServiceProvider.GetRequiredService<ILogger<PatientAssignedToShiftHandler>>();
        return new PatientAssignedToShiftHandler(handoverRepo, shiftTransitionService, logger);
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
    public async Task Handle_PrimaryAssignment_CreatesHandoverAutomatically()
    {
        // Arrange
        var handler = GetHandler();
        var conn = GetConnection();
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var shiftDayId = DapperTestSeeder.ShiftDayId;
        var shiftNightId = DapperTestSeeder.ShiftNightId;

        try
        {
            // Use helpers to create test data
            var patientId = await CreateTestPatientAsync(testRunId, unitId);
            var userId = await CreateTestUserAsync(testRunId);
            var (dayInstanceId, _) = await CreateShiftInstancesForTodayAsync(shiftDayId, shiftNightId, unitId);
            await CreateCoverageAsync(testRunId, userId, patientId, dayInstanceId, unitId, isPrimary: true);

            // Create event using helper
            var domainEvent = await CreatePatientAssignedToShiftEventAsync(
                patientId, userId, shiftDayId, dayInstanceId, unitId, isPrimary: true);

            // Act
            await handler.Handle(domainEvent, CancellationToken.None);

            // Assert: Verify handover was created
            var handovers = await conn.QueryAsync<dynamic>(@"
                SELECT h.ID, h.PATIENT_ID, h.CURRENT_STATE, h.SENDER_USER_ID, h.CREATED_BY_USER_ID
                FROM HANDOVERS h
                WHERE h.PATIENT_ID = :PatientId",
                new { PatientId = patientId });

            var handoverList = handovers.ToList();
            Assert.Single(handoverList);
            var handover = handoverList.First();
            Assert.NotNull(handover);
            Assert.Equal(patientId, (string)handover!.PATIENT_ID);
            Assert.Equal("Draft", (string)handover.CURRENT_STATE);
            Assert.Equal(userId, (string?)handover.SENDER_USER_ID);
            Assert.Equal(userId, (string?)handover.CREATED_BY_USER_ID);
        }
        finally
        {
            conn?.Dispose();
        }
    }

    [Fact]
    public async Task Handle_NonPrimaryAssignment_DoesNotCreateHandover()
    {
        // Arrange
        var handler = GetHandler();
        var conn = GetConnection();
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var shiftDayId = DapperTestSeeder.ShiftDayId;
        var shiftNightId = DapperTestSeeder.ShiftNightId;

        try
        {
            // Use helpers to create test data
            var patientId = await CreateTestPatientAsync(testRunId, unitId);
            var userId = await CreateTestUserAsync(testRunId);
            var (dayInstanceId, _) = await CreateShiftInstancesForTodayAsync(shiftDayId, shiftNightId, unitId);

            // Create event with IsPrimary=false using helper
            var domainEvent = await CreatePatientAssignedToShiftEventAsync(
                patientId, userId, shiftDayId, dayInstanceId, unitId, isPrimary: false);

            // Act
            await handler.Handle(domainEvent, CancellationToken.None);

            // Assert: No handover should be created
            var handoverCount = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM HANDOVERS WHERE PATIENT_ID = :PatientId",
                new { PatientId = patientId });

            Assert.Equal(0, handoverCount);
        }
        finally
        {
            conn?.Dispose();
        }
    }

    [Fact]
    public async Task Handle_ExistingHandover_DoesNotCreateDuplicate()
    {
        // Arrange
        var handler = GetHandler();
        var handoverRepo = GetHandoverRepository();
        var shiftTransitionService = _scope.ServiceProvider.GetRequiredService<IShiftTransitionService>();
        var conn = GetConnection();
        var testRunId = Guid.NewGuid().ToString()[..8];
        var unitId = DapperTestSeeder.UnitId;
        var shiftDayId = DapperTestSeeder.ShiftDayId;

        try
        {
            // Use helpers to create test data
            var patientId = await CreateTestPatientAsync(testRunId, unitId);
            var userId = await CreateTestUserAsync(testRunId);
            
            // Get next shift ID (same logic as handler uses)
            var nextShiftId = await shiftTransitionService.GetNextShiftIdAsync(shiftDayId);
            if (string.IsNullOrEmpty(nextShiftId))
            {
                throw new InvalidOperationException($"Next shift not found for {shiftDayId}");
            }
            
            var (dayInstanceId, _) = await CreateShiftInstancesForTodayAsync(shiftDayId, nextShiftId, unitId);
            await CreateCoverageAsync(testRunId, userId, patientId, dayInstanceId, unitId, isPrimary: true);

            // Create handover manually first (using same nextShiftId that handler will use)
            var createRequest = new Relevo.Core.Models.CreateHandoverRequest(
                patientId, userId, null, shiftDayId, nextShiftId, userId, "Existing handover"
            );
            await handoverRepo.CreateHandoverAsync(createRequest);

            // Act: Try to create handover via event (should be idempotent)
            var domainEvent = await CreatePatientAssignedToShiftEventAsync(
                patientId, userId, shiftDayId, dayInstanceId, unitId, isPrimary: true);
            await handler.Handle(domainEvent, CancellationToken.None);

            // Assert: Still only one handover
            var handoverCount = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM HANDOVERS WHERE PATIENT_ID = :PatientId",
                new { PatientId = patientId });

            Assert.Equal(1, handoverCount);
        }
        finally
        {
            conn?.Dispose();
        }
    }
}

