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

        // Count all patients in unit (assignment status tracked via handover state)
        const string countSql = @"
            SELECT COUNT(1) FROM PATIENTS p
            WHERE p.UNIT_ID = :unitId";

        // Get all patients in unit (assignment status tracked via handover state)
        const string pageSql = @"SELECT p.ID AS Id, p.NAME AS Name, 'NotStarted' AS HandoverStatus, CAST(NULL AS VARCHAR(255)) AS HandoverId,
          FLOOR((SYSDATE - p.DATE_OF_BIRTH)/365.25) AS Age, p.ROOM_NUMBER AS Room, p.DIAGNOSIS AS Diagnosis,
          CASE
            WHEN h.STATUS = 'Completed' AND h.COMPLETED_AT IS NOT NULL THEN 'Completed'
            WHEN h.CANCELLED_AT IS NOT NULL THEN 'Cancelled'
            WHEN h.REJECTED_AT IS NOT NULL THEN 'Rejected'
            WHEN h.EXPIRED_AT IS NOT NULL THEN 'Expired'
            WHEN h.ACCEPTED_AT IS NOT NULL THEN 'Accepted'
            WHEN h.STARTED_AT IS NOT NULL THEN 'InProgress'
            WHEN h.READY_AT IS NOT NULL THEN 'Ready'
            ELSE 'Draft'
          END AS Status,
          hpd.ILLNESS_SEVERITY AS Severity
          FROM (
            SELECT ID, NAME, DATE_OF_BIRTH, ROOM_NUMBER, DIAGNOSIS, ROW_NUMBER() OVER (ORDER BY ID) AS RN
            FROM PATIENTS
            WHERE UNIT_ID = :unitId
          ) p
          LEFT JOIN (
            SELECT ID, PATIENT_ID, STATUS, COMPLETED_AT, CANCELLED_AT, REJECTED_AT, EXPIRED_AT, ACCEPTED_AT, STARTED_AT, READY_AT,
                   ROW_NUMBER() OVER (PARTITION BY PATIENT_ID ORDER BY CREATED_AT DESC) AS rn
            FROM HANDOVERS
          ) h ON p.ID = h.PATIENT_ID AND h.rn = 1
          LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
          WHERE p.RN BETWEEN :startRow AND :endRow";

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
        const string pageSql = @"SELECT p.ID AS Id, p.NAME AS Name, 'NotStarted' AS HandoverStatus, CAST(NULL AS VARCHAR(255)) AS HandoverId,
          CAST(FLOOR((SYSDATE - p.DATE_OF_BIRTH)/365.25) AS DECIMAL(10,2)) AS Age, p.ROOM_NUMBER AS Room, p.DIAGNOSIS AS Diagnosis,
          CASE
            WHEN h.STATUS = 'Completed' AND h.COMPLETED_AT IS NOT NULL THEN 'Completed'
            WHEN h.CANCELLED_AT IS NOT NULL THEN 'Cancelled'
            WHEN h.REJECTED_AT IS NOT NULL THEN 'Rejected'
            WHEN h.EXPIRED_AT IS NOT NULL THEN 'Expired'
            WHEN h.ACCEPTED_AT IS NOT NULL THEN 'Accepted'
            WHEN h.STARTED_AT IS NOT NULL THEN 'InProgress'
            WHEN h.READY_AT IS NOT NULL THEN 'Ready'
            ELSE 'Draft'
          END AS Status,
          hpd.ILLNESS_SEVERITY AS Severity
          FROM (
            SELECT ID, NAME, DATE_OF_BIRTH, ROOM_NUMBER, DIAGNOSIS, ROW_NUMBER() OVER (ORDER BY ID) AS RN
            FROM PATIENTS
          ) p
          LEFT JOIN (
            SELECT ID, PATIENT_ID, STATUS, COMPLETED_AT, CANCELLED_AT, REJECTED_AT, EXPIRED_AT, ACCEPTED_AT, STARTED_AT, READY_AT,
                   ROW_NUMBER() OVER (PARTITION BY PATIENT_ID ORDER BY CREATED_AT DESC) AS rn
            FROM HANDOVERS
          ) h ON p.ID = h.PATIENT_ID AND h.rn = 1
          LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
          WHERE p.RN BETWEEN :startRow AND :endRow";

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

            // Insert new assignments with explicit ASSIGNMENT_ID
            foreach (var patientId in patientIds)
            {
                // Check if patient is already assigned to someone else
                var existingAssignment = await conn.ExecuteScalarAsync<string>(
                    "SELECT ASSIGNMENT_ID FROM USER_ASSIGNMENTS WHERE PATIENT_ID = :patientId",
                    new { patientId });

                if (existingAssignment != null)
                {
                    _logger.LogWarning("Patient {PatientId} is already assigned (assignment: {ExistingAssignment}), skipping assignment for user {UserId}",
                        patientId, existingAssignment, userId);
                    continue;
                }

                var assignmentId = $"assign-{userId}-{shiftId}-{patientId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}";

                await conn.ExecuteAsync(@"
                INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID)
                VALUES (:assignmentId, :userId, :shiftId, :patientId)",
                new { assignmentId, userId, shiftId, patientId });

                assignmentIds.Add(assignmentId);
                _logger.LogDebug("Assigned patient {PatientId} to user {UserId} with assignment {AssignmentId}",
                    patientId, userId, assignmentId);
            }

            _logger.LogInformation("Successfully assigned {Count} patients to user {UserId}. Assignment IDs: {@AssignmentIds}", assignmentIds.Count, userId, assignmentIds);
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

            // Get assigned patients with full patient details
            const string patientsSql = @"
            SELECT p.ID AS Id, p.NAME AS Name, 'NotStarted' AS HandoverStatus, CAST(NULL AS VARCHAR(255)) AS HandoverId,
            FLOOR((SYSDATE - p.DATE_OF_BIRTH)/365.25) AS Age, p.ROOM_NUMBER AS Room, p.DIAGNOSIS AS Diagnosis,
            CASE
              WHEN h.STATUS = 'Completed' AND h.COMPLETED_AT IS NOT NULL THEN 'Completed'
              WHEN h.CANCELLED_AT IS NOT NULL THEN 'Cancelled'
              WHEN h.REJECTED_AT IS NOT NULL THEN 'Rejected'
              WHEN h.EXPIRED_AT IS NOT NULL THEN 'Expired'
              WHEN h.ACCEPTED_AT IS NOT NULL THEN 'Accepted'
              WHEN h.STARTED_AT IS NOT NULL THEN 'InProgress'
              WHEN h.READY_AT IS NOT NULL THEN 'Ready'
              ELSE 'Draft'
            END AS Status,
            hpd.ILLNESS_SEVERITY AS Severity
            FROM PATIENTS p
            INNER JOIN USER_ASSIGNMENTS ua ON p.ID = ua.PATIENT_ID
            LEFT JOIN (
              SELECT ID, PATIENT_ID, STATUS, COMPLETED_AT, CANCELLED_AT, REJECTED_AT, EXPIRED_AT, ACCEPTED_AT, STARTED_AT, READY_AT,
                     ROW_NUMBER() OVER (PARTITION BY PATIENT_ID ORDER BY CREATED_AT DESC) AS rn
              FROM HANDOVERS
            ) h ON p.ID = h.PATIENT_ID AND h.rn = 1
            LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
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
          SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME, h.STATUS,
                 pd.ILLNESS_SEVERITY, pd.SUMMARY_TEXT as PATIENT_SUMMARY,
                sa.CONTENT as SITUATION_AWARENESS_CONTENT, sa.LAST_EDITED_BY as SITUATION_AWARENESS_EDITOR,
                syn.CONTENT as SYNTHESIS_CONTENT, syn.LAST_EDITED_BY as SYNTHESIS_EDITOR,
                 h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO,
                 cb.FULL_NAME as CREATED_BY_NAME, td.FULL_NAME as ASSIGNED_TO_NAME,
                 COALESCE(h.RESPONSIBLE_PHYSICIAN_ID, h.CREATED_BY) AS RESPONSIBLE_PHYSICIAN_ID,
                 rp.FULL_NAME as RESPONSIBLE_PHYSICIAN_NAME,
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
                 h.TO_DOCTOR_ID,
                 h.VERSION
          FROM HANDOVERS h
          INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
          INNER JOIN USER_ASSIGNMENTS ua ON h.PATIENT_ID = ua.PATIENT_ID
          LEFT JOIN VW_HANDOVERS_STATE vws ON h.ID = vws.HandoverId
          LEFT JOIN USERS cb ON h.CREATED_BY = cb.ID
          LEFT JOIN USERS td ON h.TO_DOCTOR_ID = td.ID
          LEFT JOIN USERS rp ON rp.ID = COALESCE(h.RESPONSIBLE_PHYSICIAN_ID, h.CREATED_BY)
          LEFT JOIN HANDOVER_PATIENT_DATA pd ON h.ID = pd.HANDOVER_ID
          LEFT JOIN HANDOVER_SITUATION_AWARENESS sa ON h.ID = sa.HANDOVER_ID
          LEFT JOIN HANDOVER_SYNTHESIS syn ON h.ID = syn.HANDOVER_ID
          WHERE ua.USER_ID = :userId
          ORDER BY h.CREATED_AT DESC
          OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY";

        var handoverRows = conn.Query(handoverSql, new { userId, offset, pageSize }).ToList();

        // Get action items for each handover
        var handovers = new List<HandoverRecord>();
        foreach (var row in handoverRows)
        {
            var handoverId = row.ID;


                var handover = new HandoverRecord(
                    Id: row.ID,
                    AssignmentId: row.ASSIGNMENT_ID,
                    PatientId: row.PATIENT_ID,
                    PatientName: row.PATIENT_NAME,
                    Status: row.STATUS,
                    IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
                    PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
                    SituationAwarenessDocId: row.SITUATION_AWARENESS_EDITOR,
                    Synthesis: string.IsNullOrEmpty(row.SYNTHESIS_CONTENT) ? null : new HandoverSynthesis(row.SYNTHESIS_CONTENT),
                    ShiftName: row.SHIFT_NAME ?? "Unknown",
                    CreatedBy: row.CREATED_BY ?? "system",
                    AssignedTo: row.ASSIGNED_TO ?? "system",
                    CreatedByName: row.CREATED_BY_NAME,
                    AssignedToName: row.ASSIGNED_TO_NAME,
                    ReceiverUserId: row.RECEIVER_USER_ID,
                    ResponsiblePhysicianId: row.RESPONSIBLE_PHYSICIAN_ID,
                    ResponsiblePhysicianName: row.RESPONSIBLE_PHYSICIAN_NAME,
                    CreatedAt: row.CREATED_AT,
                    ReadyAt: row.READY_AT,
                    StartedAt: row.STARTED_AT,
                    AcknowledgedAt: row.ACKNOWLEDGED_AT,
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
                    StateName: row.STATENAME ?? "Draft",
                    Version: row.VERSION ?? 1
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

    public async Task CreateHandoverForAssignmentAsync(string assignmentId, string userId, string userName, DateTime windowDate, string fromShiftId, string toShiftId)
    {
        try
        {
            _logger.LogInformation("Starting handover creation for assignment {AssignmentId}, user {UserId}, window {WindowDate}, from {FromShiftId} to {ToShiftId}",
                assignmentId, userId, windowDate, fromShiftId, toShiftId);

            // Ensure the user exists in the database before creating handovers
            EnsureUserExists(userId, null, null, null, null);

            using IDbConnection conn = _factory.CreateConnection();

            // Get assignment details
            var assignment = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT USER_ID, SHIFT_ID, PATIENT_ID FROM USER_ASSIGNMENTS WHERE ASSIGNMENT_ID = :assignmentId",
                new { assignmentId });

            _logger.LogInformation("Assignment lookup result: {@Assignment}", assignment != null ? (object)assignment : new { Message = "Assignment not found" });

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

            var fromShiftName = await conn.ExecuteScalarAsync<string>("SELECT NAME FROM SHIFTS WHERE ID = :fromShiftId", new { fromShiftId }) ?? "Unknown";
            var toShiftName = await conn.ExecuteScalarAsync<string>("SELECT NAME FROM SHIFTS WHERE ID = :toShiftId", new { toShiftId }) ?? "Unknown";

            // Check if there's already an active handover for this patient/window/shift combination
            var existingActiveHandover = await conn.ExecuteScalarAsync<string>(@"
                SELECT ID FROM HANDOVERS
                WHERE PATIENT_ID = :patientId
                AND HANDOVER_WINDOW_DATE = :windowDate
                AND FROM_SHIFT_ID = :fromShiftId
                AND TO_SHIFT_ID = :toShiftId
                AND COMPLETED_AT IS NULL
                AND CANCELLED_AT IS NULL
                AND REJECTED_AT IS NULL
                AND EXPIRED_AT IS NULL", new {
                    patientId = assignment.PATIENT_ID,
                    windowDate,
                    fromShiftId,
                    toShiftId
                });

            if (existingActiveHandover != null)
            {
                _logger.LogInformation("Skipping handover creation for assignment {AssignmentId} - active handover {ExistingHandoverId} already exists for patient {PatientId}", new object[] { assignmentId, existingActiveHandover, assignment.PATIENT_ID });
                return;
            }

            // Create handover
            await conn.ExecuteAsync(@"
            INSERT INTO HANDOVERS (
                ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, SHIFT_NAME, CREATED_BY,
                TO_DOCTOR_ID, RECEIVER_USER_ID, FROM_DOCTOR_ID,
                HANDOVER_WINDOW_DATE, FROM_SHIFT_ID, TO_SHIFT_ID, RESPONSIBLE_PHYSICIAN_ID
            ) VALUES (
                :handoverId, :assignmentId, :patientId, 'Draft', :shiftName, :userId,
                null, null, :userId,
                :windowDate, :fromShiftId, :toShiftId, :userId
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

            // Create handover patient data
            await conn.ExecuteAsync(@"
            INSERT INTO HANDOVER_PATIENT_DATA (
                HANDOVER_ID, ILLNESS_SEVERITY, SUMMARY_TEXT, LAST_EDITED_BY
            ) VALUES (
                :handoverId, 'Stable', 'Handover iniciado - información pendiente de completar', :userId
            )", new {
                handoverId,
                userId
            });

            // Create situation awareness record
            await conn.ExecuteAsync(@"
            INSERT INTO HANDOVER_SITUATION_AWARENESS (
                HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT
            ) VALUES (
                :handoverId, '', 'Draft', :userId, SYSDATE, SYSDATE
            )", new {
                handoverId,
                userId
            });

            // Create synthesis record
            await conn.ExecuteAsync(@"
            INSERT INTO HANDOVER_SYNTHESIS (
                HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT
            ) VALUES (
                :handoverId, '', 'Draft', :userId, SYSDATE, SYSDATE
            )", new {
                handoverId,
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
            // TEMPORARY: Return hardcoded result to test if the issue is in the repository or endpoint
            var handovers = new List<HandoverRecord>
            {
                new HandoverRecord(
                    Id: "handover-001",
                    AssignmentId: "assign-001",
                    PatientId: patientId,
                    PatientName: "Test Patient",
                    Status: "Ready",
                    IllnessSeverity: new HandoverIllnessSeverity("Stable"),
                    PatientSummary: new HandoverPatientSummary("Test summary"),
                    SituationAwarenessDocId: null,
                    Synthesis: null,
                    ShiftName: "Test Shift",
                    CreatedBy: "system",
                    AssignedTo: "system",
                    CreatedByName: null,
                    AssignedToName: null,
                    ReceiverUserId: null,
                    ResponsiblePhysicianId: "system",
                    ResponsiblePhysicianName: "Test Physician",
                    CreatedAt: "2024-01-01 12:00:00",
                    ReadyAt: "2024-01-01 12:00:00",
                    StartedAt: null,
                    AcknowledgedAt: null,
                    AcceptedAt: null,
                    CompletedAt: null,
                    CancelledAt: null,
                    RejectedAt: null,
                    RejectionReason: null,
                    ExpiredAt: null,
                    HandoverType: "ShiftToShift",
                    HandoverWindowDate: null,
                    FromShiftId: "shift-day",
                    ToShiftId: "shift-night",
                    ToDoctorId: "system",
                    StateName: "Ready",
                    Version: 1
                )
            };

            return (handovers, 1);
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
              SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME, h.STATUS,
                     pd.ILLNESS_SEVERITY, pd.SUMMARY_TEXT as PATIENT_SUMMARY,
                sa.CONTENT as SITUATION_AWARENESS_CONTENT, sa.LAST_EDITED_BY as SITUATION_AWARENESS_EDITOR,
                syn.CONTENT as SYNTHESIS_CONTENT, syn.LAST_EDITED_BY as SYNTHESIS_EDITOR,
                     h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO,
                     h.RECEIVER_USER_ID,
                     cb.FULL_NAME as CREATED_BY_NAME, td.FULL_NAME as ASSIGNED_TO_NAME,
                     COALESCE(h.RESPONSIBLE_PHYSICIAN_ID, h.CREATED_BY) AS RESPONSIBLE_PHYSICIAN_ID,
                     rp.FULL_NAME as RESPONSIBLE_PHYSICIAN_NAME,
                     TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
                     TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as READY_AT,
                     TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as STARTED_AT,
                     TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACKNOWLEDGED_AT,
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
                     h.TO_DOCTOR_ID,
                     h.VERSION
              FROM HANDOVERS h
              LEFT JOIN PATIENTS p ON h.PATIENT_ID = p.ID
              LEFT JOIN VW_HANDOVERS_STATE vws ON h.ID = vws.HandoverId
              LEFT JOIN USERS cb ON h.CREATED_BY = cb.ID
              LEFT JOIN USERS td ON h.TO_DOCTOR_ID = td.ID
              LEFT JOIN USERS rp ON rp.ID = COALESCE(h.RESPONSIBLE_PHYSICIAN_ID, h.CREATED_BY)
              LEFT JOIN HANDOVER_PATIENT_DATA pd ON h.ID = pd.HANDOVER_ID
              LEFT JOIN HANDOVER_SITUATION_AWARENESS sa ON h.ID = sa.HANDOVER_ID
              LEFT JOIN HANDOVER_SYNTHESIS syn ON h.ID = syn.HANDOVER_ID
              WHERE h.ID = :handoverId";

            var row = conn.QueryFirstOrDefault(handoverSql, new { handoverId });

            if (row == null)
            {
                return null;
            }

            // Get action items for the handover

            return new HandoverRecord(
                Id: row.ID,
                AssignmentId: row.ASSIGNMENT_ID,
                PatientId: row.PATIENT_ID,
                PatientName: row.PATIENT_NAME,
                Status: row.STATUS,
                IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
                PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
                SituationAwarenessDocId: row.SITUATION_AWARENESS_EDITOR,
                Synthesis: !string.IsNullOrEmpty(row.SYNTHESIS_CONTENT) ? new HandoverSynthesis(row.SYNTHESIS_CONTENT) : null,
                ShiftName: row.SHIFT_NAME ?? "Unknown",
                CreatedBy: row.CREATED_BY ?? "system",
                AssignedTo: row.ASSIGNED_TO ?? "system",
                CreatedByName: row.CREATED_BY_NAME,
                AssignedToName: row.ASSIGNED_TO_NAME,
                ReceiverUserId: row.RECEIVER_USER_ID,
                ResponsiblePhysicianId: row.RESPONSIBLE_PHYSICIAN_ID,
                ResponsiblePhysicianName: row.RESPONSIBLE_PHYSICIAN_NAME,
                CreatedAt: row.CREATED_AT,
                ReadyAt: row.READY_AT,
                StartedAt: row.STARTED_AT,
                AcknowledgedAt: row.ACKNOWLEDGED_AT,
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
                StateName: row.STATENAME ?? "Draft",
                Version: row.VERSION ?? 1
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
                     p.UNIT_ID, p.ROOM_NUMBER, p.DIAGNOSIS, p.ALLERGIES, p.MEDICATIONS, p.NOTES, p.MRN
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
                Mrn: row.MRN ?? GenerateMrn(row.ID), // Use MRN from database, fallback to generated
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

    // Singleton Sections
    public async Task<HandoverPatientDataRecord?> GetPatientDataAsync(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT HANDOVER_ID as HandoverId, ILLNESS_SEVERITY as IllnessSeverity, SUMMARY_TEXT as SummaryText,
                       LAST_EDITED_BY as LastEditedBy, STATUS, CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
                FROM HANDOVER_PATIENT_DATA
                WHERE HANDOVER_ID = :handoverId";
            
            return await conn.QueryFirstOrDefaultAsync<HandoverPatientDataRecord>(sql, new { handoverId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get patient data for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<HandoverSituationAwarenessRecord?> GetSituationAwarenessAsync(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            // First check if the handover exists
            var handoverExists = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM HANDOVERS WHERE ID = :handoverId",
                new { handoverId });

            // If handover doesn't exist, return null
            if (handoverExists == 0)
            {
                return null;
            }

            const string sql = @"
                SELECT HANDOVER_ID as HandoverId, CONTENT as Content, STATUS,
                       LAST_EDITED_BY as LastEditedBy, CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
                FROM HANDOVER_SITUATION_AWARENESS
                WHERE HANDOVER_ID = :handoverId";

            var result = await conn.QueryFirstOrDefaultAsync<HandoverSituationAwarenessRecord>(sql, new { handoverId });

            // If no record exists, create a default one
            if (result == null)
            {
                // Get the handover's created_by to use as last_edited_by
                var createdBy = await conn.ExecuteScalarAsync<string>(
                    "SELECT CREATED_BY FROM HANDOVERS WHERE ID = :handoverId",
                    new { handoverId });

                await conn.ExecuteAsync(@"
                INSERT INTO HANDOVER_SITUATION_AWARENESS (
                    HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT
                ) VALUES (
                    :handoverId, '', 'Draft', :createdBy, SYSDATE, SYSDATE
                )", new { handoverId, createdBy });

                // Return the newly created record
                result = await conn.QueryFirstOrDefaultAsync<HandoverSituationAwarenessRecord>(sql, new { handoverId });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get situation awareness for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<HandoverSynthesisRecord?> GetSynthesisAsync(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT HANDOVER_ID as HandoverId, CONTENT as Content, STATUS,
                       LAST_EDITED_BY as LastEditedBy, CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
                FROM HANDOVER_SYNTHESIS
                WHERE HANDOVER_ID = :handoverId";

            var result = await conn.QueryFirstOrDefaultAsync<HandoverSynthesisRecord>(sql, new { handoverId });

            // If no record exists, create a default one
            if (result == null)
            {
                // Get the handover's created_by to use as last_edited_by
                var createdBy = await conn.ExecuteScalarAsync<string>(
                    "SELECT CREATED_BY FROM HANDOVERS WHERE ID = :handoverId",
                    new { handoverId });

                await conn.ExecuteAsync(@"
                INSERT INTO HANDOVER_SYNTHESIS (
                    HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT
                ) VALUES (
                    :handoverId, '', 'Draft', :createdBy, SYSDATE, SYSDATE
                )", new { handoverId, createdBy });

                // Return the newly created record
                result = await conn.QueryFirstOrDefaultAsync<HandoverSynthesisRecord>(sql, new { handoverId });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get synthesis for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<bool> UpdatePatientDataAsync(string handoverId, string illnessSeverity, string? summaryText, string status, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                MERGE INTO HANDOVER_PATIENT_DATA pd
                USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (pd.HANDOVER_ID = src.HANDOVER_ID)
                WHEN MATCHED THEN
                    UPDATE SET
                        ILLNESS_SEVERITY = :illnessSeverity,
                        SUMMARY_TEXT = :summaryText,
                        STATUS = :status,
                        LAST_EDITED_BY = :userId,
                        UPDATED_AT = SYSTIMESTAMP
                WHEN NOT MATCHED THEN
                    INSERT (HANDOVER_ID, ILLNESS_SEVERITY, SUMMARY_TEXT, STATUS, LAST_EDITED_BY, UPDATED_AT)
                    VALUES (:handoverId, :illnessSeverity, :summaryText, :status, :userId, SYSTIMESTAMP)";

            var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, illnessSeverity, summaryText, status, userId });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update patient data for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<bool> UpdateSituationAwarenessAsync(string handoverId, string? content, string status, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                MERGE INTO HANDOVER_SITUATION_AWARENESS s
                USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (s.HANDOVER_ID = src.HANDOVER_ID)
                WHEN MATCHED THEN
                    UPDATE SET CONTENT = :content, STATUS = :status, LAST_EDITED_BY = :userId, UPDATED_AT = SYSTIMESTAMP
                WHEN NOT MATCHED THEN
                    INSERT (HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, UPDATED_AT)
                    VALUES (:handoverId, :content, :status, :userId, SYSTIMESTAMP)";

            var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, content, status, userId });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update situation awareness for handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task<bool> UpdateSynthesisAsync(string handoverId, string? content, string status, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                MERGE INTO HANDOVER_SYNTHESIS s
                USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (s.HANDOVER_ID = src.HANDOVER_ID)
                WHEN MATCHED THEN
                    UPDATE SET CONTENT = :content, STATUS = :status, LAST_EDITED_BY = :userId, UPDATED_AT = SYSTIMESTAMP
                WHEN NOT MATCHED THEN
                    INSERT (HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, UPDATED_AT)
                    VALUES (:handoverId, :content, :status, :userId, SYSTIMESTAMP)";

            var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, content, status, userId });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update synthesis for handover {HandoverId}", handoverId);
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
                       hm.USER_NAME as UserName,
                       hm.MESSAGE_TEXT as MessageText, hm.MESSAGE_TYPE as MessageType,
                       hm.CREATED_AT as CreatedAt, hm.UPDATED_AT as UpdatedAt
                FROM HANDOVER_MESSAGES hm
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
                VALUES (:id, :handoverId, :userId, :userName, :messageText, :messageType, SYSTIMESTAMP, SYSTIMESTAMP)";

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

    public bool DeleteContingencyPlan(string handoverId, string contingencyId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                DELETE FROM HANDOVER_CONTINGENCY
                WHERE ID = :contingencyId AND HANDOVER_ID = :handoverId";

            var result = conn.Execute(sql, new { contingencyId, handoverId });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete contingency plan {ContingencyId}", contingencyId);
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
                WHERE ID = :handoverId AND STARTED_AT IS NOT NULL AND ACCEPTED_AT IS NULL";
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
                WHERE ID = :handoverId AND ACCEPTED_AT IS NOT NULL AND COMPLETED_AT IS NULL";
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

    // ===================================
    // Optimistic Locking Implementations
    // ===================================

    public async Task<bool> StartHandover(string handoverId, string userId, int expectedVersion)
    {
        return await ExecuteWithOptimisticLock(handoverId, expectedVersion, async (conn) =>
        {
            const string sql = @"
                UPDATE HANDOVERS
                SET STARTED_AT = SYSTIMESTAMP,
                    STATUS = 'InProgress',
                    UPDATED_AT = SYSTIMESTAMP,
                    VERSION = VERSION + 1
                WHERE ID = :handoverId 
                  AND VERSION = :expectedVersion
                  AND READY_AT IS NOT NULL 
                  AND STARTED_AT IS NULL";
            
            var result = await conn.ExecuteAsync(sql, new { handoverId, expectedVersion });
            return result > 0;
        }, "Start");
    }

    public async Task<bool> ReadyHandover(string handoverId, string userId, int expectedVersion)
    {
        return await ExecuteWithOptimisticLock(handoverId, expectedVersion, async (conn) =>
        {
            const string sql = @"
                UPDATE HANDOVERS
                SET READY_AT = SYSTIMESTAMP,
                    STATUS = 'Ready',
                    UPDATED_AT = SYSTIMESTAMP,
                    VERSION = VERSION + 1
                WHERE ID = :handoverId 
                  AND VERSION = :expectedVersion
                  AND READY_AT IS NULL";
            
            var result = await conn.ExecuteAsync(sql, new { handoverId, expectedVersion });
            return result > 0;
        }, "Ready");
    }

    public async Task<bool> AcceptHandover(string handoverId, string userId, int expectedVersion)
    {
        return await ExecuteWithOptimisticLock(handoverId, expectedVersion, async (conn) =>
        {
            const string sql = @"
                UPDATE HANDOVERS
                SET ACCEPTED_AT = SYSTIMESTAMP,
                    UPDATED_AT = SYSTIMESTAMP,
                    VERSION = VERSION + 1
                WHERE ID = :handoverId 
                  AND VERSION = :expectedVersion
                  AND STARTED_AT IS NOT NULL 
                  AND ACCEPTED_AT IS NULL";
            
            var result = await conn.ExecuteAsync(sql, new { handoverId, expectedVersion });
            return result > 0;
        }, "Accept");
    }

    public async Task<bool> CompleteHandover(string handoverId, string userId, int expectedVersion)
    {
        return await ExecuteWithOptimisticLock(handoverId, expectedVersion, async (conn) =>
        {
            const string sql = @"
                UPDATE HANDOVERS
                SET COMPLETED_AT = SYSTIMESTAMP,
                    STATUS = 'Completed',
                    UPDATED_AT = SYSTIMESTAMP,
                    COMPLETED_BY = :userId,
                    VERSION = VERSION + 1
                WHERE ID = :handoverId 
                  AND VERSION = :expectedVersion
                  AND ACCEPTED_AT IS NOT NULL 
                  AND COMPLETED_AT IS NULL";
            
            var result = await conn.ExecuteAsync(sql, new { handoverId, userId, expectedVersion });
            return result > 0;
        }, "Complete");
    }

    public async Task<bool> CancelHandover(string handoverId, string userId, int expectedVersion)
    {
        return await ExecuteWithOptimisticLock(handoverId, expectedVersion, async (conn) =>
        {
            const string sql = @"
                UPDATE HANDOVERS
                SET CANCELLED_AT = SYSTIMESTAMP,
                    STATUS = 'Cancelled',
                    UPDATED_AT = SYSTIMESTAMP,
                    VERSION = VERSION + 1
                WHERE ID = :handoverId 
                  AND VERSION = :expectedVersion
                  AND CANCELLED_AT IS NULL 
                  AND ACCEPTED_AT IS NULL";
            
            var result = await conn.ExecuteAsync(sql, new { handoverId, expectedVersion });
            return result > 0;
        }, "Cancel");
    }

    public async Task<bool> RejectHandover(string handoverId, string userId, string reason, int expectedVersion)
    {
        return await ExecuteWithOptimisticLock(handoverId, expectedVersion, async (conn) =>
        {
            const string sql = @"
                UPDATE HANDOVERS
                SET REJECTED_AT = SYSTIMESTAMP,
                    REJECTION_REASON = :reason,
                    STATUS = 'Rejected',
                    UPDATED_AT = SYSTIMESTAMP,
                    VERSION = VERSION + 1
                WHERE ID = :handoverId 
                  AND VERSION = :expectedVersion
                  AND REJECTED_AT IS NULL 
                  AND ACCEPTED_AT IS NULL";
            
            var result = await conn.ExecuteAsync(sql, new { handoverId, reason, expectedVersion });
            return result > 0;
        }, "Reject");
    }

    /// <summary>
    /// Helper method to execute a handover state transition with optimistic locking.
    /// If the update fails due to version mismatch, throws OptimisticLockException.
    /// </summary>
    private async Task<bool> ExecuteWithOptimisticLock(
        string handoverId, 
        int expectedVersion, 
        Func<IDbConnection, Task<bool>> operation,
        string operationName)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            
            var success = await operation(conn);

            if (!success)
            {
                // Check if version mismatch vs invalid state
                var current = await conn.QuerySingleOrDefaultAsync<int?>(
                    "SELECT VERSION FROM HANDOVERS WHERE ID = :handoverId",
                    new { handoverId });

                if (current.HasValue && current.Value != expectedVersion)
                {
                    throw new Core.Exceptions.OptimisticLockException(
                        $"{operationName} failed: Version mismatch for handover {handoverId}. " +
                        $"Expected version {expectedVersion}, but current version is {current.Value}. " +
                        $"The handover was modified by another user.");
                }

                // Invalid state transition (not a version mismatch)
                _logger.LogWarning("{Operation} failed for handover {HandoverId}: Invalid state transition", 
                    operationName, handoverId);
                return false;
            }

            return true;
        }
        catch (Core.Exceptions.OptimisticLockException)
        {
            throw; // Re-throw optimistic lock exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to {Operation} handover {HandoverId}", operationName, handoverId);
            throw;
        }
    }

    // Patient Summaries
    public PatientSummaryRecord? GetPatientSummary(string patientId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT * FROM (
                    SELECT ID AS Id,
                           PATIENT_ID AS PatientId,
                           PHYSICIAN_ID AS PhysicianId,
                           SUMMARY_TEXT AS SummaryText,
                           CREATED_AT AS CreatedAt,
                           UPDATED_AT AS UpdatedAt,
                           LAST_EDITED_BY AS LastEditedBy
                    FROM PATIENT_SUMMARIES
                    WHERE PATIENT_ID = :patientId
                    ORDER BY UPDATED_AT DESC
                ) WHERE ROWNUM = 1";

            return conn.QueryFirstOrDefault<PatientSummaryRecord>(sql, new { patientId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get patient summary for patient {PatientId}", patientId);
            throw;
        }
    }

    public PatientSummaryRecord CreatePatientSummary(string patientId, string physicianId, string summaryText, string createdBy)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            var summaryId = Guid.NewGuid().ToString();

            const string sql = @"
                INSERT INTO PATIENT_SUMMARIES (ID, PATIENT_ID, PHYSICIAN_ID, SUMMARY_TEXT, CREATED_AT, UPDATED_AT, LAST_EDITED_BY)
                VALUES (:summaryId, :patientId, :physicianId, :summaryText, SYSTIMESTAMP, SYSTIMESTAMP, :createdBy)";

            conn.Execute(sql, new { summaryId, patientId, physicianId, summaryText, createdBy });

            return new PatientSummaryRecord(
                summaryId,
                patientId,
                physicianId,
                summaryText,
                DateTime.UtcNow,
                DateTime.UtcNow,
                createdBy
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create patient summary for patient {PatientId}", patientId);
            throw;
        }
    }

    public bool UpdatePatientSummary(string summaryId, string summaryText, string lastEditedBy)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE PATIENT_SUMMARIES
                SET SUMMARY_TEXT = :summaryText,
                    UPDATED_AT = SYSTIMESTAMP,
                    LAST_EDITED_BY = :lastEditedBy
                WHERE ID = :summaryId";

            var result = conn.Execute(sql, new { summaryId, summaryText, lastEditedBy });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update patient summary {SummaryId}", summaryId);
            throw;
        }
    }
}
