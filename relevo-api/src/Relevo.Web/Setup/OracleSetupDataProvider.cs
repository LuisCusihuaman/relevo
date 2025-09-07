using System.Data;
using Dapper;
using Relevo.Infrastructure.Data.Oracle;
using Relevo.Web.Patients;
using Relevo.Web.Me;

namespace Relevo.Web.Setup;

public class OracleSetupDataProvider(IOracleConnectionFactory _factory) : ISetupDataProvider
{
  // In Oracle-only context for api.setup.http, we pull units/shifts/patients from DB
  // Assignments remain in-memory per-process for demo
  private readonly Dictionary<string, (string ShiftId, HashSet<string> PatientIds)> _assignments = new();

  public IReadOnlyList<UnitRecord> GetUnits()
  {
    using IDbConnection conn = _factory.CreateConnection();
    const string sql = "SELECT ID AS Id, NAME AS Name FROM UNITS ORDER BY ID";
    return conn.Query<UnitRecord>(sql).ToList();
  }

  public IReadOnlyList<ShiftRecord> GetShifts()
  {
    using IDbConnection conn = _factory.CreateConnection();
    const string sql = @"SELECT ID AS Id, NAME AS Name, START_TIME AS StartTime, END_TIME AS EndTime FROM SHIFTS ORDER BY ID";
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

  public (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetMyHandovers(string userId, int page, int pageSize)
  {
    using IDbConnection conn = _factory.CreateConnection();

    // First, get the patient IDs assigned to this user
    if (!_assignments.TryGetValue(userId, out var assignment) || assignment.PatientIds.Count == 0)
      return (Array.Empty<HandoverRecord>(), 0);

    var patientIds = assignment.PatientIds.ToArray();
    if (patientIds.Length == 0)
      return (Array.Empty<HandoverRecord>(), 0);

    // Get total count of handovers for user's patients
    const string countSql = "SELECT COUNT(1) FROM HANDOVERS WHERE PATIENT_ID IN :patientIds";
    int total = conn.ExecuteScalar<int>(countSql, new { patientIds });

    if (total == 0)
      return (Array.Empty<HandoverRecord>(), 0);

    // Get handovers with pagination
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);
    int offset = (p - 1) * ps;

    const string handoverSql = @"
      SELECT h.ID, h.PATIENT_ID, h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY,
             h.SITUATION_AWARENESS_DOC_ID, h.SYNTHESIS, h.SHIFT_NAME
      FROM HANDOVERS h
      WHERE h.PATIENT_ID IN :patientIds
      ORDER BY h.CREATED_AT DESC
      OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY";

    var handoverRows = conn.Query(handoverSql, new { patientIds, offset, pageSize }).ToList();

    // Get action items for each handover
    var handovers = new List<HandoverRecord>();
    foreach (var row in handoverRows)
    {
      var handoverId = row.ID;

      const string actionItemsSql = @"
        SELECT ID, DESCRIPTION, IS_COMPLETED
        FROM HANDOVER_ACTION_ITEMS
        WHERE HANDOVER_ID = :handoverId
        ORDER BY CREATED_AT";

      var actionItems = conn.Query(actionItemsSql, new { handoverId })
        .Select(item => new HandoverActionItem(item.ID, item.DESCRIPTION, item.IS_COMPLETED == 1))
        .ToList();

      var handover = new HandoverRecord(
        Id: row.ID,
        PatientId: row.PATIENT_ID,
        Status: row.STATUS,
        IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
        PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
        ActionItems: actionItems,
        SituationAwarenessDocId: row.SITUATION_AWARENESS_DOC_ID,
        Synthesis: string.IsNullOrEmpty(row.SYNTHESIS) ? null : new HandoverSynthesis(row.SYNTHESIS)
      );

      handovers.Add(handover);
    }

    return (handovers, total);
  }
}


