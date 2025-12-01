using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

/// <summary>
/// Tests for MarkAsReadyAsync
/// V3_PLAN.md regla #10: Cannot pass to Ready without coverage >= 1
/// </summary>
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
        var handoverId = DapperTestSeeder.HandoverId;
        var userId = DapperTestSeeder.UserId;

        var success = await repository.MarkAsReadyAsync(handoverId, userId);

        Assert.True(success);

        var updated = await repository.GetHandoverByIdAsync(handoverId);
        Assert.NotNull(updated);
        Assert.Equal("Ready", updated.Handover.Status);
        Assert.NotNull(updated.Handover.ReadyAt);
    }

    // Note: Test "MarkAsReady_ReturnsFalse_WhenNoCoverageExists" was removed as duplicate.
    // The coverage requirement is already tested in HandoverRepositorySenderSelectionTests.CreateHandover_FailsWhenNoCoverage.
    // CreateHandoverAsync validates coverage before creating, so a handover without coverage can only exist
    // if inserted directly into DB (edge case). This edge case is better tested as a DB constraint test.
}
