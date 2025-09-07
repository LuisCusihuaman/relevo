using System.Data;
using Dapper;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.Repositories;

public class OracleSetupRepository : ISetupRepository
{
    private readonly IOracleConnectionFactory _factory;

    public OracleSetupRepository(IOracleConnectionFactory factory)
    {
        _factory = factory;
    }

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
        const string pageSql = @"SELECT ID AS Id, NAME AS Name FROM (
          SELECT ID, NAME, ROW_NUMBER() OVER (ORDER BY ID) AS RN
          FROM PATIENTS WHERE UNIT_ID = :unitId
        ) WHERE RN BETWEEN :startRow AND :endRow";

        int total = conn.ExecuteScalar<int>(countSql, new { unitId });
        int startRow = ((p - 1) * ps) + 1;
        int endRow = p * ps;
        var items = conn.Query<PatientRecord>(pageSql, new { unitId, startRow, endRow }).ToList();
        return (items, total);
    }

    public async Task AssignAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        using IDbConnection conn = _factory.CreateConnection();

        // Remove existing assignments for this user
        await conn.ExecuteAsync("DELETE FROM USER_ASSIGNMENTS WHERE USER_ID = :userId",
            new { userId });

        // Insert new assignments
        foreach (var patientId in patientIds)
        {
            await conn.ExecuteAsync(@"
            INSERT INTO USER_ASSIGNMENTS (USER_ID, SHIFT_ID, PATIENT_ID)
            VALUES (:userId, :shiftId, :patientId)",
            new { userId, shiftId, patientId });
        }
    }

    public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(string userId, int page, int pageSize)
    {
        using IDbConnection conn = _factory.CreateConnection();

        // Get total count of assigned patients
        const string countSql = "SELECT COUNT(*) FROM USER_ASSIGNMENTS WHERE USER_ID = :userId";
        int total = conn.ExecuteScalar<int>(countSql, new { userId });

        if (total == 0)
            return (Array.Empty<PatientRecord>(), 0);

        // Get assigned patients with pagination
        int p = Math.Max(page, 1);
        int ps = Math.Max(pageSize, 1);
        int offset = (p - 1) * ps;

        const string patientsSql = @"
          SELECT p.ID AS Id, p.NAME AS Name
          FROM PATIENTS p
          INNER JOIN USER_ASSIGNMENTS ua ON p.ID = ua.PATIENT_ID
          WHERE ua.USER_ID = :userId
          ORDER BY p.ID
          OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY";

        var patients = conn.Query<PatientRecord>(patientsSql,
            new { userId, offset, pageSize });

        return (patients.ToList(), total);
    }

    public (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetMyHandovers(string userId, int page, int pageSize)
    {
        using IDbConnection conn = _factory.CreateConnection();

        // First, get the patient IDs assigned to this user from database
        const string patientIdsSql = "SELECT PATIENT_ID FROM USER_ASSIGNMENTS WHERE USER_ID = :userId";
        var patientIds = conn.Query<string>(patientIdsSql, new { userId }).ToArray();

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
