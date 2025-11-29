using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class PatientRepositoryGetByIdTests : BaseDapperRepoTestFixture
{
    public PatientRepositoryGetByIdTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IPatientRepository GetPatientRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IPatientRepository>();
    }

    [Fact]
    public async Task GetPatientById_ReturnsPatient()
    {
        var repository = GetPatientRepository();
        var patientId = "pat-001";

        var patient = await repository.GetPatientByIdAsync(patientId);

        Assert.NotNull(patient);
        Assert.Equal(patientId, patient.Id);
        Assert.Equal("María García", patient.Name);
        Assert.Equal("UCI", patient.CurrentUnit);
    }

    [Fact]
    public async Task GetPatientById_ReturnsNull_WhenNotFound()
    {
        var repository = GetPatientRepository();
        var patientId = "non-existent-id";

        var patient = await repository.GetPatientByIdAsync(patientId);

        Assert.Null(patient);
    }
}

