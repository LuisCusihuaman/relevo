using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryGetSynthesisTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryGetSynthesisTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task GetSynthesis_ReturnsSynthesis()
    {
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;
        
        var synthesis = await repository.GetSynthesisAsync(handoverId);

        Assert.NotNull(synthesis);
        Assert.Equal(handoverId, synthesis.HandoverId);
    }
}
