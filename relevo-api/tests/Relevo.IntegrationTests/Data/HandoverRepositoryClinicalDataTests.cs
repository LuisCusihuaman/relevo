using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryClinicalDataTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryClinicalDataTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task GetClinicalData_ReturnsRecord()
    {
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;

        var data = await repository.GetClinicalDataAsync(handoverId);

        Assert.NotNull(data);
        Assert.Equal(handoverId, data.HandoverId);
        Assert.NotNull(data.IllnessSeverity);
    }

    [Fact]
    public async Task UpdateClinicalData_UpdatesRecord()
    {
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;
        var userId = DapperTestSeeder.UserId;

        var success = await repository.UpdateClinicalDataAsync(handoverId, "Unstable", "Worsening", userId);

        Assert.True(success);

        var updated = await repository.GetClinicalDataAsync(handoverId);
        Assert.Equal("Unstable", updated?.IllnessSeverity);
        Assert.Equal("Worsening", updated?.SummaryText);
    }
}
