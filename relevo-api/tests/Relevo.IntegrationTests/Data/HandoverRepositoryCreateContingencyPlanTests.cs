using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryCreateContingencyPlanTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryCreateContingencyPlanTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task CreateContingencyPlan_ReturnsCreatedPlan()
    {
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;
        var condition = "If Condition Met";
        var action = "Do Action";
        var priority = "medium"; // V3: Must be lowercase per CHK_CONT_PRIORITY constraint
        var userId = DapperTestSeeder.UserId;

        var plan = await repository.CreateContingencyPlanAsync(handoverId, condition, action, priority, userId);

        Assert.NotNull(plan);
        Assert.Equal(handoverId, plan.HandoverId);
        Assert.Equal(condition, plan.ConditionText);
        Assert.False(string.IsNullOrEmpty(plan.Id));
    }
}
