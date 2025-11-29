using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Handovers;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversGetContingencyPlans(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsPlansForHandover()
  {
    var handoverId = DapperTestSeeder.HandoverId;
    var planId = DapperTestSeeder.ContingencyPlanId;
    
    var result = await _client.GetAndDeserializeAsync<GetContingencyPlansResponse>($"/handovers/{handoverId}/contingency-plans");

    Assert.NotNull(result);
    Assert.NotEmpty(result.Plans);
    Assert.Contains(result.Plans, p => p.Id == planId);
    Assert.Equal("If BP drops below 90/60", result.Plans.First(p => p.Id == planId).ConditionText);
  }
}
