using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryGetPatientDataTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryGetPatientDataTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task GetPatientHandoverData_ReturnsCorrectData()
    {
        var repository = GetHandoverRepository();
        var handoverId = DapperTestSeeder.HandoverId;
        var patientId = DapperTestSeeder.PatientId1;

        var data = await repository.GetPatientHandoverDataAsync(handoverId);

        Assert.NotNull(data);
        Assert.Equal(patientId, data.Id);
        Assert.NotNull(data.Name);
        Assert.NotNull(data.Unit);
        Assert.NotNull(data.IllnessSeverity);
        
        Assert.NotNull(data.AssignedPhysician);
    }

    [Fact]
    public async Task GetPatientHandoverData_ReturnsNullForInvalidId()
    {
        var repository = GetHandoverRepository();
        var handoverId = "non-existent-id";

        var data = await repository.GetPatientHandoverDataAsync(handoverId);

        Assert.Null(data);
    }
}
