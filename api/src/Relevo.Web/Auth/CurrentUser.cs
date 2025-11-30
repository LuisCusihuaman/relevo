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

    public string? FirstName => User?.FindFirst("given_name")?.Value;
    public string? LastName => User?.FindFirst("family_name")?.Value;
    public string? FullName => User?.FindFirst("name")?.Value;
    public string? AvatarUrl => User?.FindFirst("picture")?.Value;
    public string? OrgRole => User?.FindFirst("org_role")?.Value;

    public bool HasRole(string role) => User?.IsInRole(role) ?? false;
}

