namespace Relevo.Core.Interfaces;

public interface ICurrentUser
{
    string? Id { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool HasRole(string role);
}

