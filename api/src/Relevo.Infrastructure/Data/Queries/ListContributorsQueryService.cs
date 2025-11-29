using Relevo.UseCases.Contributors;
using Relevo.UseCases.Contributors.List;
using Dapper;

namespace Relevo.Infrastructure.Data.Queries;

public class ListContributorsQueryService(DapperConnectionFactory _connectionFactory) : IListContributorsQueryService
{
    public async Task<IEnumerable<ContributorDTO>> ListAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, Name, PhoneNumber_Number AS PhoneNumber
            FROM Contributors
        """;
        
        return await conn.QueryAsync<ContributorDTO>(sql);
    }
}
