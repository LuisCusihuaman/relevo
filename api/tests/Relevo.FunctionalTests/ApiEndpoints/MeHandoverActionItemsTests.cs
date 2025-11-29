using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Me;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class MeHandoverActionItemsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetActionItems_ReturnsActionItemsForHandover()
    {
        // Act
        var response = await _client.GetAsync($"/me/handovers/{DapperTestSeeder.HandoverId}/action-items");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetHandoverActionItemsResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.ActionItems);
    }

    [Fact]
    public async Task CreateActionItem_CreatesNewActionItem()
    {
        // Arrange
        var request = new
        {
            Description = "New test action item",
            Priority = "high"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/me/handovers/{DapperTestSeeder.HandoverId}/action-items", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateHandoverActionItemResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotEmpty(result.ActionItemId);
    }

    [Fact]
    public async Task UpdateActionItem_UpdatesExistingActionItem()
    {
        // Arrange
        var request = new { IsCompleted = true };

        // Act
        var response = await _client.PutAsJsonAsync($"/me/handovers/{DapperTestSeeder.HandoverId}/action-items/{DapperTestSeeder.ActionItemId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<UpdateHandoverActionItemResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task DeleteActionItem_DeletesExistingActionItem()
    {
        // First create an item to delete
        var createRequest = new
        {
            Description = "Item to delete",
            Priority = "low"
        };
        var createResponse = await _client.PostAsJsonAsync($"/me/handovers/{DapperTestSeeder.HandoverId}/action-items", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateHandoverActionItemResponse>();
        Assert.NotNull(createResult);

        // Act
        var response = await _client.DeleteAsync($"/me/handovers/{DapperTestSeeder.HandoverId}/action-items/{createResult.ActionItemId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DeleteHandoverActionItemResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task UpdateActionItem_ReturnsNotFoundForNonExistentItem()
    {
        // Arrange
        var request = new { IsCompleted = true };

        // Act
        var response = await _client.PutAsJsonAsync($"/me/handovers/{DapperTestSeeder.HandoverId}/action-items/non-existent-item", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteActionItem_ReturnsNotFoundForNonExistentItem()
    {
        // Act
        var response = await _client.DeleteAsync($"/me/handovers/{DapperTestSeeder.HandoverId}/action-items/non-existent-item");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
