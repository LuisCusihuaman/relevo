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

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task UpdatePatientSummary_UpdatesExistingSummary()
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

        // Update summary
        var updated = await patientRepository.UpdatePatientSummaryAsync(handoverId, "Updated Text", userId);
        Assert.True(updated);

        // Verify update
        var newSummary = await patientRepository.GetPatientSummaryFromHandoverAsync(handoverId);
        Assert.NotNull(newSummary);
        Assert.Equal("Updated Text", newSummary.SummaryText);
    }
}
