using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class PatientRepositoryGetSummaryTests : BaseDapperRepoTestFixture
{
    public PatientRepositoryGetSummaryTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IPatientRepository GetPatientRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IPatientRepository>();
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task GetPatientSummary_ReturnsSummary()
    {
        var patientRepository = GetPatientRepository();
        var handoverRepository = GetHandoverRepository();
        var patientId = DapperTestSeeder.PatientId1;
        var userId = DapperTestSeeder.UserId;

        // V3: Get current handover (should exist from seeder)
        var handoverId = await handoverRepository.GetCurrentHandoverIdAsync(patientId);
        Assert.NotNull(handoverId);

        // Get summary from handover
        var summary = await patientRepository.GetPatientSummaryFromHandoverAsync(handoverId);

        Assert.NotNull(summary);
        Assert.Equal(patientId, summary.PatientId);
        Assert.NotNull(summary.SummaryText);
    }

    [Fact]
    public async Task GetPatientSummary_ReturnsNullForNoHandover()
    {
        var patientRepository = GetPatientRepository();
        var handoverId = "handover-nonexistent";

        var summary = await patientRepository.GetPatientSummaryFromHandoverAsync(handoverId);

        Assert.Null(summary);
    }
}
