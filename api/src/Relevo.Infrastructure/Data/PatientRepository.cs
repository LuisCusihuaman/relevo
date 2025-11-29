using System.Data;
using Dapper;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

public class PatientRepository(DapperConnectionFactory _connectionFactory) : IPatientRepository
{
  public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(string unitId, int page, int pageSize)
  {
    using var conn = _connectionFactory.CreateConnection();

    var p = Math.Max(page, 1);
    var ps = Math.Max(pageSize, 1);
    var offset = (p - 1) * ps;

    // Count
    var total = await conn.ExecuteScalarAsync<int>(
        "SELECT COUNT(*) FROM PATIENTS WHERE UNIT_ID = :UnitId",
        new { UnitId = unitId });

    if (total == 0)
        return (Array.Empty<PatientRecord>(), 0);

    // Query
    const string sql = @"
      SELECT ID, NAME, HandoverStatus, HandoverId, Age, Room, DIAGNOSIS, Status, Severity FROM (
        SELECT
            p.ID,
            p.NAME,
            'not-started' as HandoverStatus,
            CAST(NULL AS VARCHAR2(50)) as HandoverId,
            ROUND(MONTHS_BETWEEN(SYSDATE, p.DATE_OF_BIRTH)/12, 1) as Age,
            p.ROOM_NUMBER as Room,
            p.DIAGNOSIS,
            CAST(NULL AS VARCHAR2(20)) as Status,
            CAST(NULL AS VARCHAR2(20)) as Severity,
            ROW_NUMBER() OVER (ORDER BY p.NAME) AS RN
        FROM PATIENTS p
        WHERE p.UNIT_ID = :UnitId
      )
      WHERE RN BETWEEN :StartRow AND :EndRow";

    var patients = await conn.QueryAsync<PatientRecord>(sql, new { UnitId = unitId, StartRow = offset + 1, EndRow = offset + ps });

    return (patients.ToList(), total);
  }

  public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetAllPatientsAsync(int page, int pageSize)
  {
    using var conn = _connectionFactory.CreateConnection();

    var p = Math.Max(page, 1);
    var ps = Math.Max(pageSize, 1);
    var offset = (p - 1) * ps;

    // Count
    var total = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM PATIENTS");

    if (total == 0)
        return (Array.Empty<PatientRecord>(), 0);

    // Query
    const string sql = @"
      SELECT ID, NAME, HandoverStatus, HandoverId, Age, Room, DIAGNOSIS, Status, Severity FROM (
        SELECT
            p.ID,
            p.NAME,
            'not-started' as HandoverStatus,
            CAST(NULL AS VARCHAR2(50)) as HandoverId,
            ROUND(MONTHS_BETWEEN(SYSDATE, p.DATE_OF_BIRTH)/12, 1) as Age,
            p.ROOM_NUMBER as Room,
            p.DIAGNOSIS,
            CAST(NULL AS VARCHAR2(20)) as Status,
            CAST(NULL AS VARCHAR2(20)) as Severity,
            ROW_NUMBER() OVER (ORDER BY p.NAME) AS RN
        FROM PATIENTS p
      )
      WHERE RN BETWEEN :StartRow AND :EndRow";

    var patients = await conn.QueryAsync<PatientRecord>(sql, new { StartRow = offset + 1, EndRow = offset + ps });

    return (patients.ToList(), total);
  }
}
