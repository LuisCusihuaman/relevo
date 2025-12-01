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

    [Fact]
    public async Task GetHandoverById_ReturnsV3Fields()
    {
        // V3: Verify GetHandoverByIdAsync returns all V3 fields
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;

        var detail = await repository.GetHandoverByIdAsync(handoverId);

        Assert.NotNull(detail);
        var handover = detail.Handover;
        
        // V3 fields that should be present
        Assert.NotNull(handover.ShiftWindowId); // V3: SHIFT_WINDOW_ID instead of direct shift references
        Assert.NotNull(handover.Status); // V3: CURRENT_STATE virtual column
        Assert.NotNull(handover.PatientId);
        Assert.NotNull(handover.CreatedAt);
        
        // V3: SENDER_USER_ID should be set if handover is Ready or beyond
        if (handover.Status != "Draft")
        {
            Assert.NotNull(handover.SenderUserId);
        }
    }
}
