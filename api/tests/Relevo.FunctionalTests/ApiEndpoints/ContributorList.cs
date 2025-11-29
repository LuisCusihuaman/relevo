using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Contributors;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class ContributorList(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsTwoContributors()
  {
    var result = await _client.GetAndDeserializeAsync<ContributorListResponse>("/Contributors");

    Assert.Equal(2, result.Contributors.Count);
    Assert.Contains(result.Contributors, i => i.Name == TestSeeds.Contributor1); // Seeded in DapperTestSeeder
    Assert.Contains(result.Contributors, i => i.Name == TestSeeds.Contributor2); // Seeded in DapperTestSeeder
  }
}
