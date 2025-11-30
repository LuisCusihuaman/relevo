using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class ShiftRepositoryTests : BaseDapperRepoTestFixture
{
    public ShiftRepositoryTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IShiftRepository GetShiftRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IShiftRepository>();
    }

    [Fact]
    public async Task GetShifts_ReturnsShifts()
    {
        var repository = GetShiftRepository();
        var shiftDayId = DapperTestSeeder.ShiftDayId;
        var shiftNightId = DapperTestSeeder.ShiftNightId;

        var shifts = await repository.GetShiftsAsync();

        Assert.NotEmpty(shifts);
        Assert.Contains(shifts, s => s.Id == shiftDayId);
        Assert.Contains(shifts, s => s.Id == shiftNightId);
    }
}
