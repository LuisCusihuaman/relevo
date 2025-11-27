using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;
using System.Collections.Generic;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleShiftRepository : IShiftRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleShiftRepository> _logger;

    public OracleShiftRepository(IOracleConnectionFactory factory, ILogger<OracleShiftRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public IReadOnlyList<ShiftRecord> GetShifts()
    {
        using IDbConnection conn = _factory.CreateConnection();
        const string sql = @"SELECT ID AS Id, NAME AS Name, START_TIME AS StartTime, END_TIME AS EndTime FROM SHIFTS ORDER BY ID";
        return conn.Query<ShiftRecord>(sql).ToList();
    }
}
