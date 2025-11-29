using Dapper;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

public class UserRepository(DapperConnectionFactory _connectionFactory) : IUserRepository
{
    public async Task<UserProfileRecord?> GetUserByIdAsync(string userId)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT ID, EMAIL, FIRST_NAME as FirstName, LAST_NAME as LastName, FULL_NAME as FullName
            FROM USERS
            WHERE ID = :userId";

        var user = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { userId });

        if (user == null)
            return null;

        return new UserProfileRecord(
            Id: (string)user.ID,
            Email: (string?)user.EMAIL ?? "",
            FirstName: (string?)user.FIRSTNAME ?? "",
            LastName: (string?)user.LASTNAME ?? "",
            FullName: (string?)user.FULLNAME ?? "",
            Roles: new List<string> { "Doctor" }, // Default role
            IsActive: true
        );
    }
}

