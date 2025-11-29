using System.Security.Claims;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Auth;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    // Standard OIDC 'sub' claim
    public string? Id => User?.FindFirst("sub")?.Value;

    // Standard OIDC 'email' claim
    public string? Email => User?.FindFirst("email")?.Value;

    public bool HasRole(string role) => User?.IsInRole(role) ?? false;
}

