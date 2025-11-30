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
        var handoverId = DapperTestSeeder.HandoverId; // Use existing handover from seeder
        var summaryText = "Integration Test Summary";
        var userId = DapperTestSeeder.UserId;

        var summary = await repository.CreatePatientSummaryAsync(handoverId, summaryText, userId);

        Assert.NotNull(summary);
        Assert.Equal(handoverId, summary.Id); // Id is now handoverId
        Assert.Equal(summaryText, summary.SummaryText);
        Assert.False(string.IsNullOrEmpty(summary.PatientId));
    }
}
