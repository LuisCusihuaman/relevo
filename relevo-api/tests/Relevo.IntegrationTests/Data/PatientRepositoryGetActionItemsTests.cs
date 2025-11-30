using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class PatientRepositoryGetActionItemsTests : BaseDapperRepoTestFixture
{
    public PatientRepositoryGetActionItemsTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IPatientRepository GetPatientRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IPatientRepository>();
    }

    [Fact]
    public async Task GetPatientActionItems_ReturnsItems()
    {
        var repository = GetPatientRepository();
        var patientId = DapperTestSeeder.PatientId1;
        var handoverId = DapperTestSeeder.HandoverId;

        var items = await repository.GetPatientActionItemsAsync(patientId);

        Assert.NotEmpty(items);
        Assert.Contains(items, i => i.Description == "Check blood pressure");
        Assert.Contains(items, i => i.HandoverId == handoverId);
    }
}
