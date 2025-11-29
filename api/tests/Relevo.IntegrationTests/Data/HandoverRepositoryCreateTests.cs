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
        var repository = GetHandoverRepository();
        var patientId = DapperTestSeeder.PatientId1;
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
    }
}
