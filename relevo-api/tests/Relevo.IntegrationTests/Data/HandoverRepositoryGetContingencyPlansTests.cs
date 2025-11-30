using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryGetContingencyPlansTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryGetContingencyPlansTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task GetContingencyPlans_ReturnsPlans()
    {
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;
        var planId = DapperTestSeeder.ContingencyPlanId;
        
        var plans = await repository.GetContingencyPlansAsync(handoverId);

        Assert.NotEmpty(plans);
        Assert.Contains(plans, p => p.Id == planId);
    }
}
