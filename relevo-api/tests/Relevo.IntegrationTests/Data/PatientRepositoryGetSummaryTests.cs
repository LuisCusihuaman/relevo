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

    [Fact]
    public async Task GetPatientSummary_ReturnsSummary()
    {
        var repository = GetPatientRepository();
        var patientId = DapperTestSeeder.PatientId1;

        var summary = await repository.GetPatientSummaryAsync(patientId);

        Assert.NotNull(summary);
        Assert.Equal(patientId, summary.PatientId);
        Assert.NotNull(summary.SummaryText);
    }

    [Fact]
    public async Task GetPatientSummary_ReturnsNullForNoSummary()
    {
        var repository = GetPatientRepository();
        var patientId = "pat-no-summary";

        var summary = await repository.GetPatientSummaryAsync(patientId);

        Assert.Null(summary);
    }
}
