using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Xunit;
using System.Net;
using System.Net.Http.Json;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversCreateContingencyPlan(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task CreatesContingencyPlan()
  {
    var request = new CreateContingencyPlanRequest
    {
        HandoverId = "hvo-001",
        ConditionText = "Functional Condition",
        ActionText = "Functional Action",
        Priority = "Low"
    };

    var response = await _client.PostAsJsonAsync("/handovers/hvo-001/contingency-plans", request);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<CreateContingencyPlanResponse>();

    Assert.NotNull(result);
    Assert.NotNull(result.Plan);
    Assert.Equal("hvo-001", result.Plan.HandoverId);
    Assert.Equal("Functional Condition", result.Plan.ConditionText);
  }
}

