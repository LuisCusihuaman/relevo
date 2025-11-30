using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryGetByIdTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryGetByIdTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task GetHandoverById_ReturnsHandoverWithActionItems()
    {
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;
        var actionItemId = DapperTestSeeder.ActionItemId;

        var detail = await repository.GetHandoverByIdAsync(handoverId);

        Assert.NotNull(detail);
        Assert.Equal(handoverId, detail.Handover.Id);
        Assert.NotEmpty(detail.ActionItems);
        Assert.Contains(detail.ActionItems, i => i.Id == actionItemId);
    }

    [Fact]
    public async Task GetHandoverById_ReturnsNullForInvalidId()
    {
        var repository = GetHandoverRepository();
        var handoverId = "non-existent-id";

        var detail = await repository.GetHandoverByIdAsync(handoverId);

        Assert.Null(detail);
    }
}
