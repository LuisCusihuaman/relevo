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

        var shifts = await repository.GetShiftsAsync();

        Assert.NotEmpty(shifts);
        Assert.Contains(shifts, s => s.Id == "shift-day");
        Assert.Contains(shifts, s => s.Id == "shift-night");
    }
}

