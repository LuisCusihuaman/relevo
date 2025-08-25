using System.Data;
using Dapper;
using Relevo.Infrastructure.Data.Oracle;
using Relevo.Web.Patients;

namespace Relevo.Web.Setup;

public class OracleSetupDataProvider(IOracleConnectionFactory _factory) : ISetupDataProvider
{
  // In Oracle-only context for api.setup.http, we pull units/shifts/patients from DB
  // Assignments remain in-memory per-process for demo
  private readonly Dictionary<string, (string ShiftId, HashSet<string> PatientIds)> _assignments = new();

  public IReadOnlyList<UnitRecord> GetUnits()
  {
    using IDbConnection conn = _factory.CreateConnection();
    const string sql = "SELECT ID AS Id, NAME AS Name FROM UNITS";
    return conn.Query<UnitRecord>(sql).ToList();
  }

  public IReadOnlyList<ShiftRecord> GetShifts()
  {
    using IDbConnection conn = _factory.CreateConnection();
    const string sql = @"SELECT ID AS Id, NAME AS Name, START_TIME AS StartTime, END_TIME AS EndTime FROM SHIFTS";
    return conn.Query<ShiftRecord>(sql).ToList();
  }

  public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(string unitId, int page, int pageSize)
  {
    using IDbConnection conn = _factory.CreateConnection();
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);
    const string countSql = "SELECT COUNT(1) FROM PATIENTS WHERE UNIT_ID = :unitId";
    const string pageSql = @"SELECT * FROM (
      SELECT ID AS Id, NAME AS Name, ROW_NUMBER() OVER (ORDER BY ID) AS RN
      FROM PATIENTS WHERE UNIT_ID = :unitId
    ) WHERE RN BETWEEN :startRow AND :endRow";

    int total = conn.ExecuteScalar<int>(countSql, new { unitId });
    int startRow = ((p - 1) * ps) + 1;
    int endRow = p * ps;
    var items = conn.Query<PatientRecord>(pageSql, new { unitId, startRow, endRow }).ToList();
    return (items, total);
  }

  public void Assign(string userId, string shiftId, IEnumerable<string> patientIds)
  {
    if (!_assignments.TryGetValue(userId, out var existing))
    {
      existing = (shiftId, new HashSet<string>());
    }
    existing.ShiftId = shiftId;
    existing.PatientIds = new HashSet<string>(patientIds);
    _assignments[userId] = existing;
  }

  public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(string userId, int page, int pageSize)
  {
    if (!_assignments.TryGetValue(userId, out var assignment) || assignment.PatientIds.Count == 0)
      return (Array.Empty<PatientRecord>(), 0);

    using IDbConnection conn = _factory.CreateConnection();
    // Fetch subset by ids; to keep simple, perform IN query
    var ids = assignment.PatientIds.ToArray();
    if (ids.Length == 0) return (Array.Empty<PatientRecord>(), 0);

    // Build a dynamic IN clause using Dapper parameter expansion
    const string sql = "SELECT ID AS Id, NAME AS Name FROM PATIENTS WHERE ID IN :ids";
    var selected = conn.Query<PatientRecord>(sql, new { ids }).ToList();

    int total = selected.Count;
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);
    var pageItems = selected.Skip((p - 1) * ps).Take(ps).ToList();
    return (pageItems, total);
  }
}


