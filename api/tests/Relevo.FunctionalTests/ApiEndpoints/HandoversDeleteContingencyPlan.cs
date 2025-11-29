using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Handovers;
using Xunit;
using System.Net;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversDeleteContingencyPlan(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task DeletesContingencyPlan()
  {
    var handoverId = DapperTestSeeder.HandoverId;
    var planId = DapperTestSeeder.ContingencyPlanId;
    
    var response = await _client.DeleteAsync($"/handovers/{handoverId}/contingency-plans/{planId}");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await _client.GetAndDeserializeAsync<GetContingencyPlansResponse>($"/handovers/{handoverId}/contingency-plans");
    Assert.DoesNotContain(result.Plans, p => p.Id == planId);
  }

  [Fact]
  public async Task ReturnsNotFoundForNonExistentPlan()
  {
    var handoverId = DapperTestSeeder.HandoverId;
    var response = await _client.DeleteAsync($"/handovers/{handoverId}/contingency-plans/non-existent");
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }
}
