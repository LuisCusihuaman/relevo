using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Me;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class MeHandoverActivityTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetActivity_ReturnsActivityLogForHandover()
    {
        // Act
        var response = await _client.GetAsync($"/me/handovers/{DapperTestSeeder.HandoverId}/activity");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetHandoverActivityResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Activities);
    }
}

