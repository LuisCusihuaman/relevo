using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryMarkAsReadyTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryMarkAsReadyTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task MarkAsReady_UpdatesStatusAndReadyAt()
    {
        var repository = GetHandoverRepository();
        var handoverId = "hvo-001";
        var userId = "dr-1";

        var success = await repository.MarkAsReadyAsync(handoverId, userId);

        Assert.True(success);

        var updated = await repository.GetHandoverByIdAsync(handoverId);
        Assert.NotNull(updated);
        Assert.Equal("Ready", updated.Handover.Status);
        Assert.NotNull(updated.Handover.ReadyAt);
    }
}

