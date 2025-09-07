using NSubstitute;
using Relevo.Core.Models;
using Relevo.Infrastructure.Auth;
using Xunit;

namespace Relevo.UnitTests.Auth;

public class HttpContextUserContextTests
{
    private readonly HttpContextUserContext _userContext;

    public HttpContextUserContextTests()
    {
        _userContext = new HttpContextUserContext();
        // Clear any existing user state
        _userContext.ClearUserAsync().Wait();
    }

    [Fact]
    public async Task CurrentUser_WithUserInContext_ReturnsUser()
    {
        // Arrange
        var user = new User { Id = "test-user", Email = "test@example.com" };
        await _userContext.SetUserAsync(user);

        // Act
        var result = _userContext.CurrentUser;

        // Assert
        Assert.Equal(user, result);
    }

    [Fact]
    public async Task CurrentUser_WithNoUserInContext_ReturnsNull()
    {
        // Arrange
        await _userContext.ClearUserAsync();

        // Act
        var result = _userContext.CurrentUser;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task IsAuthenticated_WithUser_ReturnsTrue()
    {
        // Arrange
        var user = new User { Id = "test-user" };
        await _userContext.SetUserAsync(user);

        // Act
        var result = _userContext.IsAuthenticated;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsAuthenticated_WithNoUser_ReturnsFalse()
    {
        // Arrange
        await _userContext.ClearUserAsync();

        // Act
        var result = _userContext.IsAuthenticated;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetUserAsync_WithValidUser_SetsUser()
    {
        // Arrange
        var user = new User { Id = "test-user" };

        // Act
        await _userContext.SetUserAsync(user);

        // Assert
        Assert.Equal(user, _userContext.CurrentUser);
    }

    [Fact]
    public async Task ClearUserAsync_WithUser_ClearsUser()
    {
        // Arrange
        var user = new User { Id = "test-user" };
        await _userContext.SetUserAsync(user);

        // Act
        await _userContext.ClearUserAsync();

        // Assert
        Assert.Null(_userContext.CurrentUser);
    }
}
