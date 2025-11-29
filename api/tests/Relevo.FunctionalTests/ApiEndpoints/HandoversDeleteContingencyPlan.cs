using Ardalis.HttpClientTestExtensions;
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
    var response = await _client.DeleteAsync("/handovers/hvo-001/contingency-plans/plan-001");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    // Verify deletion by trying to get it (or ensure it's not in list)
    var result = await _client.GetAndDeserializeAsync<GetContingencyPlansResponse>("/handovers/hvo-001/contingency-plans");
    Assert.DoesNotContain(result.Plans, p => p.Id == "plan-001");
  }

  [Fact]
  public async Task ReturnsNotFoundForNonExistentPlan()
  {
    var response = await _client.DeleteAsync("/handovers/hvo-001/contingency-plans/non-existent");
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }
}

