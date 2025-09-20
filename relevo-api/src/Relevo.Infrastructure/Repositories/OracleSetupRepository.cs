using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.Repositories;

public class OracleSetupRepository : ISetupRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleSetupRepository> _logger;

    public OracleSetupRepository(IOracleConnectionFactory factory, ILogger<OracleSetupRepository> logger)
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
        const string pageSql = @"SELECT ID AS Id, NAME AS Name, 'NotStarted' AS HandoverStatus, CAST(NULL AS VARCHAR(255)) AS HandoverId FROM (
          SELECT ID, NAME, ROW_NUMBER() OVER (ORDER BY ID) AS RN
          FROM PATIENTS WHERE UNIT_ID = :unitId
        ) WHERE RN BETWEEN :startRow AND :endRow";

        int total = conn.ExecuteScalar<int>(countSql, new { unitId });
        int startRow = ((p - 1) * ps) + 1;
        int endRow = p * ps;
        var items = conn.Query<PatientRecord>(pageSql, new { unitId, startRow, endRow }).ToList();
        return (items, total);
    }

    public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetAllPatients(int page, int pageSize)
    {
        using IDbConnection conn = _factory.CreateConnection();
        int p = Math.Max(page, 1);
        int ps = Math.Max(pageSize, 1);
        const string countSql = "SELECT COUNT(1) FROM PATIENTS";
        const string pageSql = @"SELECT ID AS Id, NAME AS Name, 'NotStarted' AS HandoverStatus, CAST(NULL AS VARCHAR(255)) AS HandoverId FROM (
          SELECT ID, NAME, ROW_NUMBER() OVER (ORDER BY ID) AS RN
          FROM PATIENTS
        ) WHERE RN BETWEEN :startRow AND :endRow";

        int total = conn.ExecuteScalar<int>(countSql);
        int startRow = ((p - 1) * ps) + 1;
        int endRow = p * ps;
        var items = conn.Query<PatientRecord>(pageSql, new { startRow, endRow }).ToList();
        return (items, total);
    }

    public async Task<IReadOnlyList<string>> AssignAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        var assignmentIds = new List<string>();

        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            _logger.LogInformation("🔍 Assignment Debug - Storing assignment for UserId: {UserId}, ShiftId: {ShiftId}, PatientCount: {PatientCount}",
                userId, shiftId, patientIds.Count());

            // Remove existing handovers for this user first (to avoid FK constraint violation)
            await conn.ExecuteAsync(@"
                DELETE FROM HANDOVERS
                WHERE ASSIGNMENT_ID IN (
                    SELECT ASSIGNMENT_ID FROM USER_ASSIGNMENTS WHERE USER_ID = :userId
                )", new { userId });

            // Remove existing assignments for this user
            await conn.ExecuteAsync("DELETE FROM USER_ASSIGNMENTS WHERE USER_ID = :userId",
                new { userId });

            _logger.LogInformation("Removed existing assignments and handovers for user {UserId}", userId);

            // Insert new assignments with explicit ASSIGNMENT_ID
            foreach (var patientId in patientIds)
            {
                var assignmentId = $"assign-{userId}-{shiftId}-{patientId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}";

                await conn.ExecuteAsync(@"
                INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID)
                VALUES (:assignmentId, :userId, :shiftId, :patientId)",
                new { assignmentId, userId, shiftId, patientId });

                assignmentIds.Add(assignmentId);
                _logger.LogDebug("Assigned patient {PatientId} to user {UserId} with assignment {AssignmentId}",
                    patientId, userId, assignmentId);
            }

            _logger.LogInformation("Successfully assigned {Count} patients to user {UserId}", patientIds.Count(), userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign patients to user {UserId}", userId);
            throw;
        }

        return assignmentIds;
    }

    public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(string userId, int page, int pageSize)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            _logger.LogInformation("🔍 Retrieval Debug - Getting patients for UserId: {UserId}, page {Page}, pageSize {PageSize}",
                userId, page, pageSize);

            // Get total count of assigned patients
            const string countSql = "SELECT COUNT(*) FROM USER_ASSIGNMENTS WHERE USER_ID = :userId";
            int total = conn.ExecuteScalar<int>(countSql, new { userId });

            _logger.LogDebug("Found {Total} assigned patients for user {UserId}", total, userId);

            if (total == 0)
                return (Array.Empty<PatientRecord>(), 0);

            // Get assigned patients (ultra simplified)
            const string patientsSql = @"
              SELECT p.ID AS Id, p.NAME AS Name, 'NotStarted' AS HandoverStatus, CAST(NULL AS VARCHAR(255)) AS HandoverId
              FROM PATIENTS p
              INNER JOIN USER_ASSIGNMENTS ua ON p.ID = ua.PATIENT_ID
              WHERE ua.USER_ID = :userId";

            var allPatients = conn.Query<PatientRecord>(patientsSql, new { userId }).ToList();

            // Simple pagination in memory (temporary solution)
            int p = Math.Max(page, 1);
            int ps = Math.Max(pageSize, 1);
            int skip = (p - 1) * ps;
            var patients = allPatients.Skip(skip).Take(ps).ToList();

            _logger.LogDebug("Retrieved {Count} patients from database", patients.Count);

            return (patients, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get patients for user {UserId}", userId);
            throw;
        }
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
          SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME, h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY,
                 h.SITUATION_AWARENESS_DOC_ID, h.SYNTHESIS, h.SHIFT_NAME, h.CREATED_BY, h.ASSIGNED_TO,
                 TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT
          FROM HANDOVERS h
          INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
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
                    AssignmentId: row.ASSIGNMENT_ID,
                    PatientId: row.PATIENT_ID,
                    Status: row.STATUS,
                    IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
                    PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
                    ActionItems: actionItems,
                    ShiftName: row.SHIFT_NAME ?? "Unknown",
                    CreatedBy: row.CREATED_BY ?? "system",
                    AssignedTo: row.ASSIGNED_TO ?? "system",
                    PatientName: row.PATIENT_NAME,
                    SituationAwarenessDocId: row.SITUATION_AWARENESS_DOC_ID,
                    Synthesis: string.IsNullOrEmpty(row.SYNTHESIS) ? null : new HandoverSynthesis(row.SYNTHESIS),
                    CreatedAt: row.CREATED_AT
                );

            handovers.Add(handover);
        }

        return (handovers, total);
    }

    public async Task CreateHandoverForAssignmentAsync(string assignmentId, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            // Get assignment details
            var assignment = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT USER_ID, SHIFT_ID, PATIENT_ID FROM USER_ASSIGNMENTS WHERE ASSIGNMENT_ID = :assignmentId",
                new { assignmentId });

            if (assignment == null)
            {
                throw new ArgumentException($"Assignment {assignmentId} not found");
            }

            // Get shift name
            var shiftName = await conn.ExecuteScalarAsync<string>(
                "SELECT NAME FROM SHIFTS WHERE ID = :shiftId",
                new { shiftId = assignment.SHIFT_ID });

            // Generate handover ID (must be <= 50 chars)
            var timestamp = DateTime.Now.ToString("yyMMddHHmm");
            var randomPart = new Random().Next(1000, 9999);
            var handoverId = $"hvo-{timestamp}-{randomPart}";

            // Create handover
            await conn.ExecuteAsync(@"
            INSERT INTO HANDOVERS (
                ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY,
                PATIENT_SUMMARY, SHIFT_NAME, CREATED_BY, ASSIGNED_TO
            ) VALUES (
                :handoverId, :assignmentId, :patientId, 'Active', 'Stable',
                'Handover iniciado - información pendiente de completar', :shiftName, :userId, :userId
            )", new {
                handoverId,
                assignmentId,
                patientId = assignment.PATIENT_ID,
                shiftName = shiftName ?? "Unknown",
                userId
            });

            _logger.LogInformation("Created handover {HandoverId} for assignment {AssignmentId}", handoverId, assignmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create handover for assignment {AssignmentId}", assignmentId);
            throw;
        }
    }

    public (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetPatientHandovers(string patientId, int page, int pageSize)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            // Get total count
            const string countSql = "SELECT COUNT(1) FROM HANDOVERS WHERE PATIENT_ID = :patientId";
            int total = conn.ExecuteScalar<int>(countSql, new { patientId });

            // Get handovers with pagination
            int p = Math.Max(page, 1);
            int ps = Math.Max(pageSize, 1);
            int offset = (p - 1) * ps;

            const string handoverSql = @"
              SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME, h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY,
                     h.SITUATION_AWARENESS_DOC_ID, h.SYNTHESIS, h.SHIFT_NAME, h.CREATED_BY, h.ASSIGNED_TO,
                     TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT
              FROM HANDOVERS h
              LEFT JOIN PATIENTS p ON h.PATIENT_ID = p.ID
              WHERE h.PATIENT_ID = :patientId
              ORDER BY h.CREATED_AT DESC";

            var handoverRows = conn.Query(handoverSql, new { patientId }).ToList();

            // Get action items for each handover
            var handovers = new List<HandoverRecord>();

            foreach (var row in handoverRows)
            {
                const string actionItemsSql = @"
                SELECT ID, DESCRIPTION, IS_COMPLETED
                FROM HANDOVER_ACTION_ITEMS
                WHERE HANDOVER_ID = :handoverId
                ORDER BY CREATED_AT";

                var actionItems = conn.Query(actionItemsSql, new { handoverId = row.ID })
                    .Select(item => new HandoverActionItem(item.ID, item.DESCRIPTION, item.IS_COMPLETED == 1))
                    .ToList();

                var handover = new HandoverRecord(
                    Id: row.ID,
                    AssignmentId: row.ASSIGNMENT_ID,
                    PatientId: row.PATIENT_ID,
                    Status: row.STATUS,
                    IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
                    PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
                    ActionItems: actionItems,
                    ShiftName: row.SHIFT_NAME ?? "Unknown",
                    CreatedBy: row.CREATED_BY ?? "system",
                    AssignedTo: row.ASSIGNED_TO ?? "system",
                    PatientName: row.PATIENT_NAME,
                    SituationAwarenessDocId: row.SITUATION_AWARENESS_DOC_ID,
                    Synthesis: string.IsNullOrEmpty(row.SYNTHESIS) ? null : new HandoverSynthesis(row.SYNTHESIS),
                    CreatedAt: row.CREATED_AT
                );

                handovers.Add(handover);
            }

            return (handovers, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get handovers for patient {PatientId}", patientId);
            throw;
        }
    }

    public HandoverRecord? GetHandoverById(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string handoverSql = @"
              SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME, h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY,
                     h.SITUATION_AWARENESS_DOC_ID, h.SYNTHESIS, h.SHIFT_NAME, h.CREATED_BY, h.ASSIGNED_TO,
                     TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT
              FROM HANDOVERS h
              LEFT JOIN PATIENTS p ON h.PATIENT_ID = p.ID
              WHERE h.ID = :handoverId";

            var row = conn.QueryFirstOrDefault(handoverSql, new { handoverId });

            if (row == null)
            {
                return null;
            }

            // Get action items for the handover
            const string actionItemsSql = @"
            SELECT ID, DESCRIPTION, IS_COMPLETED
            FROM HANDOVER_ACTION_ITEMS
            WHERE HANDOVER_ID = :handoverId
            ORDER BY CREATED_AT";

            var actionItems = conn.Query(actionItemsSql, new { handoverId })
                .Select(item => new HandoverActionItem(item.ID, item.DESCRIPTION, item.IS_COMPLETED == 1))
                .ToList();

            return new HandoverRecord(
                Id: row.ID,
                AssignmentId: row.ASSIGNMENT_ID,
                PatientId: row.PATIENT_ID,
                PatientName: row.PATIENT_NAME,
                Status: row.STATUS,
                IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
                PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
                ActionItems: actionItems,
                SituationAwarenessDocId: row.SITUATION_AWARENESS_DOC_ID,
                Synthesis: !string.IsNullOrEmpty(row.SYNTHESIS) ? new HandoverSynthesis(row.SYNTHESIS) : null,
                ShiftName: row.SHIFT_NAME ?? "Unknown",
                CreatedBy: row.CREATED_BY ?? "system",
                AssignedTo: row.ASSIGNED_TO ?? "system",
                CreatedAt: row.CREATED_AT
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get handover {HandoverId}", handoverId);
            throw;
        }
    }

    public PatientDetailRecord? GetPatientById(string patientId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string patientSql = @"
              SELECT p.ID, p.NAME, p.DATE_OF_BIRTH, p.GENDER, p.ADMISSION_DATE,
                     p.UNIT_ID, p.ROOM_NUMBER, p.DIAGNOSIS, p.ALLERGIES, p.MEDICATIONS, p.NOTES
              FROM PATIENTS p
              WHERE p.ID = :patientId";

            var row = conn.QueryFirstOrDefault(patientSql, new { patientId });

            if (row == null)
            {
                return null;
            }

            // Parse allergies and medications (assuming they are stored as comma-separated strings)
            var allergies = ParseCommaSeparatedString(row.ALLERGIES);
            var medications = ParseCommaSeparatedString(row.MEDICATIONS);

            return new PatientDetailRecord(
                Id: row.ID,
                Name: row.NAME ?? "Unknown",
                Mrn: GenerateMrn(row.ID), // Generate MRN from patient ID since it's not in the database
                Dob: row.DATE_OF_BIRTH?.ToString("yyyy-MM-dd") ?? "",
                Gender: row.GENDER ?? "Unknown",
                AdmissionDate: row.ADMISSION_DATE?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                CurrentUnit: row.UNIT_ID ?? "",
                RoomNumber: row.ROOM_NUMBER ?? "",
                Diagnosis: row.DIAGNOSIS ?? "",
                Allergies: allergies,
                Medications: medications,
                Notes: row.NOTES ?? ""
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get patient {PatientId}", patientId);
            throw;
        }
    }

    private string GenerateMrn(string patientId)
    {
        // Generate a simple MRN based on patient ID
        // For example: pat-001 becomes MRN001
        var numberPart = patientId.Split('-').LastOrDefault() ?? "000";
        return $"MRN{numberPart.PadLeft(3, '0')}";
    }

    private List<string> ParseCommaSeparatedString(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new List<string>();
        }

        return value.Split(',')
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrEmpty(item))
            .ToList();
    }
}
