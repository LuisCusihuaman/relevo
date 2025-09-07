using Microsoft.Extensions.Logging;
using NSubstitute;
using Relevo.Core.Models;
using Relevo.Infrastructure.Auth;
using Xunit;

namespace Relevo.UnitTests.Auth;

public class ClerkAuthorizationServiceTests
{
    private readonly ILogger<ClerkAuthorizationService> _logger;
    private readonly ClerkAuthorizationService _service;

    public ClerkAuthorizationServiceTests()
    {
        _logger = Substitute.For<ILogger<ClerkAuthorizationService>>();
        _service = new ClerkAuthorizationService(_logger);
    }

    [Fact]
    public async Task AuthorizeAsync_WithValidUserAndPermission_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Id = "test-user",
            Permissions = new[] { "patients.read", "patients.assign" }
        };

        // Act
        var result = await _service.AuthorizeAsync(user, "patients.read");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AuthorizeAsync_WithUserWithoutPermission_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = "test-user",
            Permissions = new[] { "patients.read" }
        };

        // Act
        var result = await _service.AuthorizeAsync(user, "admin.full_access");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AuthorizeAsync_WithNullUser_ReturnsFalse()
    {
        // Act
        var result = await _service.AuthorizeAsync(null!, "patients.read");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AuthorizeAsync_WithInactiveUser_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = "test-user",
            IsActive = false,
            Permissions = new[] { "patients.read" }
        };

        // Act
        var result = await _service.AuthorizeAsync(user, "patients.read");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasRoleAsync_WithValidUserAndRole_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Id = "test-user",
            Roles = new[] { "clinician", "admin" }
        };

        // Act
        var result = await _service.HasRoleAsync(user, "clinician");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasRoleAsync_WithUserWithoutRole_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = "test-user",
            Roles = new[] { "clinician" }
        };

        // Act
        var result = await _service.HasRoleAsync(user, "admin");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithValidUser_ReturnsPermissions()
    {
        // Arrange
        var user = new User
        {
            Id = "test-user",
            Permissions = new[] { "patients.read", "patients.assign" }
        };

        // Act
        var permissions = await _service.GetUserPermissionsAsync(user);

        // Assert
        Assert.Equal(user.Permissions, permissions);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithNullUser_ReturnsEmpty()
    {
        // Act
        var permissions = await _service.GetUserPermissionsAsync(null!);

        // Assert
        Assert.Empty(permissions);
    }
}
