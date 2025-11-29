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
        var handoverId = "hvo-001";

        var data = await repository.GetPatientHandoverDataAsync(handoverId);

        Assert.NotNull(data);
        Assert.Equal("pat-001", data.Id);
        Assert.Equal("María García", data.Name);
        Assert.Equal("UCI", data.Unit);
        Assert.Equal("Stable", data.IllnessSeverity);
        Assert.Equal("Patient stable overnight", data.SummaryText);
        
        Assert.NotNull(data.AssignedPhysician);
        Assert.Equal("Dr. One", data.AssignedPhysician.Name);
        Assert.Equal("07:00", data.AssignedPhysician.ShiftStart);
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

