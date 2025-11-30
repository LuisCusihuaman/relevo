using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Relevo.Web.Auth;

public static class ClerkAuthExtensions
{
    public static IServiceCollection AddClerkAuthentication(this IServiceCollection services, IConfiguration config)
    {
        // 1. Load Configuration
        var issuer = config["Clerk:Issuer"] 
            ?? throw new InvalidOperationException("Clerk:Issuer not configured");
        
        var authorizedParties = config.GetSection("Clerk:AuthorizedParties").Get<string[]>() ?? [];

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // 2. Disable Legacy Microsoft Claim Mapping
                // Keeps 'sub' as 'sub', instead of converting to 'http://schemas.../nameidentifier'
                options.MapInboundClaims = false;

                // 3. OIDC / OAuth2 Discovery
                // Pointing to oauth-authorization-server is more specific for APIs than openid-configuration
                options.Authority = issuer;
                options.MetadataAddress = $"{issuer}/.well-known/oauth-authorization-server";

                // 4. Token Validation Parameters
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    // Clerk specific: Audience is often not set in Frontend tokens, 
                    // relying on 'azp' instead. (Keycloak will require this to be true).
                    ValidateAudience = false, 
                    
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true, // Cryptographic signature check (JWKS)
                    ClockSkew = TimeSpan.FromMinutes(1),

                    NameClaimType = "sub", // Map User.Identity.Name to the User ID
                    RoleClaimType = "roles"
                };

                options.Events = new JwtBearerEvents
                {
                    // 5. Hybrid Token Extraction (The "Bridge")
                    OnMessageReceived = ctx =>
                    {
                        if (string.IsNullOrEmpty(ctx.Token))
                        {
                            // A. Legacy Header support (from your existing Frontend)
                            var clerkHeader = ctx.Request.Headers["x-clerk-user-token"].FirstOrDefault();
                            if (!string.IsNullOrEmpty(clerkHeader))
                                ctx.Token = clerkHeader;

                            // B. Cookie support (Future-proofing for Same-Origin)
                            else if (ctx.Request.Cookies.TryGetValue("__session", out var cookieToken))
                                ctx.Token = cookieToken;
                        }
                        return Task.CompletedTask;
                    },

                    // 6. Security: Authorized Party Validation
                    OnTokenValidated = ctx =>
                    {
                        var azp = ctx.Principal?.FindFirst("azp")?.Value;
                        
                        // Reject if 'azp' is present but not in our allow-list
                        if (!string.IsNullOrEmpty(azp) &&
                            authorizedParties.Length > 0 &&
                            !authorizedParties.Contains(azp, StringComparer.OrdinalIgnoreCase))
                        {
                            ctx.Fail("Invalid azp (Authorized Party)");
                        }
                        
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }
}

