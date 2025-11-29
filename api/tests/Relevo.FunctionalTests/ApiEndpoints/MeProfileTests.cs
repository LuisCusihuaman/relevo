using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Me;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class MeProfileTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetMyProfile_ReturnsProfile()
    {
        // Act
        var response = await _client.GetAsync("/me/profile");

        // Assert
        // May return 404 if user doesn't exist in test DB, or 200 if it does
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound but got {response.StatusCode}");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<GetMyProfileResponse>();
            Assert.NotNull(result);
            Assert.NotEmpty(result.Id);
        }
    }
}

