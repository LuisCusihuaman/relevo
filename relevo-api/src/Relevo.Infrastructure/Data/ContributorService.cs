using Dapper;
using Relevo.Core.ContributorAggregate;
using System.Data;

namespace Relevo.Infrastructure.Data;

// Oracle-specific implementation using Oracle syntax
public class OracleContributorService(IDbConnectionFactory factory) : Relevo.Core.Interfaces.IContributorService
{
    private readonly IDbConnectionFactory _factory = factory;

    public async Task<int> CreateAsync(Contributor contributor)
    {
        var conn = _factory.CreateConnection();

        const string sql = @"
            INSERT INTO CONTRIBUTORS (NAME, EMAIL, PHONE_NUMBER)
            VALUES (@Name, @Email, @PhoneNumber)";

        var phone = contributor.PhoneNumber;
        var parameters = new
        {
            Name = contributor.Name,
            Email = $"{contributor.Name.ToLower().Replace(" ", ".")}@hospital.com.ar",
            PhoneNumber = phone?.ToString() ?? string.Empty
        };

        await conn.ExecuteAsync(sql, parameters);
        return 0; // Oracle auto-increment will handle the ID
    }

    public async Task<Contributor?> GetByIdAsync(int id)
    {
        var conn = _factory.CreateConnection();

        // Oracle syntax: uppercase columns
        const string sql = @"SELECT ID, NAME, EMAIL, PHONE_NUMBER FROM CONTRIBUTORS WHERE ID = @Id";
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });

        if (result == null) return null;

        var nameValue = (string?)result.NAME ?? "";
        if (string.IsNullOrEmpty(nameValue))
        {
            return null;
        }

        var contributor = new Contributor(nameValue);

        // Set the Id using reflection
        try
        {
            var idProperty = contributor.GetType().GetProperty("Id");
            if (idProperty != null && idProperty.CanWrite)
            {
                idProperty.SetValue(contributor, (int)result.ID);
            }
        }
        catch (Exception)
        {
            // If we can't set the Id, that's okay
        }

        // Set phone number (Oracle single column)
        if (!string.IsNullOrEmpty((string?)result.PHONE_NUMBER))
        {
            contributor.SetPhoneNumber((string)result.PHONE_NUMBER);
        }

        return contributor;
    }

    public async Task UpdateAsync(Contributor contributor)
    {
        var conn = _factory.CreateConnection();

        const string sql = @"
            UPDATE CONTRIBUTORS
            SET NAME = @Name,
                EMAIL = @Email,
                PHONE_NUMBER = @PhoneNumber
            WHERE ID = @Id";

        var phone = contributor.PhoneNumber;
        await conn.ExecuteAsync(sql, new
        {
            contributor.Id,
            contributor.Name,
            Email = $"{contributor.Name.ToLower().Replace(" ", ".")}@hospital.com.ar",
            PhoneNumber = phone?.ToString() ?? string.Empty
        });
    }

    public async Task DeleteAsync(int id)
    {
        var conn = _factory.CreateConnection();
        const string sql = "DELETE FROM CONTRIBUTORS WHERE ID = @Id";
        await conn.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<List<Contributor>> GetAllAsync()
    {
        var conn = _factory.CreateConnection();

        // Oracle syntax: uppercase columns
        const string sql = @"SELECT ID, NAME, EMAIL, PHONE_NUMBER FROM CONTRIBUTORS";
        var results = await conn.QueryAsync<dynamic>(sql);
        var contributors = new List<Contributor>();

        foreach (var result in results)
        {
            var nameValue = (string)result.NAME;
            if (string.IsNullOrEmpty(nameValue))
            {
                continue; // Skip records with empty names
            }

            var contributor = new Contributor(nameValue);

            // Set phone number (Oracle single column)
            if (!string.IsNullOrEmpty((string?)result.PHONE_NUMBER))
            {
                contributor.SetPhoneNumber((string)result.PHONE_NUMBER);
            }

            contributors.Add(contributor);
        }

        return contributors;
    }
}
