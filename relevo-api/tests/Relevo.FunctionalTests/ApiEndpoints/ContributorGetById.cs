using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Contributors;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class ContributorGetById(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task ReturnsSeedContributorGivenId1()
  {
    var result = await _client.GetAndDeserializeAsync<ContributorRecord>(GetContributorByIdRequest.BuildRoute(1));

    Assert.Equal(1, result.Id);
    Assert.Equal(TestSeeds.Contributor1, result.Name); // Seeded in DapperTestSeeder
  }

  [Fact]
  public async Task ReturnsNotFoundGivenNonExistentId()
  {
    string route = GetContributorByIdRequest.BuildRoute(999999);
    _ = await _client.GetAndEnsureNotFoundAsync(route);
  }
}
