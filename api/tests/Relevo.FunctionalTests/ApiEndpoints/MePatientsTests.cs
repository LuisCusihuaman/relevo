using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Me;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class MePatientsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetMyPatients_ReturnsOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/me/patients");

        // Assert - endpoint should return OK even if no patients assigned to the hardcoded user
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetMyPatientsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.NotNull(result.Pagination);
    }

    [Fact]
    public async Task GetMyPatients_SupportsPagination()
    {
        // Act
        var response = await _client.GetAsync("/me/patients?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetMyPatientsResponse>();
        Assert.NotNull(result);
        Assert.Equal(10, result.Pagination.PageSize);
    }
}

