using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryUpdateSituationAwarenessTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryUpdateSituationAwarenessTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task UpdateSituationAwareness_UpdatesExistingRecord()
    {
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;
        var userId = DapperTestSeeder.UserId;

        await repository.GetSituationAwarenessAsync(handoverId);

        var success = await repository.UpdateSituationAwarenessAsync(handoverId, "Updated SA", "Final", userId);

        Assert.True(success);

        var updated = await repository.GetSituationAwarenessAsync(handoverId);
        Assert.Equal("Updated SA", updated?.Content);
        Assert.Equal("Final", updated?.Status);
        Assert.Equal(userId, updated?.LastEditedBy);
    }
}
