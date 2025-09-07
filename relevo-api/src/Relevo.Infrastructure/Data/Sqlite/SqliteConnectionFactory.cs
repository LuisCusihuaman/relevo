using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Relevo.Infrastructure.Data.Sqlite;

public interface ISqliteConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqliteConnectionFactory(IConfiguration configuration) : ISqliteConnectionFactory, IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        // Prefer explicit SQLite connection string, fallback to generic ConnectionStrings:SqliteConnection
        string? conn = configuration["SqliteConnection"] ?? configuration.GetConnectionString("SqliteConnection");

        if (string.IsNullOrWhiteSpace(conn))
        {
            throw new InvalidOperationException("SQLite connection string not configured. Set SqliteConnection or ConnectionStrings:SqliteConnection.");
        }

        return new SqliteConnection(conn);
    }
}
