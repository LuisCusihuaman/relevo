using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Contributors;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class ContributorList(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task ReturnsContributors()
  {
    var result = await _client.GetAndDeserializeAsync<ContributorListResponse>("/Contributors");

    Assert.NotEmpty(result.Contributors);
    Assert.Contains(result.Contributors, i => i.Name == TestSeeds.Contributor1);
    Assert.Contains(result.Contributors, i => i.Name == TestSeeds.Contributor2);
  }
}
