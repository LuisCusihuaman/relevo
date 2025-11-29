using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class PatientRepositoryCreateSummaryTests : BaseDapperRepoTestFixture
{
    public PatientRepositoryCreateSummaryTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IPatientRepository GetPatientRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IPatientRepository>();
    }

    [Fact]
    public async Task CreatePatientSummary_ReturnsCreatedSummary()
    {
        var repository = GetPatientRepository();
        var patientId = "pat-001";
        var physicianId = "dr-1";
        var summaryText = "Integration Test Summary";

        var summary = await repository.CreatePatientSummaryAsync(patientId, physicianId, summaryText, physicianId);

        Assert.NotNull(summary);
        Assert.Equal(patientId, summary.PatientId);
        Assert.Equal(summaryText, summary.SummaryText);
        Assert.False(string.IsNullOrEmpty(summary.Id));
    }
}

