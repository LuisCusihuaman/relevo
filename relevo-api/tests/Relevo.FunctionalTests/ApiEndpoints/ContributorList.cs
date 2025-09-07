using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Contributors;
using System.Linq;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class ContributorList(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsTestContributors()
  {
    var result = await _client.GetAndDeserializeAsync<ContributorListResponse>("/Contributors");

    // Should have at least the test contributors
    Assert.True(result.Contributors.Count >= 2);
    Assert.Contains(result.Contributors, i => i.Name == "Ardalis");
    Assert.Contains(result.Contributors, i => i.Name == "Snowfrog");

    // Verify the test contributors have the expected phone numbers
    var ardalis = result.Contributors.First(c => c.Name == "Ardalis");
    var snowfrog = result.Contributors.First(c => c.Name == "Snowfrog");

    Assert.Equal("+1-555-0101", ardalis.PhoneNumber);
    Assert.Equal("+1-555-0102", snowfrog.PhoneNumber);
  }
}
