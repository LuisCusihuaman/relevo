using Relevo.Core.Interfaces;
using Relevo.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Relevo.IntegrationTests.Data;

public class HandoverRepositoryGetPatientHandoversTests : BaseDapperRepoTestFixture
{
    public HandoverRepositoryGetPatientHandoversTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private IHandoverRepository GetHandoverRepository()
    {
        return _scope.ServiceProvider.GetRequiredService<IHandoverRepository>();
    }

    [Fact]
    public async Task GetPatientHandovers_ReturnsHandovers()
    {
        var repository = GetHandoverRepository();
        var patientId = DapperTestSeeder.PatientId1;
        var handoverId = DapperTestSeeder.HandoverId;

        var (handovers, total) = await repository.GetPatientHandoversAsync(patientId, 1, 25);

        Assert.True(total > 0);
        Assert.Contains(handovers, h => h.Id == handoverId);
        Assert.Equal(patientId, handovers.First().PatientId);
    }

    [Fact]
    public async Task GetPatientHandovers_ReturnsV3Fields()
    {
        // V3: Verify GetPatientHandoversAsync returns all V3 fields
        var repository = GetHandoverRepository();
        var patientId = DapperTestSeeder.PatientId1;

        var (handovers, total) = await repository.GetPatientHandoversAsync(patientId, 1, 25);

        Assert.True(total > 0);
        Assert.NotEmpty(handovers);
        
        var handover = handovers.First();
        
        // V3 fields that should be present
        Assert.NotNull(handover.ShiftWindowId); // V3: SHIFT_WINDOW_ID
        Assert.NotNull(handover.Status); // V3: CURRENT_STATE
        Assert.Equal(patientId, handover.PatientId);
        Assert.NotNull(handover.CreatedAt);
    }

    [Fact]
    public async Task GetPatientHandovers_OrdersByCreatedAtDesc()
    {
        // V3: Verify handovers are ordered by CREATED_AT DESC
        var repository = GetHandoverRepository();
        var patientId = DapperTestSeeder.PatientId1;

        var (handovers, total) = await repository.GetPatientHandoversAsync(patientId, 1, 25);

        if (handovers.Count >= 2)
        {
            var first = handovers[0];
            var second = handovers[1];
            // CreatedAt is string, so compare as strings (ISO format dates are comparable)
            if (!string.IsNullOrEmpty(first.CreatedAt) && !string.IsNullOrEmpty(second.CreatedAt))
            {
                Assert.True(string.CompareOrdinal(first.CreatedAt, second.CreatedAt) >= 0, 
                    "Handovers should be ordered by CREATED_AT DESC");
            }
        }
    }

    [Fact]
    public async Task GetPatientHandovers_HandlesPagination()
    {
        // V3: Verify pagination works correctly
        var repository = GetHandoverRepository();
        var patientId = DapperTestSeeder.PatientId1;

        var (page1, total1) = await repository.GetPatientHandoversAsync(patientId, 1, 1);
        var (page2, total2) = await repository.GetPatientHandoversAsync(patientId, 2, 1);

        Assert.Equal(total1, total2); // Total should be same
        if (total1 > 1)
        {
            Assert.Single(page1);
            Assert.Single(page2);
            Assert.NotEqual(page1.First().Id, page2.First().Id);
        }
    }
}
