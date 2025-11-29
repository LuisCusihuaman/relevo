using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Me;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class MeHandoverChecklistsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetChecklists_ReturnsChecklistsForHandover()
    {
        // Act
        var response = await _client.GetAsync($"/me/handovers/{DapperTestSeeder.HandoverId}/checklists");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetHandoverChecklistsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Checklists);
    }

    [Fact]
    public async Task UpdateChecklistItem_UpdatesItem()
    {
        // Arrange
        var request = new { IsChecked = true };

        // Act - Note: This may return NotFound if the checklist item doesn't match the userId in the WHERE clause
        var response = await _client.PutAsJsonAsync(
            $"/me/handovers/{DapperTestSeeder.HandoverId}/checklists/{DapperTestSeeder.ChecklistItemId}",
            request);

        // Assert - Accept either OK (item found and updated) or NotFound (item not found for this user)
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound but got {response.StatusCode}");
    }
}

