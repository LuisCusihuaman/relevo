using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.Handovers;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversGetById(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task ReturnsHandoverGivenValidId()
  {
    var handoverId = DapperTestSeeder.HandoverId;
    var actionItemId = DapperTestSeeder.ActionItemId;
    
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.Equal(handoverId, result.Id);
    Assert.NotEmpty(result.actionItems);
    Assert.Contains(result.actionItems, i => i.id == actionItemId);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenInvalidId()
  {
    await _client.GetAndEnsureNotFoundAsync("/handovers/invalid-id");
  }
}
