using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryGetPatientHandoversTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryGetPatientHandoversTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task GetPatientHandovers_ReturnsHandovers()
    {
        var repository = GetHandoverRepository();
        var patientId = "pat-001";

        var (handovers, total) = await repository.GetPatientHandoversAsync(patientId, 1, 25);

        Assert.True(total > 0);
        Assert.Contains(handovers, h => h.Id == "hvo-001");
        Assert.Equal(patientId, handovers.First().PatientId);
    }
}

