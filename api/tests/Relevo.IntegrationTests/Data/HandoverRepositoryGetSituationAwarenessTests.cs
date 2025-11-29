using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryGetSituationAwarenessTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryGetSituationAwarenessTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task GetSituationAwareness_ReturnsRecord()
    {
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;

        var sa = await repository.GetSituationAwarenessAsync(handoverId);

        Assert.NotNull(sa);
        Assert.Equal(handoverId, sa.HandoverId);
    }
}
