using Ardalis.HttpClientTestExtensions;
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
    var result = await _client.GetAndDeserializeAsync<GetContingencyPlansResponse>("/handovers/hvo-001/contingency-plans");

    Assert.NotNull(result);
    Assert.NotEmpty(result.Plans);
    Assert.Contains(result.Plans, p => p.Id == "plan-001");
    Assert.Equal("If BP drops below 90/60", result.Plans.First(p => p.Id == "plan-001").ConditionText);
  }
}

