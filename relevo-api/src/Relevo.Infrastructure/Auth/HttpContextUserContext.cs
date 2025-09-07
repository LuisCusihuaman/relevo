using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Auth;

public class HttpContextUserContext : IUserContext
{
    private static readonly AsyncLocal<User?> _currentUser = new();

    public User? CurrentUser => _currentUser.Value;

    public bool IsAuthenticated => CurrentUser != null;

    public Task SetUserAsync(User user)
    {
        _currentUser.Value = user;
        return Task.CompletedTask;
    }

    public Task ClearUserAsync()
    {
        _currentUser.Value = null;
        return Task.CompletedTask;
    }
}
