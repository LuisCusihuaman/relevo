using Dapper;
using Relevo.Core.ContributorAggregate;
using Relevo.Core.Interfaces;
using System.Data;

// SQLite-specific implementation using EF Core column names
namespace Relevo.Infrastructure.Data.Sqlite;

public class SqliteContributorService(IDbConnectionFactory factory) : IContributorService
{
    private readonly IDbConnectionFactory _factory = factory;

    public async Task<int> CreateAsync(Contributor contributor)
    {
        var conn = _factory.CreateConnection();

        const string sql = @"
            INSERT INTO Contributors (Name, Status, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension)
            VALUES (@Name, @Status, @CountryCode, @Number, @Extension)";

        var phone = contributor.PhoneNumber;
        var parameters = new
        {
            Name = contributor.Name,
            Status = 0, // NotSet status
            CountryCode = phone?.CountryCode ?? "",
            Number = phone?.Number ?? "",
            Extension = phone?.Extension ?? ""
        };

        await conn.ExecuteAsync(sql, parameters);
        return 0; // SQLite auto-increment will handle the ID
    }

    public async Task<Contributor?> GetByIdAsync(int id)
    {
        var conn = _factory.CreateConnection();

        // SQLite syntax: mixed case columns from EF Core
        const string sql = @"SELECT Id, Name, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension FROM Contributors WHERE Id = @Id";
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });

        if (result == null) return null;

        var nameValue = (string?)result.Name ?? "";
        if (string.IsNullOrEmpty(nameValue))
        {
            return null;
        }

        var contributor = new Contributor(nameValue);

        // Set the Id directly from the result
        try
        {
            var idProperty = contributor.GetType().GetProperty("Id");
            if (idProperty != null && idProperty.CanWrite)
            {
                idProperty.SetValue(contributor, (int)result.Id);
            }
        }
        catch (Exception)
        {
            // If we can't set the Id, that's okay
        }

        // Reconstruct phone number from separate columns
        var countryCode = (string?)result.PhoneNumber_CountryCode ?? "";
        var number = (string?)result.PhoneNumber_Number ?? "";
        var extension = (string?)result.PhoneNumber_Extension ?? "";

        if (!string.IsNullOrEmpty(number))
        {
            var phoneNumber = $"{countryCode}{number}";
            if (!string.IsNullOrEmpty(extension))
            {
                phoneNumber += $" ext {extension}";
            }
            contributor.SetPhoneNumber(phoneNumber);
        }

        return contributor;
    }

    public async Task UpdateAsync(Contributor contributor)
    {
        var conn = _factory.CreateConnection();

        const string sql = @"
            UPDATE Contributors
            SET Name = @Name,
                PhoneNumber_CountryCode = @CountryCode,
                PhoneNumber_Number = @Number,
                PhoneNumber_Extension = @Extension
            WHERE Id = @Id";

        var phone = contributor.PhoneNumber;
        await conn.ExecuteAsync(sql, new
        {
            contributor.Id,
            contributor.Name,
            CountryCode = phone?.CountryCode ?? "",
            Number = phone?.Number ?? "",
            Extension = phone?.Extension ?? ""
        });
    }

    public async Task DeleteAsync(int id)
    {
        var conn = _factory.CreateConnection();
        const string sql = "DELETE FROM Contributors WHERE Id = @Id";
        await conn.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<List<Contributor>> GetAllAsync()
    {
        var conn = _factory.CreateConnection();

        // SQLite syntax: mixed case columns from EF Core
        const string sql = @"SELECT Id, Name, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension FROM Contributors";
        var results = await conn.QueryAsync<dynamic>(sql);
        var contributors = new List<Contributor>();

        foreach (var result in results)
        {
            var nameValue = (string)result.Name;
            if (string.IsNullOrEmpty(nameValue))
            {
                continue; // Skip records with empty names
            }

            var contributor = new Contributor(nameValue);

            // Reconstruct phone number from separate columns
            var countryCode = (string?)result.PhoneNumber_CountryCode ?? "";
            var number = (string?)result.PhoneNumber_Number ?? "";
            var extension = (string?)result.PhoneNumber_Extension ?? "";

            if (!string.IsNullOrEmpty(number))
            {
                var phoneNumber = $"{countryCode}{number}";
                if (!string.IsNullOrEmpty(extension))
                {
                    phoneNumber += $" ext {extension}";
                }
                contributor.SetPhoneNumber(phoneNumber);
            }

            contributors.Add(contributor);
        }

        return contributors;
    }
}
