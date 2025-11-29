using System.Net.Http;
using System.Text;
using System.Text.Json;
using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Contributors;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class ContributorCreate(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsSuccessGivenValidNewContributor()
  {
    var request = new CreateContributorRequest
    {
      Name = "NewContributor",
      PhoneNumber = "1234567890"
    };

    var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
    var result = await _client.PostAndDeserializeAsync<CreateContributorResponse>("/Contributors", jsonContent);

    Assert.True(result.Id > 0);
    Assert.Equal(request.Name, result.Name);
  }
}

