using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

public interface IUserRepository
{
    Task<UserProfileRecord?> GetUserByIdAsync(string userId);
}

