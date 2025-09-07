using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Middleware;

public class ClerkAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClerkAuthenticationMiddleware> _logger;

    public ClerkAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<ClerkAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IAuthenticationService authenticationService,
        IUserContext userContext)
    {
        // Skip authentication for certain endpoints
        if (IsPublicEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        try
        {
            // Extract token from headers
            var token = ExtractToken(context.Request);

            if (string.IsNullOrEmpty(token))
            {
                // Allow anonymous access with demo user for development
                _logger.LogWarning("No authentication token provided for {Path}, allowing anonymous access", context.Request.Path);

                // Create a demo user for development
                var demoUser = new Relevo.Core.Models.User
                {
                    Id = "demo-user",
                    Email = "demo@example.com",
                    FirstName = "Demo",
                    LastName = "User",
                    Roles = new[] { "clinician" },
                    Permissions = new[] { "patients.read", "patients.assign" },
                    IsActive = true
                };
                await userContext.SetUserAsync(demoUser);

                await _next(context);
                return;
            }

            // Authenticate the user
            var authResult = await authenticationService.AuthenticateAsync(token);

            if (!authResult.IsAuthenticated || authResult.User == null)
            {
                _logger.LogWarning("Authentication failed for {Path}: {Error}",
                    context.Request.Path, authResult.Error);

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Authentication failed",
                    message = authResult.Error ?? "Invalid or expired token"
                });
                return;
            }

            // Set the user in the context
            await userContext.SetUserAsync(authResult.User);

            // Add user info to response headers for debugging
            context.Response.Headers["X-User-Id"] = authResult.User.Id;
            context.Response.Headers["X-User-Email"] = authResult.User.Email;

            _logger.LogInformation("Authenticated user {UserId} for {Path}",
                authResult.User.Id, context.Request.Path);

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication middleware error for {Path}", context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Authentication error",
                message = "An error occurred during authentication"
            });
        }
    }

    private bool IsPublicEndpoint(PathString path)
    {
        // Define public endpoints that don't require authentication
        var publicPaths = new[]
        {
            "/setup",
            "/swagger",
            "/health",
            "/metrics"
        };

        return publicPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }

    private string? ExtractToken(HttpRequest request)
    {
        // Try to extract from Authorization header first
        var authHeader = request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) &&
            authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader[7..]; // Remove "Bearer " prefix
        }

        // Try Clerk-specific header
        var clerkHeader = request.Headers["x-clerk-user-token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(clerkHeader))
        {
            return clerkHeader;
        }

        // Fallback to query parameter for development/testing
        var tokenParam = request.Query["token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(tokenParam))
        {
            return tokenParam;
        }

        return null;
    }
}
