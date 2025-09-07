using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string token);
    Task<bool> ValidateTokenAsync(string token);
}

public interface IAuthorizationService
{
    Task<bool> AuthorizeAsync(User user, string permission);
    Task<bool> HasRoleAsync(User user, string role);
    Task<IEnumerable<string>> GetUserPermissionsAsync(User user);
}

public interface IUserContext
{
    User? CurrentUser { get; }
    bool IsAuthenticated { get; }
    Task SetUserAsync(User user);
    Task ClearUserAsync();
}

public class AuthenticationResult
{
    public bool IsAuthenticated { get; set; }
    public User? User { get; set; }
    public string? Error { get; set; }

    public static AuthenticationResult Success(User user) =>
        new() { IsAuthenticated = true, User = user };

    public static AuthenticationResult Failure(string error) =>
        new() { IsAuthenticated = false, Error = error };
}
