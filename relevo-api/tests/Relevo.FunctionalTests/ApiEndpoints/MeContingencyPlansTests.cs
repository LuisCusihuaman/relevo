using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Me;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class MeContingencyPlansTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateAuthenticatedClient();

    [Fact]
    public async Task GetContingencyPlans_ReturnsPlansForHandover()
    {
        // Act
        var response = await _client.GetAsync($"/me/handovers/{DapperTestSeeder.HandoverId}/contingency-plans");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetMeContingencyPlansResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.ContingencyPlans);
        Assert.NotEmpty(result.ContingencyPlans);
    }

    [Fact]
    public async Task CreateContingencyPlan_CreatesNewPlan()
    {
        // Arrange
        var request = new
        {
            ConditionText = "If temperature rises above 38C",
            ActionText = "Administer antipyretic",
            Priority = "medium" // V3: Must be lowercase per CHK_CONT_PRIORITY constraint
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/me/handovers/{DapperTestSeeder.HandoverId}/contingency-plans",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateMeContingencyPlanResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.ContingencyPlan);
        Assert.Equal("If temperature rises above 38C", result.ContingencyPlan.ConditionText);
    }
}

