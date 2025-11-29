using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Me;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class MeHandoverMessagesTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateAuthenticatedClient();

    [Fact]
    public async Task GetMessages_ReturnsMessagesForHandover()
    {
        // Act
        var response = await _client.GetAsync($"/me/handovers/{DapperTestSeeder.HandoverId}/messages");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetHandoverMessagesResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Messages);
        Assert.NotEmpty(result.Messages);
    }

    [Fact]
    public async Task CreateMessage_CreatesNewMessage()
    {
        // Arrange
        var request = new
        {
            MessageText = "Test message from functional test",
            MessageType = "note"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/me/handovers/{DapperTestSeeder.HandoverId}/messages",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateHandoverMessageResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Message);
        Assert.Equal("Test message from functional test", result.Message.MessageText);
    }
}

