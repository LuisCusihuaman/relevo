using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Auth;

public class ClerkAuthenticationService : IAuthenticationService
{
    private readonly IConfiguration? _configuration;
    private readonly ILogger<ClerkAuthenticationService> _logger;

    public ClerkAuthenticationService(
        IConfiguration? configuration,
        ILogger<ClerkAuthenticationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                return AuthenticationResult.Failure("Token is required");
            }

            // Remove "Bearer " prefix if present
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token[7..];
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Extract user information from JWT payload
            var user = ExtractUserFromToken(jwtToken);

            // Validate token with Clerk (basic validation for now)
            // TODO: Implement full Clerk JWT validation with public keys
            var isValid = await ValidateTokenWithClerkAsync(token, user.Id);

            if (!isValid)
            {
                return AuthenticationResult.Failure("Invalid token");
            }

            return AuthenticationResult.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed for token");
            return AuthenticationResult.Failure("Authentication failed");
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var result = await AuthenticateAsync(token);
        return result.IsAuthenticated;
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
            Roles = ExtractRoles(claims),
            Permissions = ExtractPermissions(claims),
            LastLoginAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    private IEnumerable<string> ExtractRoles(IEnumerable<Claim> claims)
    {
        // Extract roles from Clerk's role claim or custom metadata
        var rolesClaim = claims.FirstOrDefault(c => c.Type == "roles" || c.Type == "role");
        if (rolesClaim != null)
        {
            try
            {
                // Try to parse as JSON array
                var roles = JsonSerializer.Deserialize<string[]>(rolesClaim.Value);
                return roles ?? Array.Empty<string>();
            }
            catch
            {
                // Fallback to single role
                return new[] { rolesClaim.Value };
            }
        }

        // Default role for healthcare workers
        return new[] { "clinician" };
    }

    private IEnumerable<string> ExtractPermissions(IEnumerable<Claim> claims)
    {
        // Extract permissions from Clerk's metadata or role-based permissions
        var permissions = new List<string>();

        // Basic permissions for clinicians
        permissions.Add("patients.read");
        permissions.Add("patients.assign");
        permissions.Add("shifts.read");

        // Add role-based permissions
        var roles = ExtractRoles(claims);
        if (roles.Contains("admin"))
        {
            permissions.Add("admin.full_access");
        }

        return permissions;
    }

    private Task<bool> ValidateTokenWithClerkAsync(string token, string userId)
    {
        // TODO: Implement full Clerk JWT validation
        // For now, do basic validation
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Check if token is expired
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                return Task.FromResult(false);
            }

            // Check if token has required claims
            if (string.IsNullOrEmpty(userId))
            {
                return Task.FromResult(false);
            }

            // TODO: Validate with Clerk's public keys
            // This would involve:
            // 1. Fetching Clerk's JWKS endpoint
            // 2. Validating the JWT signature
            // 3. Checking issuer and audience

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return Task.FromResult(false);
        }
    }
}
