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
                _logger.LogWarning("Token validation failed for user {UserId}, falling back to demo user", user.Id);
                return AuthenticationResult.Failure("Invalid token");
            }

            // If user details are missing, try to enrich with Clerk user info
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.FirstName))
            {
                var enrichedUser = await EnrichUserWithClerkDataAsync(user.Id, user);
                if (enrichedUser != null)
                {
                    user = enrichedUser;
                }
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

        // Debug: Log all available claims
        _logger.LogInformation("JWT Claims: {Claims}",
            string.Join(", ", claims.Select(c => $"{c.Type}: {c.Value}")));

        // Extract user ID with priority: sub (standard JWT) -> user_id (Clerk specific) -> nameidentifier -> fallback
        var userId = claims.FirstOrDefault(c => c.Type == "sub")?.Value
                   ?? claims.FirstOrDefault(c => c.Type == "user_id")?.Value  // Clerk specific
                   ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                   ?? string.Empty;

        // Log user ID extraction for debugging consistency issues
        _logger.LogDebug("Extracted user ID '{UserId}' from JWT claims for email '{Email}'",
            userId, claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "no-email");

        return new User
        {
            Id = userId,
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
                _logger.LogWarning("Token expired for user {UserId}. ValidTo: {ValidTo}, Current: {Current}",
                    userId, jwtToken.ValidTo, DateTime.UtcNow);
                return Task.FromResult(false);
            }

            // Check if token has required claims
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Token missing user ID claim");
                return Task.FromResult(false);
            }

            // Check issuer
            var issuer = jwtToken.Issuer;
            if (string.IsNullOrEmpty(issuer) || !issuer.Contains("clerk"))
            {
                _logger.LogWarning("Token has invalid issuer: {Issuer}", issuer);
                return Task.FromResult(false);
            }

            _logger.LogInformation("Token validation successful for user {UserId}", userId);

            // TODO: Validate with Clerk's public keys
            // This would involve:
            // 1. Fetching Clerk's JWKS endpoint
            // 2. Validating the JWT signature
            // 3. Checking issuer and audience

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed for user {UserId}", userId);
            return Task.FromResult(false);
        }
    }

    private async Task<User?> EnrichUserWithClerkDataAsync(string userId, User existingUser)
    {
        try
        {
            // Get Clerk API key from configuration
            var clerkApiKey = _configuration?["Clerk:ApiKey"] ?? _configuration?["CLERK_SECRET_KEY"];

            if (string.IsNullOrEmpty(clerkApiKey))
            {
                _logger.LogWarning("Clerk API key not configured, skipping user enrichment");
                return null;
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {clerkApiKey}");

            // Call Clerk's users endpoint to get full user data
            var response = await httpClient.GetAsync($"https://api.clerk.dev/v1/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch user data from Clerk: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var clerkUserData = JsonSerializer.Deserialize<ClerkUserResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (clerkUserData != null)
            {
                // Update user with Clerk data
                existingUser.Email = clerkUserData.EmailAddresses?.FirstOrDefault()?.EmailAddress ?? existingUser.Email;
                existingUser.FirstName = clerkUserData.FirstName ?? existingUser.FirstName;
                existingUser.LastName = clerkUserData.LastName ?? existingUser.LastName;

                _logger.LogInformation("Enriched user {UserId} with Clerk data: {Email}, {FirstName} {LastName}",
                    userId, existingUser.Email, existingUser.FirstName, existingUser.LastName);

                return existingUser;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enrich user data from Clerk for user {UserId}", userId);
        }

        return null;
    }
}

// DTOs for Clerk API response
public class ClerkUserResponse
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<ClerkEmailAddress>? EmailAddresses { get; set; }
}

public class ClerkEmailAddress
{
    public string? EmailAddress { get; set; }
}
