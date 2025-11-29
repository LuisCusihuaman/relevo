using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversGetById(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsHandoverGivenValidId()
  {
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>("/handovers/hvo-001");

    Assert.NotNull(result);
    Assert.Equal("hvo-001", result.Id);
    Assert.Equal("Draft", result.Status);
    Assert.NotEmpty(result.actionItems);
    Assert.Contains(result.actionItems, i => i.id == "item-001");
  }

  [Fact]
  public async Task ReturnsNotFoundGivenInvalidId()
  {
    await _client.GetAndEnsureNotFoundAsync("/handovers/invalid-id");
  }
}

