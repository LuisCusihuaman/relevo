using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class PatientRepositoryUpdateSummaryTests : BaseDapperRepoTestFixture
{
    public PatientRepositoryUpdateSummaryTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IPatientRepository GetPatientRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IPatientRepository>();
    }

    [Fact]
    public async Task UpdatePatientSummary_UpdatesExistingSummary()
    {
        var repository = GetPatientRepository();
        var patientId = "pat-001";
        var userId = "dr-1";
        
        // Assuming seed data exists for pat-001 (sum-001)
        var summary = await repository.GetPatientSummaryAsync(patientId);
        Assert.NotNull(summary);

        var updated = await repository.UpdatePatientSummaryAsync(summary.Id, "Updated Text", userId);
        Assert.True(updated);

        var newSummary = await repository.GetPatientSummaryAsync(patientId);
        Assert.Equal("Updated Text", newSummary?.SummaryText);
    }
}

