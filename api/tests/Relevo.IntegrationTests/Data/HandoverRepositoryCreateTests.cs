using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryCreateTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryCreateTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task CreateHandover_CreatesAndReturnsHandover()
    {
        var repository = GetHandoverRepository();
        var request = new CreateHandoverRequest(
            "pat-001", "dr-1", "dr-1", "shift-day", "shift-night", "dr-1", "Test Notes"
        );

        var handover = await repository.CreateHandoverAsync(request);

        Assert.NotNull(handover);
        Assert.NotNull(handover.Id);
        Assert.Equal("pat-001", handover.PatientId);
        Assert.Equal("Draft", handover.Status);
    }
}

