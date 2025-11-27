using System.Collections.Generic;

namespace Relevo.Core.Interfaces;

public interface IUserRepository
{
    void EnsureUserExists(string userId, string? email, string? firstName, string? lastName, string? fullName);
    UserPreferencesRecord? GetUserPreferences(string userId);
    IReadOnlyList<UserSessionRecord> GetUserSessions(string userId);
    bool UpdateUserPreferences(string userId, UserPreferencesRecord preferences);
}
