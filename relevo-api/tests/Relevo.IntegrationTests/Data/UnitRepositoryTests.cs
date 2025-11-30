using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class UnitRepositoryTests : BaseDapperRepoTestFixture
{
    public UnitRepositoryTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IUnitRepository GetUnitRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IUnitRepository>();
    }

    [Fact]
    public async Task GetUnits_ReturnsUnits()
    {
        var repository = GetUnitRepository();
        var unitId = DapperTestSeeder.UnitId;

        var units = await repository.GetUnitsAsync();

        Assert.NotEmpty(units);
        Assert.Contains(units, u => u.Id == unitId);
    }
}
