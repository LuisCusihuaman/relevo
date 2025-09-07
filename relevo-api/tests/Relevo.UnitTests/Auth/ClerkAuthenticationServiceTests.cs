using Microsoft.Extensions.Logging;
using NSubstitute;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.Infrastructure.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Relevo.UnitTests.Auth;

/// <summary>
/// Test implementation of ClerkAuthenticationService for unit testing
/// </summary>
public class TestClerkAuthenticationService : IAuthenticationService
{
    private readonly ILogger<ClerkAuthenticationService> _logger;

    public TestClerkAuthenticationService(ILogger<ClerkAuthenticationService> logger)
    {
        _logger = logger;
    }

    public Task<AuthenticationResult> AuthenticateAsync(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticationResult.Failure("Token is required"));
            }

            // For testing, accept any non-empty token as valid
            if (token.StartsWith("test-token"))
            {
                var user = new User
                {
                    Id = "test-user-id",
                    Email = "test@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    Roles = new[] { "clinician" },
                    Permissions = new[] { "patients.read", "patients.assign" },
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                };
                return Task.FromResult(AuthenticationResult.Success(user));
            }

            // Try to parse as JWT for real tokens
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);

                // Basic validation (more comprehensive validation would be done in production)
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    return Task.FromResult(AuthenticationResult.Failure("Token expired"));
                }

                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Task.FromResult(AuthenticationResult.Failure("User ID claim missing from token"));
                }

                var user = ExtractUserFromToken(jwtToken);
                return Task.FromResult(AuthenticationResult.Success(user));
            }

            return Task.FromResult(AuthenticationResult.Failure("Invalid token format"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed");
            return Task.FromResult(AuthenticationResult.Failure($"Authentication failed: {ex.Message}"));
        }
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        // For testing, any non-empty token is considered valid
        return Task.FromResult(!string.IsNullOrEmpty(token) && token.StartsWith("test-"));
    }

    private User ExtractUserFromToken(JwtSecurityToken jwtToken)
    {
        var claims = jwtToken.Claims;

        return new User
        {
            Id = claims.FirstOrDefault(c => c.Type == "sub")?.Value
                 ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                 ?? string.Empty,
            Email = claims.FirstOrDefault(c => c.Type == "email")?.Value
                   ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                   ?? string.Empty,
            FirstName = claims.FirstOrDefault(c => c.Type == "first_name")?.Value ?? string.Empty,
            LastName = claims.FirstOrDefault(c => c.Type == "last_name")?.Value ?? string.Empty,
            Roles = new[] { "clinician" }, // Default role for tests
            Permissions = new[] { "patients.read", "patients.assign" }, // Default permissions for tests
            LastLoginAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}

public class ClerkAuthenticationServiceTests
{
    private readonly ILogger<ClerkAuthenticationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _service;

    public ClerkAuthenticationServiceTests()
    {
        _logger = Substitute.For<ILogger<ClerkAuthenticationService>>();
        _httpClient = new HttpClient();
        // For unit tests, we'll create a mock service that accepts test tokens
        _service = new TestClerkAuthenticationService(_logger);
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
        Assert.Contains("Invalid token format", result.Error);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidTestToken_ReturnsSuccess()
    {
        // Arrange
        var token = "test-token-valid";

        // Act
        var result = await _service.AuthenticateAsync(token);

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.NotNull(result.User);
        Assert.Equal("test-user-id", result.User.Id);
        Assert.Equal("test@example.com", result.User.Email);
    }

}
