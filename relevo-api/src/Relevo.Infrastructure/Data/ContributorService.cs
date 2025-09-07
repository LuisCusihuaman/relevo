using Dapper;
using Relevo.Core.ContributorAggregate;
using System.Data;

namespace Relevo.Infrastructure.Data;

// Simple service for Contributor operations using Dapper
public class ContributorService(IDbConnectionFactory factory) : Relevo.Core.Interfaces.IContributorService
{
    private readonly IDbConnectionFactory _factory = factory;

    public async Task<int> CreateAsync(Contributor contributor)
    {
        using IDbConnection conn = _factory.CreateConnection();

        const string sql = @"
            INSERT INTO Contributors (Name, Status, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension)
            VALUES (:Name, :Status, :CountryCode, :Number, :Extension)
            RETURNING Id";

        var phone = contributor.PhoneNumber;
        var parameters = new DynamicParameters();
        parameters.Add("Name", contributor.Name);
        parameters.Add("Status", contributor.Status.ToString());
        parameters.Add("CountryCode", phone?.CountryCode ?? string.Empty);
        parameters.Add("Number", phone?.Number ?? string.Empty);
        parameters.Add("Extension", phone?.Extension ?? string.Empty);

        var newId = await conn.ExecuteScalarAsync<int>(sql, parameters);
        return newId;
    }

    public async Task<Contributor?> GetByIdAsync(int id)
    {
        using IDbConnection conn = _factory.CreateConnection();

        const string sql = @"
            SELECT Id, Name, Status,
                   PhoneNumber_CountryCode as CountryCode,
                   PhoneNumber_Number as Number,
                   PhoneNumber_Extension as Extension
            FROM Contributors WHERE Id = :Id";

        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });
        if (result == null) return null;

        var contributor = new Contributor((string)result.Name);

        // Set ID using reflection (simplified approach)
        var idProperty = typeof(Contributor).GetProperty("Id");
        if (idProperty != null)
        {
            idProperty.SetValue(contributor, (int)result.Id);
        }

        // Set phone number if exists
        if (!string.IsNullOrEmpty((string?)result.Number))
        {
            var phoneNumber = $"{result.CountryCode}{result.Number}";
            if (!string.IsNullOrEmpty((string?)result.Extension))
            {
                phoneNumber += $" ext {result.Extension}";
            }
            contributor.SetPhoneNumber(phoneNumber);
        }

        return contributor;
    }

    public async Task UpdateAsync(Contributor contributor)
    {
        using IDbConnection conn = _factory.CreateConnection();

        const string sql = @"
            UPDATE Contributors
            SET Name = :Name,
                Status = :Status,
                PhoneNumber_CountryCode = :CountryCode,
                PhoneNumber_Number = :Number,
                PhoneNumber_Extension = :Extension
            WHERE Id = :Id";

        var phone = contributor.PhoneNumber;
        await conn.ExecuteAsync(sql, new
        {
            contributor.Id,
            contributor.Name,
            Status = contributor.Status.ToString(),
            CountryCode = phone?.CountryCode ?? string.Empty,
            Number = phone?.Number ?? string.Empty,
            Extension = phone?.Extension ?? string.Empty
        });
    }

    public async Task DeleteAsync(int id)
    {
        using IDbConnection conn = _factory.CreateConnection();
        const string sql = "DELETE FROM Contributors WHERE Id = :Id";
        await conn.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<List<Contributor>> GetAllAsync()
    {
        using IDbConnection conn = _factory.CreateConnection();

        const string sql = @"
            SELECT Id, Name, Status,
                   PhoneNumber_CountryCode as CountryCode,
                   PhoneNumber_Number as Number,
                   PhoneNumber_Extension as Extension
            FROM Contributors";

        var results = await conn.QueryAsync<dynamic>(sql);
        var contributors = new List<Contributor>();

        foreach (var result in results)
        {
            var contributor = new Contributor((string)result.Name);

            // Set ID using reflection
            var idProperty = typeof(Contributor).GetProperty("Id");
            if (idProperty != null)
            {
                idProperty.SetValue(contributor, (int)result.Id);
            }

            // Set phone number if exists
            if (!string.IsNullOrEmpty((string?)result.Number))
            {
                var phoneNumber = $"{result.CountryCode}{result.Number}";
                if (!string.IsNullOrEmpty((string?)result.Extension))
                {
                    phoneNumber += $" ext {result.Extension}";
                }
                contributor.SetPhoneNumber(phoneNumber);
            }

            contributors.Add(contributor);
        }

        return contributors;
    }
}
