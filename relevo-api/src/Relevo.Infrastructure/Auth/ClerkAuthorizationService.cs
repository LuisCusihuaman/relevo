using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Auth;

public class ClerkAuthorizationService : IAuthorizationService
{
    private readonly ILogger<ClerkAuthorizationService> _logger;

    public ClerkAuthorizationService(ILogger<ClerkAuthorizationService> logger)
    {
        _logger = logger;
    }

    public Task<bool> AuthorizeAsync(User user, string permission)
    {
        if (user == null || !user.IsActive)
        {
            return Task.FromResult(false);
        }

        var hasPermission = user.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);

        _logger.LogInformation("Authorization check for user {UserId}: permission '{Permission}' = {Result}",
            user.Id, permission, hasPermission);

        return Task.FromResult(hasPermission);
    }

    public Task<bool> HasRoleAsync(User user, string role)
    {
        if (user == null || !user.IsActive)
        {
            return Task.FromResult(false);
        }

        var hasRole = user.Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

        _logger.LogInformation("Role check for user {UserId}: role '{Role}' = {Result}",
            user.Id, role, hasRole);

        return Task.FromResult(hasRole);
    }

    public Task<IEnumerable<string>> GetUserPermissionsAsync(User user)
    {
        if (user == null)
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        return Task.FromResult(user.Permissions);
    }
}
