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

    public async Task EnsureUserExistsAsync(string userId, string? email = null, string? firstName = null, string? lastName = null)
    {
        using var conn = _connectionFactory.CreateConnection();

        // Check if user already exists
        var existingUser = await conn.QueryFirstOrDefaultAsync<string>(
            "SELECT ID FROM USERS WHERE ID = :userId", 
            new { userId });

        if (existingUser != null)
            return; // User already exists, nothing to do

        // Create the user with basic info
        // Use a default email if not provided (required by schema)
        var defaultEmail = email ?? $"{userId}@clerk.local";
        var fullName = (firstName != null && lastName != null) 
            ? $"{firstName} {lastName}" 
            : null;

        await conn.ExecuteAsync(@"
            INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, ROLE, IS_ACTIVE, CREATED_AT, UPDATED_AT)
            VALUES (:userId, :defaultEmail, :firstName, :lastName, :fullName, 'doctor', 1, SYSTIMESTAMP, SYSTIMESTAMP)",
            new { userId, defaultEmail, firstName, lastName, fullName });
    }
}

