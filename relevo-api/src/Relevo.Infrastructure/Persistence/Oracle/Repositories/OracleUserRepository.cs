using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;
using System.Collections.Generic;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleUserRepository : IUserRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleUserRepository> _logger;

    public OracleUserRepository(IOracleConnectionFactory factory, ILogger<OracleUserRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public void EnsureUserExists(string userId, string? email, string? firstName, string? lastName, string? fullName)
    {
        using IDbConnection conn = _factory.CreateConnection();

        // Check if user already exists
        var existingUser = conn.QueryFirstOrDefault("SELECT ID FROM USERS WHERE ID = :userId", new { userId });

        if (existingUser != null)
        {
            // User already exists, nothing to do
            return;
        }

        // Create the user with basic info
        // Use a default email if not provided (required by schema)
        var defaultEmail = email ?? $"{userId}@clerk.local";
        conn.Execute(@"
            INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, ROLE, IS_ACTIVE, CREATED_AT, UPDATED_AT)
            VALUES (:userId, :defaultEmail, :firstName, :lastName, :fullName, 'doctor', 1, SYSTIMESTAMP, SYSTIMESTAMP)",
            new { userId, defaultEmail, firstName, lastName, fullName });
    }

    public UserPreferencesRecord? GetUserPreferences(string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                SELECT ID, USER_ID, THEME, LANGUAGE, TIMEZONE, NOTIFICATIONS_ENABLED, AUTO_SAVE_ENABLED,
                       CREATED_AT, UPDATED_AT
                FROM USER_PREFERENCES
                WHERE USER_ID = :userId";

            var row = conn.QueryFirstOrDefault(sql, new { userId });

            if (row == null)
            {
                return null;
            }

            return new UserPreferencesRecord(
                Id: row.ID,
                UserId: row.USER_ID,
                Theme: row.THEME ?? "light",
                Language: row.LANGUAGE ?? "en",
                Timezone: row.TIMEZONE ?? "UTC",
                NotificationsEnabled: row.NOTIFICATIONS_ENABLED == 1,
                AutoSaveEnabled: row.AUTO_SAVE_ENABLED == 1,
                CreatedAt: row.CREATED_AT,
                UpdatedAt: row.UPDATED_AT
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user preferences for user {UserId}", userId);
            return null;
        }
    }

    public IReadOnlyList<UserSessionRecord> GetUserSessions(string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                SELECT ID, USER_ID, SESSION_START, SESSION_END, IP_ADDRESS, USER_AGENT, IS_ACTIVE
                FROM USER_SESSIONS
                WHERE USER_ID = :userId
                ORDER BY SESSION_START DESC";

            var sessions = conn.Query<UserSessionRecord>(sql, new { userId }).ToList();

            return sessions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user sessions for user {UserId}", userId);
            return Array.Empty<UserSessionRecord>();
        }
    }

    public bool UpdateUserPreferences(string userId, UserPreferencesRecord preferences)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            // Check if preferences exist
            const string checkSql = "SELECT COUNT(1) FROM USER_PREFERENCES WHERE USER_ID = :userId";
            var exists = conn.ExecuteScalar<int>(checkSql, new { userId }) > 0;

            if (exists)
            {
                // Update existing preferences
                const string updateSql = @"
                    UPDATE USER_PREFERENCES
                    SET THEME = :theme, LANGUAGE = :language, TIMEZONE = :timezone,
                        NOTIFICATIONS_ENABLED = :notificationsEnabled, AUTO_SAVE_ENABLED = :autoSaveEnabled,
                        UPDATED_AT = SYSTIMESTAMP
                    WHERE USER_ID = :userId";

                var rowsAffected = conn.Execute(updateSql, new
                {
                    userId,
                    theme = preferences.Theme,
                    language = preferences.Language,
                    timezone = preferences.Timezone,
                    notificationsEnabled = preferences.NotificationsEnabled ? 1 : 0,
                    autoSaveEnabled = preferences.AutoSaveEnabled ? 1 : 0
                });

                return rowsAffected > 0;
            }
            else
            {
                // Insert new preferences
                const string insertSql = @"
                    INSERT INTO USER_PREFERENCES (ID, USER_ID, THEME, LANGUAGE, TIMEZONE, NOTIFICATIONS_ENABLED, AUTO_SAVE_ENABLED, CREATED_AT, UPDATED_AT)
                    VALUES (:id, :userId, :theme, :language, :timezone, :notificationsEnabled, :autoSaveEnabled, SYSTIMESTAMP, SYSTIMESTAMP)";

                var rowsAffected = conn.Execute(insertSql, new
                {
                    id = preferences.Id,
                    userId,
                    theme = preferences.Theme,
                    language = preferences.Language,
                    timezone = preferences.Timezone,
                    notificationsEnabled = preferences.NotificationsEnabled ? 1 : 0,
                    autoSaveEnabled = preferences.AutoSaveEnabled ? 1 : 0
                });

                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user preferences for user {UserId}", userId);
            throw;
        }
    }
}
