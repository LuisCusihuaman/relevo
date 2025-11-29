using System.Net.Http.Json;
using Relevo.Web.Handovers;
using Xunit;
using Ardalis.HttpClientTestExtensions;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class GetHandoverByIdDetailedTests(CustomWebApplicationFactory<Program> factory) 
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetHandoverById_ReturnsCorrectHandoverStructure()
    {
        var handoverId = DapperTestSeeder.HandoverId;
        var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

        Assert.NotNull(result);

        // Verify illness severity structure
        Assert.NotNull(result.illnessSeverity);
        Assert.Contains(result.illnessSeverity.severity, new[] { "Stable", "Watcher", "Unstable" });

        // Verify patient summary structure
        Assert.NotNull(result.patientSummary);
        // content might be null/empty depending on seed, but object shouldn't be null if API guarantees it
        
        // Verify action items structure
        Assert.NotNull(result.actionItems);
        if (result.actionItems.Count > 0)
        {
            foreach (var actionItem in result.actionItems)
            {
                Assert.NotNull(actionItem.id);
                Assert.NotNull(actionItem.description);
            }
        }
    }
}

