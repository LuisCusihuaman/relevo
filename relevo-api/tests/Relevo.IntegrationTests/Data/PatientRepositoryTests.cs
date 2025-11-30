using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class PatientRepositoryTests : BaseDapperRepoTestFixture
{
    public PatientRepositoryTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IPatientRepository GetPatientRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IPatientRepository>();
    }

    [Fact]
    public async Task GetPatientsByUnit_ReturnsPatients()
    {
        var repository = GetPatientRepository();
        var unitId = DapperTestSeeder.UnitId;
        var patientId = DapperTestSeeder.PatientId1;

        var (patients, total) = await repository.GetPatientsByUnitAsync(unitId, 1, 25);

        Assert.True(total > 0);
        Assert.Contains(patients, p => p.Id == patientId);
    }
}
