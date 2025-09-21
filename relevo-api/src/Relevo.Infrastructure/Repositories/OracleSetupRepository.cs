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
            // Ensure the user exists in the database before assigning patients
            EnsureUserExists(userId, null, null, null, null);

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

        // Get handovers with pagination using a JOIN approach
        int p = Math.Max(page, 1);
        int ps = Math.Max(pageSize, 1);
        int offset = (p - 1) * ps;

        // Get total count
        const string countSql = @"
            SELECT COUNT(1)
            FROM HANDOVERS h
            INNER JOIN USER_ASSIGNMENTS ua ON h.PATIENT_ID = ua.PATIENT_ID
            WHERE ua.USER_ID = :userId";

        int total = conn.ExecuteScalar<int>(countSql, new { userId });

        if (total == 0)
            return (Array.Empty<HandoverRecord>(), 0);

        // Get handovers with pagination
        const string handoverSql = @"
          SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME, h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY,
                 h.SITUATION_AWARENESS_DOC_ID, h.SYNTHESIS, h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO,
                 TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
                 TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as READY_AT,
                 TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as STARTED_AT,
                 TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACCEPTED_AT,
                 TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as COMPLETED_AT,
                 TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CANCELLED_AT,
                 TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as REJECTED_AT,
                 h.REJECTION_REASON,
                 TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as EXPIRED_AT,
                 h.HANDOVER_TYPE,
                 vws.StateName,
                 h.HANDOVER_WINDOW_DATE,
                 h.FROM_SHIFT_ID,
                 h.TO_SHIFT_ID,
                 h.TO_DOCTOR_ID
          FROM HANDOVERS h
          INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
          INNER JOIN USER_ASSIGNMENTS ua ON h.PATIENT_ID = ua.PATIENT_ID
          LEFT JOIN VW_HANDOVERS_STATE vws ON h.ID = vws.HandoverId
          WHERE ua.USER_ID = :userId
          ORDER BY h.CREATED_AT DESC
          OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY";

        var handoverRows = conn.Query(handoverSql, new { userId, offset, pageSize }).ToList();

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
                    CreatedAt: row.CREATED_AT,
                    ReadyAt: row.READY_AT,
                    StartedAt: row.STARTED_AT,
                    AcceptedAt: row.ACCEPTED_AT,
                    CompletedAt: row.COMPLETED_AT,
                    CancelledAt: row.CANCELLED_AT,
                    RejectedAt: row.REJECTED_AT,
                    RejectionReason: row.REJECTION_REASON,
                    ExpiredAt: row.EXPIRED_AT,
                    HandoverType: row.HANDOVER_TYPE,
                    HandoverWindowDate: row.HANDOVER_WINDOW_DATE,
                    FromShiftId: row.FROM_SHIFT_ID,
                    ToShiftId: row.TO_SHIFT_ID,
                    ToDoctorId: row.TO_DOCTOR_ID,
                    StateName: row.STATENAME ?? "Draft"
                );

            handovers.Add(handover);
        }

        return (handovers, total);
    }

    public void EnsureUserExists(string userId, string? email, string? firstName, string? lastName, string? fullName)
    {
        using IDbConnection conn = _factory.CreateConnection();

        // Check if user already exists
        var existingUser = conn.QueryFirstOrDefault("SELECT ID FROM USERS WHERE ID = :userId", new { userId });

        if (existingUser != null)
        {
            // User already exists, nothing to do
            return;
        }

        // Create the user with basic info
        // Use a default email if not provided (required by schema)
        var defaultEmail = email ?? $"{userId}@clerk.local";
        conn.Execute(@"
            INSERT INTO USERS (ID, EMAIL, FIRST_NAME, LAST_NAME, FULL_NAME, ROLE, IS_ACTIVE, CREATED_AT, UPDATED_AT)
            VALUES (:userId, :defaultEmail, :firstName, :lastName, :fullName, 'doctor', 1, SYSTIMESTAMP, SYSTIMESTAMP)",
            new { userId, defaultEmail, firstName, lastName, fullName });
    }

    public async Task CreateHandoverForAssignmentAsync(string assignmentId, string userId, DateTime windowDate, string fromShiftId, string toShiftId)
    {
        try
        {
            // Ensure the user exists in the database before creating handovers
            EnsureUserExists(userId, null, null, null, null);

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

            var fromShiftName = (await conn.QueryFirstOrDefaultAsync<ShiftRecord>("SELECT ID AS Id, NAME AS Name FROM SHIFTS WHERE ID = :fromShiftId", new { fromShiftId }))?.Name ?? "Unknown";
            var toShiftName = (await conn.QueryFirstOrDefaultAsync<ShiftRecord>("SELECT ID AS Id, NAME AS Name FROM SHIFTS WHERE ID = :toShiftId", new { toShiftId }))?.Name ?? "Unknown";

            // Create handover
            await conn.ExecuteAsync(@"
            INSERT INTO HANDOVERS (
                ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY,
                PATIENT_SUMMARY, SHIFT_NAME, CREATED_BY, FROM_DOCTOR_ID,
                HANDOVER_WINDOW_DATE, FROM_SHIFT_ID, TO_SHIFT_ID
            ) VALUES (
                :handoverId, :assignmentId, :patientId, 'Draft', 'Stable',
                'Handover iniciado - información pendiente de completar', :shiftName, :userId, :userId,
                :windowDate, :fromShiftId, :toShiftId
            )", new {
                handoverId,
                assignmentId,
                patientId = assignment.PATIENT_ID,
                shiftName = $"{fromShiftName} → {toShiftName}",
                userId,
                windowDate,
                fromShiftId,
                toShiftId
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
                     h.SITUATION_AWARENESS_DOC_ID, h.SYNTHESIS, h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO,
                     TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
                     TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as READY_AT,
                     TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as STARTED_AT,
                     TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACCEPTED_AT,
                     TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as COMPLETED_AT,
                     TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CANCELLED_AT,
                     TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as REJECTED_AT,
                     h.REJECTION_REASON,
                     TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as EXPIRED_AT,
                     h.HANDOVER_TYPE,
                     vws.StateName,
                     h.HANDOVER_WINDOW_DATE,
                     h.FROM_SHIFT_ID,
                     h.TO_SHIFT_ID,
                     h.TO_DOCTOR_ID
              FROM HANDOVERS h
              LEFT JOIN PATIENTS p ON h.PATIENT_ID = p.ID
              LEFT JOIN VW_HANDOVERS_STATE vws ON h.ID = vws.HandoverId
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
                    CreatedAt: row.CREATED_AT,
                    ReadyAt: row.READY_AT,
                    StartedAt: row.STARTED_AT,
                    AcceptedAt: row.ACCEPTED_AT,
                    CompletedAt: row.COMPLETED_AT,
                    CancelledAt: row.CANCELLED_AT,
                    RejectedAt: row.REJECTED_AT,
                    RejectionReason: row.REJECTION_REASON,
                    ExpiredAt: row.EXPIRED_AT,
                    HandoverType: row.HANDOVER_TYPE,
                    HandoverWindowDate: row.HANDOVER_WINDOW_DATE,
                    FromShiftId: row.FROM_SHIFT_ID,
                    ToShiftId: row.TO_SHIFT_ID,
                    ToDoctorId: row.TO_DOCTOR_ID,
                    StateName: row.STATENAME ?? "Draft"
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
                     h.SITUATION_AWARENESS_DOC_ID, h.SYNTHESIS, h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO,
                     TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
                     TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as READY_AT,
                     TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as STARTED_AT,
                     TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACCEPTED_AT,
                     TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as COMPLETED_AT,
                     TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CANCELLED_AT,
                     TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as REJECTED_AT,
                     h.REJECTION_REASON,
                     TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as EXPIRED_AT,
                     h.HANDOVER_TYPE,
                     vws.StateName,
                     h.HANDOVER_WINDOW_DATE,
                     h.FROM_SHIFT_ID,
                     h.TO_SHIFT_ID,
                     h.TO_DOCTOR_ID
              FROM HANDOVERS h
              LEFT JOIN PATIENTS p ON h.PATIENT_ID = p.ID
              LEFT JOIN VW_HANDOVERS_STATE vws ON h.ID = vws.HandoverId
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
                CreatedAt: row.CREATED_AT,
                ReadyAt: row.READY_AT,
                StartedAt: row.STARTED_AT,
                AcceptedAt: row.ACCEPTED_AT,
                CompletedAt: row.COMPLETED_AT,
                CancelledAt: row.CANCELLED_AT,
                RejectedAt: row.REJECTED_AT,
                RejectionReason: row.REJECTION_REASON,
                ExpiredAt: row.EXPIRED_AT,
                HandoverType: row.HANDOVER_TYPE,
                HandoverWindowDate: row.HANDOVER_WINDOW_DATE,
                FromShiftId: row.FROM_SHIFT_ID,
                ToShiftId: row.TO_SHIFT_ID,
                ToDoctorId: row.TO_DOCTOR_ID,
                StateName: row.STATENAME ?? "Draft"
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


    public IReadOnlyList<HandoverParticipantRecord> GetHandoverParticipants(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                SELECT ID, HANDOVER_ID as HandoverId, USER_ID as UserId, USER_NAME as UserName, USER_ROLE as UserRole, STATUS,
                       JOINED_AT as JoinedAt, LAST_ACTIVITY as LastActivity
                FROM HANDOVER_PARTICIPANTS
                WHERE HANDOVER_ID = :handoverId
                ORDER BY JOINED_AT";

            var participants = conn.Query<HandoverParticipantRecord>(sql, new { handoverId }).ToList();

            // If no participants found, return a default list with the assigned user
            if (!participants.Any())
            {
                // Get the handover creator as the default participant
                const string creatorSql = @"
                    SELECT TO_DOCTOR_ID as USER_ID, 'Assigned Physician' as USER_NAME, 'Doctor' as USER_ROLE
                    FROM HANDOVERS
                    WHERE ID = :handoverId";

                var creator = conn.QueryFirstOrDefault(creatorSql, new { handoverId });

                if (creator != null)
                {
                    participants.Add(new HandoverParticipantRecord(
                        Id: $"participant-{handoverId}-default",
                        HandoverId: handoverId,
                        UserId: creator.USER_ID,
                        UserName: creator.USER_NAME ?? "Assigned Physician",
                        UserRole: creator.USER_ROLE,
                        Status: "active",
                        JoinedAt: DateTime.Now,
                        LastActivity: DateTime.Now
                    ));
                }
            }

            return participants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get participants for handover {HandoverId}", handoverId);
            return Array.Empty<HandoverParticipantRecord>();
        }
    }

    public IReadOnlyList<HandoverSectionRecord> GetHandoverSections(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                SELECT ID, HANDOVER_ID as HandoverId, SECTION_TYPE as SectionType, CONTENT, STATUS, LAST_EDITED_BY as LastEditedBy,
                       CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
                FROM HANDOVER_SECTIONS
                WHERE HANDOVER_ID = :handoverId
                ORDER BY CREATED_AT";

            var sections = conn.Query<HandoverSectionRecord>(sql, new { handoverId }).ToList();

            // If no sections exist, create default empty sections
            if (!sections.Any())
            {
                sections = new List<HandoverSectionRecord>
                {
                    new HandoverSectionRecord(
                        Id: $"section-{handoverId}-severity",
                        HandoverId: handoverId,
                        SectionType: "illness_severity",
                        Content: "Stable",
                        Status: "draft",
                        LastEditedBy: null,
                        CreatedAt: DateTime.Now,
                        UpdatedAt: DateTime.Now
                    ),
                    new HandoverSectionRecord(
                        Id: $"section-{handoverId}-summary",
                        HandoverId: handoverId,
                        SectionType: "patient_summary",
                        Content: "",
                        Status: "draft",
                        LastEditedBy: null,
                        CreatedAt: DateTime.Now,
                        UpdatedAt: DateTime.Now
                    ),
                    new HandoverSectionRecord(
                        Id: $"section-{handoverId}-actions",
                        HandoverId: handoverId,
                        SectionType: "action_items",
                        Content: "",
                        Status: "draft",
                        LastEditedBy: null,
                        CreatedAt: DateTime.Now,
                        UpdatedAt: DateTime.Now
                    ),
                    new HandoverSectionRecord(
                        Id: $"section-{handoverId}-awareness",
                        HandoverId: handoverId,
                        SectionType: "situation_awareness",
                        Content: "",
                        Status: "draft",
                        LastEditedBy: null,
                        CreatedAt: DateTime.Now,
                        UpdatedAt: DateTime.Now
                    ),
                    new HandoverSectionRecord(
                        Id: $"section-{handoverId}-synthesis",
                        HandoverId: handoverId,
                        SectionType: "synthesis",
                        Content: "",
                        Status: "draft",
                        LastEditedBy: null,
                        CreatedAt: DateTime.Now,
                        UpdatedAt: DateTime.Now
                    )
                };
            }

            return sections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sections for handover {HandoverId}", handoverId);
            return Array.Empty<HandoverSectionRecord>();
        }
    }

    public HandoverSyncStatusRecord? GetHandoverSyncStatus(string handoverId, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                SELECT ID, HANDOVER_ID, USER_ID, SYNC_STATUS, LAST_SYNC, VERSION
                FROM HANDOVER_SYNC_STATUS
                WHERE HANDOVER_ID = :handoverId AND USER_ID = :userId";

            var syncStatus = conn.QueryFirstOrDefault<HandoverSyncStatusRecord>(sql, new { handoverId, userId });

            // If no sync status exists, create a default one
            if (syncStatus == null)
            {
                syncStatus = new HandoverSyncStatusRecord(
                    Id: $"sync-{handoverId}-{userId}",
                    HandoverId: handoverId,
                    UserId: userId,
                    SyncStatus: "synced",
                    LastSync: DateTime.Now,
                    Version: 1
                );
            }

            return syncStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sync status for handover {HandoverId}, user {UserId}", handoverId, userId);
            return null;
        }
    }

    public bool UpdateHandoverSection(string handoverId, string sectionId, string content, string status, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            // First check if the section exists
            const string checkSql = "SELECT COUNT(1) FROM HANDOVER_SECTIONS WHERE ID = :sectionId AND HANDOVER_ID = :handoverId";
            var exists = conn.ExecuteScalar<int>(checkSql, new { sectionId, handoverId }) > 0;

            if (exists)
            {
                // Update existing section
                const string updateSql = @"
                    UPDATE HANDOVER_SECTIONS
                    SET CONTENT = :content, STATUS = :status, LAST_EDITED_BY = :userId, UPDATED_AT = SYSTIMESTAMP
                    WHERE ID = :sectionId AND HANDOVER_ID = :handoverId";

                var rowsAffected = conn.Execute(updateSql, new { sectionId, handoverId, content, status, userId });
                return rowsAffected > 0;
            }
            else
            {
                // Insert new section
                const string insertSql = @"
                    INSERT INTO HANDOVER_SECTIONS (ID, HANDOVER_ID, SECTION_TYPE, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT)
                    VALUES (:sectionId, :handoverId, :sectionType, :content, :status, :userId, SYSTIMESTAMP, SYSTIMESTAMP)";

                // Extract section type from sectionId (e.g., "section-h1-severity" -> "illness_severity")
                var sectionType = sectionId.Contains("severity") ? "illness_severity" :
                                 sectionId.Contains("summary") ? "patient_summary" :
                                 sectionId.Contains("actions") ? "action_items" :
                                 sectionId.Contains("awareness") ? "situation_awareness" :
                                 "synthesis";

                var rowsAffected = conn.Execute(insertSql, new { sectionId, handoverId, sectionType, content, status, userId });
                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update handover section {SectionId}", sectionId);
            throw;
        }
    }


    public UserPreferencesRecord? GetUserPreferences(string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                SELECT ID, USER_ID, THEME, LANGUAGE, TIMEZONE, NOTIFICATIONS_ENABLED, AUTO_SAVE_ENABLED,
                       CREATED_AT, UPDATED_AT
                FROM USER_PREFERENCES
                WHERE USER_ID = :userId";

            var row = conn.QueryFirstOrDefault(sql, new { userId });

            if (row == null)
            {
                return null;
            }

            return new UserPreferencesRecord(
                Id: row.ID,
                UserId: row.USER_ID,
                Theme: row.THEME ?? "light",
                Language: row.LANGUAGE ?? "en",
                Timezone: row.TIMEZONE ?? "UTC",
                NotificationsEnabled: row.NOTIFICATIONS_ENABLED == 1,
                AutoSaveEnabled: row.AUTO_SAVE_ENABLED == 1,
                CreatedAt: row.CREATED_AT,
                UpdatedAt: row.UPDATED_AT
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user preferences for user {UserId}", userId);
            return null;
        }
    }

    public IReadOnlyList<UserSessionRecord> GetUserSessions(string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                SELECT ID, USER_ID, SESSION_START, SESSION_END, IP_ADDRESS, USER_AGENT, IS_ACTIVE
                FROM USER_SESSIONS
                WHERE USER_ID = :userId
                ORDER BY SESSION_START DESC";

            var sessions = conn.Query<UserSessionRecord>(sql, new { userId }).ToList();

            return sessions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user sessions for user {UserId}", userId);
            return Array.Empty<UserSessionRecord>();
        }
    }

    public bool UpdateUserPreferences(string userId, UserPreferencesRecord preferences)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            // Check if preferences exist
            const string checkSql = "SELECT COUNT(1) FROM USER_PREFERENCES WHERE USER_ID = :userId";
            var exists = conn.ExecuteScalar<int>(checkSql, new { userId }) > 0;

            if (exists)
            {
                // Update existing preferences
                const string updateSql = @"
                    UPDATE USER_PREFERENCES
                    SET THEME = :theme, LANGUAGE = :language, TIMEZONE = :timezone,
                        NOTIFICATIONS_ENABLED = :notificationsEnabled, AUTO_SAVE_ENABLED = :autoSaveEnabled,
                        UPDATED_AT = SYSTIMESTAMP
                    WHERE USER_ID = :userId";

                var rowsAffected = conn.Execute(updateSql, new
                {
                    userId,
                    theme = preferences.Theme,
                    language = preferences.Language,
                    timezone = preferences.Timezone,
                    notificationsEnabled = preferences.NotificationsEnabled ? 1 : 0,
                    autoSaveEnabled = preferences.AutoSaveEnabled ? 1 : 0
                });

                return rowsAffected > 0;
            }
            else
            {
                // Insert new preferences
                const string insertSql = @"
                    INSERT INTO USER_PREFERENCES (ID, USER_ID, THEME, LANGUAGE, TIMEZONE, NOTIFICATIONS_ENABLED, AUTO_SAVE_ENABLED, CREATED_AT, UPDATED_AT)
                    VALUES (:id, :userId, :theme, :language, :timezone, :notificationsEnabled, :autoSaveEnabled, SYSTIMESTAMP, SYSTIMESTAMP)";

                var rowsAffected = conn.Execute(insertSql, new
                {
                    id = preferences.Id,
                    userId,
                    theme = preferences.Theme,
                    language = preferences.Language,
                    timezone = preferences.Timezone,
                    notificationsEnabled = preferences.NotificationsEnabled ? 1 : 0,
                    autoSaveEnabled = preferences.AutoSaveEnabled ? 1 : 0
                });

                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user preferences for user {UserId}", userId);
            throw;
        }
    }

    // Handover Messages
    public IReadOnlyList<HandoverMessageRecord> GetHandoverMessages(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT hm.ID, hm.HANDOVER_ID as HandoverId, hm.USER_ID as UserId,
                       u.FIRST_NAME || ' ' || u.LAST_NAME as UserName,
                       hm.MESSAGE_TEXT as MessageText, hm.MESSAGE_TYPE as MessageType,
                       hm.CREATED_AT as CreatedAt, hm.UPDATED_AT as UpdatedAt
                FROM HANDOVER_MESSAGES hm
                INNER JOIN USERS u ON hm.USER_ID = u.ID
                WHERE hm.HANDOVER_ID = :handoverId
                ORDER BY hm.CREATED_AT ASC";

            return conn.Query<HandoverMessageRecord>(sql, new { handoverId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get handover messages for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public HandoverMessageRecord CreateHandoverMessage(string handoverId, string userId, string userName, string messageText, string messageType)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            var id = Guid.NewGuid().ToString();

            const string sql = @"
                INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, USER_NAME, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT, UPDATED_AT)
                VALUES (:id, :handoverId, :userId, :userName, :messageText, :messageType, SYSTIMESTAMP, SYSTIMESTAMP)
                RETURNING ID, HANDOVER_ID as HandoverId, USER_ID as UserId, USER_NAME as UserName,
                         MESSAGE_TEXT as MessageText, MESSAGE_TYPE as MessageType,
                         CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt INTO :newRecord";

            var parameters = new
            {
                id,
                handoverId,
                userId,
                userName,
                messageText,
                messageType
            };

            conn.Execute(sql, parameters);
            return new HandoverMessageRecord(id, handoverId, userId, userName, messageText, messageType, DateTime.Now, DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create handover message for handover {HandoverId}", handoverId);
            throw;
        }
    }

    // Handover Activity Log
    public IReadOnlyList<HandoverActivityItemRecord> GetHandoverActivityLog(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT hal.ID, hal.HANDOVER_ID as HandoverId, hal.USER_ID as UserId,
                       u.FIRST_NAME || ' ' || u.LAST_NAME as UserName,
                       hal.ACTIVITY_TYPE as ActivityType, hal.ACTIVITY_DESCRIPTION as ActivityDescription,
                       hal.SECTION_AFFECTED as SectionAffected, hal.METADATA,
                       hal.CREATED_AT as CreatedAt
                FROM HANDOVER_ACTIVITY_LOG hal
                INNER JOIN USERS u ON hal.USER_ID = u.ID
                WHERE hal.HANDOVER_ID = :handoverId
                ORDER BY hal.CREATED_AT DESC";

            return conn.Query<HandoverActivityItemRecord>(sql, new { handoverId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get handover activity log for handover {HandoverId}", handoverId);
            throw;
        }
    }

    // Handover Checklists
    public IReadOnlyList<HandoverChecklistItemRecord> GetHandoverChecklists(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT ID, HANDOVER_ID as HandoverId, USER_ID as UserId, ITEM_ID as ItemId,
                       ITEM_CATEGORY as ItemCategory, ITEM_LABEL as ItemLabel,
                       ITEM_DESCRIPTION as ItemDescription, IS_REQUIRED as IsRequired,
                       IS_CHECKED as IsChecked, CHECKED_AT as CheckedAt, CREATED_AT as CreatedAt
                FROM HANDOVER_CHECKLISTS
                WHERE HANDOVER_ID = :handoverId
                ORDER BY CREATED_AT ASC";

            return conn.Query<HandoverChecklistItemRecord>(sql, new { handoverId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get handover checklists for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public bool UpdateChecklistItem(string handoverId, string itemId, bool isChecked, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE HANDOVER_CHECKLISTS
                SET IS_CHECKED = :isChecked,
                    CHECKED_AT = CASE WHEN :isChecked = 1 THEN SYSTIMESTAMP ELSE NULL END,
                    UPDATED_AT = SYSTIMESTAMP
                WHERE HANDOVER_ID = :handoverId AND ITEM_ID = :itemId AND USER_ID = :userId";

            var result = conn.Execute(sql, new { handoverId, itemId, isChecked = isChecked ? 1 : 0, userId });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update checklist item {ItemId} for handover {HandoverId}", itemId, handoverId);
            throw;
        }
    }

    // Handover Contingency Plans
    public IReadOnlyList<HandoverContingencyPlanRecord> GetHandoverContingencyPlans(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT ID, HANDOVER_ID as HandoverId, CONDITION_TEXT as ConditionText,
                       ACTION_TEXT as ActionText, PRIORITY, STATUS, CREATED_BY as CreatedBy,
                       CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
                FROM HANDOVER_CONTINGENCY
                WHERE HANDOVER_ID = :handoverId
                ORDER BY CREATED_AT ASC";

            return conn.Query<HandoverContingencyPlanRecord>(sql, new { handoverId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get handover contingency plans for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public HandoverContingencyPlanRecord CreateContingencyPlan(string handoverId, string conditionText, string actionText, string priority, string createdBy)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            var id = Guid.NewGuid().ToString();

            const string sql = @"
                INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT, UPDATED_AT)
                VALUES (:id, :handoverId, :conditionText, :actionText, :priority, 'active', :createdBy, SYSTIMESTAMP, SYSTIMESTAMP)";

            conn.Execute(sql, new { id, handoverId, conditionText, actionText, priority, createdBy });

            return new HandoverContingencyPlanRecord(
                id, handoverId, conditionText, actionText, priority, "active",
                createdBy, DateTime.Now, DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create contingency plan for handover {HandoverId}", handoverId);
            throw;
        }
    }

    // Action Items
    public IReadOnlyList<HandoverActionItemRecord> GetHandoverActionItems(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT ID, HANDOVER_ID as HandoverId, DESCRIPTION, IS_COMPLETED as IsCompleted,
                       CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt, COMPLETED_AT as CompletedAt
                FROM HANDOVER_ACTION_ITEMS
                WHERE HANDOVER_ID = :handoverId
                ORDER BY CREATED_AT DESC";

            return conn.Query<HandoverActionItemRecord>(sql, new { handoverId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get action items for handover {HandoverId}", handoverId);
            return Array.Empty<HandoverActionItemRecord>();
        }
    }

    public string CreateHandoverActionItem(string handoverId, string description, string priority)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            var id = $"action-{Guid.NewGuid().ToString().Substring(0, 8)}";

            const string sql = @"
                INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED, CREATED_AT, UPDATED_AT)
                VALUES (:id, :handoverId, :description, 0, SYSTIMESTAMP, SYSTIMESTAMP)";

            conn.Execute(sql, new { id, handoverId, description });
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create action item for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public bool UpdateHandoverActionItem(string handoverId, string itemId, bool isCompleted)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                UPDATE HANDOVER_ACTION_ITEMS
                SET IS_COMPLETED = :isCompleted,
                    COMPLETED_AT = CASE WHEN :isCompleted = 1 THEN SYSTIMESTAMP ELSE NULL END,
                    UPDATED_AT = SYSTIMESTAMP
                WHERE ID = :itemId AND HANDOVER_ID = :handoverId";

            var result = conn.Execute(sql, new { itemId, handoverId, isCompleted = isCompleted ? 1 : 0 });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update action item {ItemId} for handover {HandoverId}", itemId, handoverId);
            throw;
        }
    }

    public bool DeleteHandoverActionItem(string handoverId, string itemId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"DELETE FROM HANDOVER_ACTION_ITEMS WHERE ID = :itemId AND HANDOVER_ID = :handoverId";

            var result = conn.Execute(sql, new { itemId, handoverId });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete action item {ItemId} for handover {HandoverId}", itemId, handoverId);
            throw;
        }
    }

    public async Task<bool> StartHandover(string handoverId, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            // Optional: Check if handover is in a 'Ready' state before starting
            var handover = GetHandoverById(handoverId);
            if (handover?.StateName != "Ready")
            {
                _logger.LogWarning("Handover {HandoverId} cannot be started because it is not in 'Ready' state. Current state: {State}", handoverId, handover?.StateName);
                // return false; // Or handle as needed
            }

            const string sql = @"
                UPDATE HANDOVERS
                SET STARTED_AT = SYSTIMESTAMP,
                    STATUS = 'InProgress',
                    UPDATED_AT = SYSTIMESTAMP
                WHERE ID = :handoverId AND STARTED_AT IS NULL";

            var result = await conn.ExecuteAsync(sql, new { handoverId });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<bool> ReadyHandover(string handoverId, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE HANDOVERS
                SET READY_AT = SYSTIMESTAMP,
                    UPDATED_AT = SYSTIMESTAMP
                WHERE ID = :handoverId AND READY_AT IS NULL";
            var result = await conn.ExecuteAsync(sql, new { handoverId });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set handover {HandoverId} to ready", handoverId);
            throw;
        }
    }

    public async Task<bool> AcceptHandover(string handoverId, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE HANDOVERS
                SET ACCEPTED_AT = SYSTIMESTAMP,
                    UPDATED_AT = SYSTIMESTAMP
                WHERE ID = :handoverId AND ACCEPTED_AT IS NULL";
            var result = await conn.ExecuteAsync(sql, new { handoverId });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to accept handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<bool> CompleteHandover(string handoverId, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE HANDOVERS
                SET COMPLETED_AT = SYSTIMESTAMP,
                    STATUS = 'Completed',
                    UPDATED_AT = SYSTIMESTAMP,
                    COMPLETED_BY = :userId
                WHERE ID = :handoverId AND COMPLETED_AT IS NULL";
            var result = await conn.ExecuteAsync(sql, new { handoverId, userId });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<bool> CancelHandover(string handoverId, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE HANDOVERS
                SET CANCELLED_AT = SYSTIMESTAMP,
                    STATUS = 'Cancelled',
                    UPDATED_AT = SYSTIMESTAMP
                WHERE ID = :handoverId AND CANCELLED_AT IS NULL AND ACCEPTED_AT IS NULL";
            var result = await conn.ExecuteAsync(sql, new { handoverId });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<bool> RejectHandover(string handoverId, string userId, string reason)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE HANDOVERS
                SET REJECTED_AT = SYSTIMESTAMP,
                    REJECTION_REASON = :reason,
                    STATUS = 'Rejected',
                    UPDATED_AT = SYSTIMESTAMP
                WHERE ID = :handoverId AND REJECTED_AT IS NULL AND ACCEPTED_AT IS NULL";
            var result = await conn.ExecuteAsync(sql, new { handoverId, reason });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject handover {HandoverId}", handoverId);
            throw;
        }
    }
}
