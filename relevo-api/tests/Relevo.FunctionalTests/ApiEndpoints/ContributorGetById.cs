using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Contributors;
using System.Linq;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class ContributorGetById(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsSeedContributorArdalis()
  {
    // First get the list to find the actual ID
    var listResult = await _client.GetAndDeserializeAsync<ContributorListResponse>("/Contributors");
    var ardalisContributor = listResult.Contributors.First(c => c.Name == "Ardalis");

    var result = await _client.GetAndDeserializeAsync<ContributorRecord>(GetContributorByIdRequest.BuildRoute((int)ardalisContributor.Id));

    Assert.Equal(ardalisContributor.Id, result.Id);
    Assert.Equal("Ardalis", result.Name);
    Assert.Equal("+1-555-0101", result.PhoneNumber);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenId1000()
  {
    string route = GetContributorByIdRequest.BuildRoute(1000);
    _ = await _client.GetAndEnsureNotFoundAsync(route);
  }
}
