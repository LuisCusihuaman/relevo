using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

public interface IUserRepository
{
    Task<UserProfileRecord?> GetUserByIdAsync(string userId);
    
    /// <summary>
    /// Ensures the user exists in the local database.
    /// If the user doesn't exist, creates a new record with the provided info.
    /// This is used for "lazy provisioning" when a user authenticated via Clerk
    /// performs their first write operation.
    /// </summary>
    Task EnsureUserExistsAsync(string userId, string? email = null, string? firstName = null, string? lastName = null);
}

