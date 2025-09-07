using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using System.Threading;

namespace Relevo.Web.Middleware;

public class ClerkAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClerkAuthenticationMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ClerkAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<ClerkAuthenticationMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IAuthenticationService authenticationService,
        IUserContext userContext)
    {
        // Skip authentication for certain endpoints
        if (IsPublicEndpoint(context.Request.Path))
        {
            _logger.LogInformation("Public endpoint detected, skipping auth: {Path}", context.Request.Path);
            await _next(context);
            return;
        }

        // For anonymous endpoints, try to authenticate with token first, then fall back to demo user
        if (IsAnonymousEndpoint(context))
        {
            _logger.LogInformation("Anonymous endpoint detected: {Path}", context.Request.Path);

            // Extract token from headers
            var token = ExtractToken(context.Request);
            _logger.LogInformation("Extracted token: {TokenPresent} for {Path}", !string.IsNullOrEmpty(token), context.Request.Path);

            if (!string.IsNullOrEmpty(token))
            {
                // Try to authenticate with the provided token
                _logger.LogInformation("🔐 Attempting authentication with token for {Path}", context.Request.Path);
                var authResult = await authenticationService.AuthenticateAsync(token);
                _logger.LogInformation("🔐 Authentication result for {Path}: IsAuthenticated={IsAuth}, HasUser={HasUser}, Error={Error}",
                    context.Request.Path, authResult.IsAuthenticated, authResult.User != null, authResult.Error ?? "none");

                if (authResult.IsAuthenticated && authResult.User != null)
                {
                    // Use the authenticated user
                    await userContext.SetUserAsync(authResult.User);

                    // Log authenticated user details
                    _logger.LogInformation("Authenticated user {UserId} ({UserEmail}) for anonymous endpoint {Path}",
                        authResult.User.Id, authResult.User.Email, context.Request.Path);

                    // Add user info to response headers
                    context.Response.Headers["X-User-Id"] = authResult.User.Id;
                    context.Response.Headers["X-User-Email"] = authResult.User.Email;

                    await _next(context);
                    return;
                }
            }

            // Fall back to demo user if no valid token
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

            // Ensure AsyncLocal flows to the current execution context
            var currentUser = userContext.CurrentUser;
            _logger.LogInformation("Using fallback demo user {UserId} ({UserEmail}) for anonymous endpoint {Path}",
                demoUser.Id, demoUser.Email, context.Request.Path);
            _logger.LogInformation("Set demo user in context: {UserId}, Retrieved: {RetrievedId}", demoUser.Id, currentUser?.Id);

            // Add user info to response headers for debugging (even for anonymous/demo users)
            context.Response.Headers["X-User-Id"] = demoUser.Id;
            context.Response.Headers["X-User-Email"] = demoUser.Email;

            await _next(context);
            return;
        }
        else
        {
            _logger.LogInformation("Authenticated endpoint detected: {Path}", context.Request.Path);
        }

        try
        {
            // Extract token from headers
            var token = ExtractToken(context.Request);

            if (string.IsNullOrEmpty(token))
            {
                // Check if this is a test environment (detected by assembly name)
                var isTestEnvironment = AppDomain.CurrentDomain.GetAssemblies()
                    .Any(a => a.FullName?.Contains("Test") == true ||
                             a.FullName?.Contains("FunctionalTests") == true ||
                             a.FullName?.Contains("UnitTests") == true);

                // In development AND not in tests, allow anonymous access with demo user
                if (_environment.IsDevelopment() && !isTestEnvironment)
                {
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

                    // Ensure AsyncLocal flows to the current execution context
                    var retrievedUser = userContext.CurrentUser;
                    _logger.LogInformation("Using development demo user {UserId} ({UserEmail}) for {Path}",
                        demoUser.Id, demoUser.Email, context.Request.Path);
                    _logger.LogInformation("Set demo user in context: {UserId}, Retrieved: {RetrievedId}", demoUser.Id, retrievedUser?.Id);

                    // Add user info to response headers for debugging (even for anonymous/demo users)
                    context.Response.Headers["X-User-Id"] = demoUser.Id;
                    context.Response.Headers["X-User-Email"] = demoUser.Email;

                    await _next(context);
                    return;
                }
                else
                {
                    // In non-development environments OR tests, require authentication
                    _logger.LogWarning("No authentication token provided for {Path}, rejecting request", context.Request.Path);

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Authentication required",
                        message = "No authentication token provided"
                    });
                    return;
                }
            }

            // Authenticate the user
            var authResult = await authenticationService.AuthenticateAsync(token);

            if (!authResult.IsAuthenticated || authResult.User == null)
            {
                _logger.LogWarning("❌ Authentication failed for {Path}: {Error}. Token length: {TokenLength}",
                    context.Request.Path, authResult.Error ?? "Unknown error", token?.Length ?? 0);

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

            // Ensure AsyncLocal flows to the current execution context
            var currentUser = userContext.CurrentUser;
            _logger.LogInformation("Set authenticated user in context: {UserId}, Retrieved: {RetrievedId}",
                authResult.User.Id, currentUser?.Id);

            // Add user info to response headers for debugging
            context.Response.Headers["X-User-Id"] = authResult.User.Id;
            context.Response.Headers["X-User-Email"] = authResult.User.Email;

            // Log comprehensive user authentication details
            var userName = $"{authResult.User.FirstName} {authResult.User.LastName}".Trim();
            _logger.LogInformation("✅ Authenticated user {UserId} ({UserEmail}) '{UserName}' for {Path}",
                authResult.User.Id, authResult.User.Email, userName, context.Request.Path);

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

    private bool IsAnonymousEndpoint(HttpContext context)
    {
        // Check if the endpoint has AllowAnonymous attribute
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            // Check for AllowAnonymous metadata
            var allowAnonymous = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();
            if (allowAnonymous != null)
            {
                return true;
            }

            // Check for any anonymous-related metadata
            var anyAnonymous = endpoint.Metadata.Any(m => m.GetType().Name.Contains("Anonymous"));
            if (anyAnonymous)
            {
                return true;
            }
        }

        return false;
    }

    private string? ExtractToken(HttpRequest request)
    {
        _logger.LogInformation("Headers received for {Path}: {Headers}",
            request.Path,
            string.Join(", ", request.Headers.Select(h => $"{h.Key}: {h.Value}")));

        // Try to extract from Authorization header first
        var authHeader = request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) &&
            authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Found Bearer token in Authorization header for {Path}", request.Path);
            return authHeader[7..]; // Remove "Bearer " prefix
        }

        // Try Clerk-specific header
        var clerkHeader = request.Headers["x-clerk-user-token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(clerkHeader))
        {
            _logger.LogInformation("Found Clerk token in x-clerk-user-token header for {Path}", request.Path);
            return clerkHeader;
        }

        // Fallback to query parameter for development/testing
        var tokenParam = request.Query["token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(tokenParam))
        {
            _logger.LogInformation("Found token in query parameter for {Path}", request.Path);
            return tokenParam;
        }

        _logger.LogWarning("No token found in any location for {Path}", request.Path);
        return null;
    }
}
