using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryDeleteContingencyPlanTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryDeleteContingencyPlanTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task DeleteContingencyPlan_DeletesExistingPlan()
    {
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;
        var planId = DapperTestSeeder.ContingencyPlanId;

        var deleted = await repository.DeleteContingencyPlanAsync(handoverId, planId);
        Assert.True(deleted);

        var plans = await repository.GetContingencyPlansAsync(handoverId);
        Assert.DoesNotContain(plans, p => p.Id == planId);
    }
}
