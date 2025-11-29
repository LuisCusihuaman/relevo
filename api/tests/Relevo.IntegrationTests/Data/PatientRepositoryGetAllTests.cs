using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class PatientRepositoryGetAllTests : BaseDapperRepoTestFixture
{
    public PatientRepositoryGetAllTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IPatientRepository GetPatientRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IPatientRepository>();
    }

    [Fact]
    public async Task GetAllPatients_ReturnsPatients()
    {
        var repository = GetPatientRepository();

        var (patients, total) = await repository.GetAllPatientsAsync(1, 25);

        Assert.True(total > 0);
        Assert.Contains(patients, p => p.Id == "pat-001");
        Assert.Contains(patients, p => p.Id == "pat-002");
    }
}

