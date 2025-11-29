namespace Relevo.Core.Models;

public record UserProfileRecord(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    IReadOnlyList<string> Roles,
    bool IsActive
);

