using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryUpdateSynthesisTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryUpdateSynthesisTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task UpdateSynthesis_UpdatesContent()
    {
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;
        var content = "Updated synthesis content";
        var userId = DapperTestSeeder.UserId;

        var updated = await repository.UpdateSynthesisAsync(handoverId, content, "Draft", userId);
        Assert.True(updated);

        var synthesis = await repository.GetSynthesisAsync(handoverId);
        Assert.NotNull(synthesis);
        Assert.Equal(content, synthesis.Content);
    }
}
