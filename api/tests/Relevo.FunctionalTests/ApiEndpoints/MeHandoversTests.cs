using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Me;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class MeHandoversTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetMyHandovers_ReturnsHandoversForUser()
    {
        // Act
        var response = await _client.GetAsync("/me/handovers");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetMyHandoversResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.NotNull(result.Pagination);
    }

    [Fact]
    public async Task GetMyHandovers_SupportsPagination()
    {
        // Act
        var response = await _client.GetAsync("/me/handovers?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetMyHandoversResponse>();
        Assert.NotNull(result);
        Assert.Equal(10, result.Pagination.PageSize);
    }
}

