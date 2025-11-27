using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;
using System.Collections.Generic;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleUnitRepository : IUnitRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleUnitRepository> _logger;

    public OracleUnitRepository(IOracleConnectionFactory factory, ILogger<OracleUnitRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public IReadOnlyList<UnitRecord> GetUnits()
    {
        using IDbConnection conn = _factory.CreateConnection();
        const string sql = "SELECT ID AS Id, NAME AS Name FROM UNITS ORDER BY ID";
        return conn.Query<UnitRecord>(sql).ToList();
    }
}
