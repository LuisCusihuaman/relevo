using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Testing;
using Relevo.Web;
using Xunit;

namespace Relevo.FunctionalTests.Auth;

public class ClerkAuthenticationMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ClerkAuthenticationMiddlewareTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MeEndpoints_WithoutAuthentication_Returns200_WithDemoUser()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/me/patients");

        // Assert
        // Me endpoints have AllowAnonymous but provide a demo user internally for testing
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MeEndpoints_WithValidToken_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = "test-token-valid"; // Use simple test token
        client.DefaultRequestHeaders.Add("x-clerk-user-token", token);

        // Act
        var response = await client.GetAsync("/me/patients");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MeEndpoints_WithBearerToken_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = "test-token-valid"; // Use simple test token
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await client.GetAsync("/me/patients");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SetupEndpoints_AllowAnonymous_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/setup/units");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MeEndpoints_WithValidToken_IncludesUserHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = "test-token-valid"; // Use simple test token
        client.DefaultRequestHeaders.Add("x-clerk-user-token", token);

        // Act
        var response = await client.GetAsync("/me/patients");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-User-Id"));
        Assert.True(response.Headers.Contains("X-User-Email"));
    }

    [Fact]
    public async Task PostAssignments_WithValidToken_Returns204()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = "test-token-valid"; // Use simple test token
        client.DefaultRequestHeaders.Add("x-clerk-user-token", token);

        var requestContent = new StringContent(
            @"{""shiftId"": ""shift-day"", ""patientIds"": [""pat-123""]}",
            System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await client.PostAsync("/me/assignments", requestContent);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private string GenerateValidJwtToken()
    {
        var claims = new[]
        {
            new Claim("sub", "test-user-id"),
            new Claim("email", "test@example.com"),
            new Claim("first_name", "John"),
            new Claim("last_name", "Doe"),
            new Claim("roles", "[\"clinician\"]")
        };

        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes("test-secret-key-for-testing-purposes-only"));
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
