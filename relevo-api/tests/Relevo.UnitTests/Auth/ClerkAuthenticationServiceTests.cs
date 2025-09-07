using Microsoft.Extensions.Logging;
using NSubstitute;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.Infrastructure.Auth;
using Xunit;

namespace Relevo.UnitTests.Auth;

public class ClerkAuthenticationServiceTests
{
    private readonly ILogger<ClerkAuthenticationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ClerkAuthenticationService _service;

    public ClerkAuthenticationServiceTests()
    {
        _logger = Substitute.For<ILogger<ClerkAuthenticationService>>();
        _httpClient = new HttpClient();
        _service = new ClerkAuthenticationService(null, _logger, _httpClient);
    }

    [Fact]
    public async Task AuthenticateAsync_WithEmptyToken_ReturnsFailure()
    {
        // Arrange
        var token = string.Empty;

        // Act
        var result = await _service.AuthenticateAsync(token);

        // Assert
        Assert.False(result.IsAuthenticated);
        Assert.Null(result.User);
        Assert.Equal("Token is required", result.Error);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var token = "invalid-token";

        // Act
        var result = await _service.AuthenticateAsync(token);

        // Assert
        Assert.False(result.IsAuthenticated);
        Assert.Null(result.User);
        Assert.Contains("Authentication failed", result.Error);
    }

}
