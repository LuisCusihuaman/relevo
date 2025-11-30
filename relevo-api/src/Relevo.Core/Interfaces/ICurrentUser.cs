namespace Relevo.Core.Interfaces;

public interface ICurrentUser
{
    string? Id { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
    string? FullName { get; }
    string? AvatarUrl { get; }
    string? OrgRole { get; }
    
    bool IsAuthenticated { get; }
    bool HasRole(string role);
}

