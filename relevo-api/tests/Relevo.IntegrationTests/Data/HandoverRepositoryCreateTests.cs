using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryCreateTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryCreateTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task CreateHandover_CreatesAndReturnsHandover()
    {
        // V3: CreateHandoverAsync is refactored to use SHIFT_WINDOW_ID
        var repository = GetHandoverRepository();
        // Use PatientId2 to avoid conflict with seeded handover for PatientId1
        var patientId = DapperTestSeeder.PatientId2;
        var userId = DapperTestSeeder.UserId;
        var shiftDayId = DapperTestSeeder.ShiftDayId;
        var shiftNightId = DapperTestSeeder.ShiftNightId;
        
        var request = new CreateHandoverRequest(
            patientId, userId, userId, shiftDayId, shiftNightId, userId, "Test Notes"
        );

        var handover = await repository.CreateHandoverAsync(request);

        Assert.NotNull(handover);
        Assert.NotNull(handover.Id);
        Assert.Equal(patientId, handover.PatientId);
        Assert.Equal("Draft", handover.Status);
        Assert.NotNull(handover.ShiftWindowId); // V3: Should have SHIFT_WINDOW_ID
    }
}
