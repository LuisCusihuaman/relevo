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
        var handoverId = "hvo-001"; // Seeded in DapperTestSeeder

        // Should return null initially as not seeded in DapperTestSeeder OR created on fly by repo logic
        // Wait, DapperSeeder only seeds HANDOVER_PATIENT_DATA, but repo GetSynthesisAsync creates if missing!
        // So it should return a default one.
        
        var synthesis = await repository.GetSynthesisAsync(handoverId);

        Assert.NotNull(synthesis);
        Assert.Equal(handoverId, synthesis.HandoverId);
    }
}
