using Microsoft.AspNetCore.Builder;

namespace Relevo.Web.Middleware;

public static class ClerkAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseClerkAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ClerkAuthenticationMiddleware>();
    }
}
