using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.Repositories;

public class OracleSetupRepository : ISetupRepository, ISetupQueryService, ISetupCommandService
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
                 h.SITUATION_AWARENESS_DOC_ID, h.SYNTHESIS, h.SHIFT_NAME, h.CREATED_BY, h.ASSIGNED_TO,
                 TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT
          FROM HANDOVERS h
          INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
          INNER JOIN USER_ASSIGNMENTS ua ON h.PATIENT_ID = ua.PATIENT_ID
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

    public HandoverRecord? GetActiveHandover(string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            // Find the active handover for this user's assigned patients
            const string handoverSql = @"
                SELECT * FROM (
                    SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME, h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY,
                           h.SITUATION_AWARENESS_DOC_ID, h.SYNTHESIS, h.SHIFT_NAME, h.CREATED_BY, h.ASSIGNED_TO,
                           TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT
                    FROM HANDOVERS h
                    INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
                    WHERE h.ASSIGNED_TO = :userId
                      AND h.STATUS IN ('Active', 'InProgress')
                    ORDER BY h.CREATED_AT DESC
                ) WHERE ROWNUM = 1";

            var row = conn.QueryFirstOrDefault(handoverSql, new { userId });

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

            var actionItems = conn.Query(actionItemsSql, new { handoverId = row.ID })
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
            _logger.LogError(ex, "Failed to get active handover for user {UserId}", userId);
            throw;
        }
    }

    public IReadOnlyList<HandoverParticipantRecord> GetHandoverParticipants(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                SELECT ID, USER_ID, USER_NAME, USER_ROLE, STATUS,
                       JOINED_AT, LAST_ACTIVITY
                FROM HANDOVER_PARTICIPANTS
                WHERE HANDOVER_ID = :handoverId
                ORDER BY JOINED_AT";

            var participants = conn.Query<HandoverParticipantRecord>(sql, new { handoverId }).ToList();

            // If no participants found, return a default list with the assigned user
            if (!participants.Any())
            {
                // Get the handover creator as the default participant
                const string creatorSql = @"
                    SELECT ASSIGNED_TO as USER_ID, 'Assigned Physician' as USER_NAME, 'Doctor' as USER_ROLE
                    FROM HANDOVERS
                    WHERE ID = :handoverId";

                var creator = conn.QueryFirstOrDefault(creatorSql, new { handoverId });

                if (creator != null)
                {
                    participants.Add(new HandoverParticipantRecord(
                        Id: $"participant-{handoverId}-default",
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
                SELECT ID, SECTION_TYPE, CONTENT, STATUS, LAST_EDITED_BY,
                       CREATED_AT, UPDATED_AT
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
                        SectionType: "illness_severity",
                        Content: "Stable",
                        Status: "draft",
                        LastEditedBy: null,
                        CreatedAt: DateTime.Now,
                        UpdatedAt: DateTime.Now
                    ),
                    new HandoverSectionRecord(
                        Id: $"section-{handoverId}-summary",
                        SectionType: "patient_summary",
                        Content: "",
                        Status: "draft",
                        LastEditedBy: null,
                        CreatedAt: DateTime.Now,
                        UpdatedAt: DateTime.Now
                    ),
                    new HandoverSectionRecord(
                        Id: $"section-{handoverId}-actions",
                        SectionType: "action_items",
                        Content: "",
                        Status: "draft",
                        LastEditedBy: null,
                        CreatedAt: DateTime.Now,
                        UpdatedAt: DateTime.Now
                    ),
                    new HandoverSectionRecord(
                        Id: $"section-{handoverId}-awareness",
                        SectionType: "situation_awareness",
                        Content: "",
                        Status: "draft",
                        LastEditedBy: null,
                        CreatedAt: DateTime.Now,
                        UpdatedAt: DateTime.Now
                    ),
                    new HandoverSectionRecord(
                        Id: $"section-{handoverId}-synthesis",
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
                SELECT ID, SYNC_STATUS, LAST_SYNC, VERSION
                FROM HANDOVER_SYNC_STATUS
                WHERE HANDOVER_ID = :handoverId AND USER_ID = :userId";

            var syncStatus = conn.QueryFirstOrDefault<HandoverSyncStatusRecord>(sql, new { handoverId, userId });

            // If no sync status exists, create a default one
            if (syncStatus == null)
            {
                syncStatus = new HandoverSyncStatusRecord(
                    Id: $"sync-{handoverId}-{userId}",
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

    // ISetupQueryService implementation (async methods)
    public async Task<IReadOnlyList<UnitRecord>> GetUnitsAsync()
    {
        using IDbConnection conn = _factory.CreateConnection();
        const string sql = "SELECT ID AS Id, NAME AS Name FROM UNITS ORDER BY ID";
        var result = await conn.QueryAsync<UnitRecord>(sql);
        return result.ToList();
    }

    public async Task<IReadOnlyList<ShiftRecord>> GetShiftsAsync()
    {
        using IDbConnection conn = _factory.CreateConnection();
        const string sql = @"SELECT ID AS Id, NAME AS Name, START_TIME AS StartTime, END_TIME AS EndTime FROM SHIFTS ORDER BY ID";
        var result = await conn.QueryAsync<ShiftRecord>(sql);
        return result.ToList();
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(string unitId, int page, int pageSize)
    {
        using IDbConnection conn = _factory.CreateConnection();
        int p = Math.Max(page, 1);
        int ps = Math.Max(pageSize, 1);
        const string countSql = "SELECT COUNT(1) FROM PATIENTS WHERE UNIT_ID = :unitId";
        const string pageSql = @"SELECT ID AS Id, NAME AS Name, 'NotStarted' AS HandoverStatus, CAST(NULL AS VARCHAR(255)) AS HandoverId FROM (
          SELECT ID, NAME, ROW_NUMBER() OVER (ORDER BY ID) AS RN
          FROM PATIENTS WHERE UNIT_ID = :unitId
        ) WHERE RN BETWEEN :startRow AND :endRow";

        int total = await conn.ExecuteScalarAsync<int>(countSql, new { unitId });
        int startRow = ((p - 1) * ps) + 1;
        int endRow = p * ps;
        var items = await conn.QueryAsync<PatientRecord>(pageSql, new { unitId, startRow, endRow });
        return (items.ToList(), total);
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetAllPatientsAsync(int page, int pageSize)
    {
        using IDbConnection conn = _factory.CreateConnection();
        int p = Math.Max(page, 1);
        int ps = Math.Max(pageSize, 1);
        const string countSql = "SELECT COUNT(1) FROM PATIENTS";
        const string pageSql = @"SELECT ID AS Id, NAME AS Name, 'NotStarted' AS HandoverStatus, CAST(NULL AS VARCHAR(255)) AS HandoverId FROM (
          SELECT ID, NAME, ROW_NUMBER() OVER (ORDER BY ID) AS RN
          FROM PATIENTS
        ) WHERE RN BETWEEN :startRow AND :endRow";

        int total = await conn.ExecuteScalarAsync<int>(countSql);
        int startRow = ((p - 1) * ps) + 1;
        int endRow = p * ps;
        var items = await conn.QueryAsync<PatientRecord>(pageSql, new { startRow, endRow });
        return (items.ToList(), total);
    }

    public async Task<PatientDetailRecord?> GetPatientByIdAsync(string patientId)
    {
        using IDbConnection conn = _factory.CreateConnection();
        const string sql = @"SELECT ID AS Id, NAME AS Name, MRN, DOB AS Dob, GENDER AS Gender,
                            ADMISSION_DATE AS AdmissionDate, UNIT_ID AS CurrentUnit,
                            ROOM_NUMBER AS RoomNumber, DIAGNOSIS, ALLERGIES, MEDICATIONS, NOTES
                            FROM PATIENTS WHERE ID = :patientId";

        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { patientId });
        if (result == null) return null;

        return new PatientDetailRecord(
            Id: result.ID,
            Name: result.NAME ?? "",
            Mrn: result.MRN ?? "",
            Dob: result.DOB?.ToString("yyyy-MM-dd") ?? "",
            Gender: result.GENDER ?? "Unknown",
            AdmissionDate: result.ADMISSION_DATE?.ToString("yyyy-MM-dd") ?? "",
            CurrentUnit: result.CURRENTUNIT ?? "",
            RoomNumber: result.ROOMNUMBER ?? "",
            Diagnosis: result.DIAGNOSIS ?? "",
            Allergies: ParseArray(result.ALLERGIES),
            Medications: ParseArray(result.MEDICATIONS),
            Notes: result.NOTES ?? ""
        );
    }

    public async Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetMyHandoversAsync(string userId, int page, int pageSize)
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

        int total = await conn.ExecuteScalarAsync<int>(countSql, new { userId });

        if (total == 0)
            return (Array.Empty<HandoverRecord>(), 0);

        // Get handovers with pagination
        const string handoverSql = @"
          SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME, h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY,
                 h.SITUATION_AWARENESS_DOC_ID, h.SYNTHESIS, h.SHIFT_NAME, h.CREATED_BY, h.ASSIGNED_TO,
                 TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT
          FROM HANDOVERS h
          INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
          INNER JOIN USER_ASSIGNMENTS ua ON h.PATIENT_ID = ua.PATIENT_ID
          WHERE ua.USER_ID = :userId
          ORDER BY h.CREATED_AT DESC
          OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY";

        var handoverRows = await conn.QueryAsync(handoverSql, new { userId, offset, pageSize });
        var handovers = new List<HandoverRecord>();

        foreach (var row in handoverRows)
        {
            var handoverId = row.ID;

            // Get action items for the handover
            const string actionItemsSql = @"
                SELECT ID, DESCRIPTION, IS_COMPLETED
                FROM HANDOVER_ACTION_ITEMS
                WHERE HANDOVER_ID = :handoverId
                ORDER BY CREATED_AT";

            var actionItems = await conn.QueryAsync(actionItemsSql, new { handoverId });
            var actionItemsList = actionItems.Select(item => new HandoverActionItem(
                item.ID,
                item.DESCRIPTION,
                item.IS_COMPLETED == 1
            )).ToList();

            handovers.Add(new HandoverRecord(
                Id: row.ID,
                AssignmentId: row.ASSIGNMENT_ID,
                PatientId: row.PATIENT_ID,
                PatientName: row.PATIENT_NAME,
                Status: row.STATUS,
                IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
                PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
                ActionItems: actionItemsList,
                SituationAwarenessDocId: row.SITUATION_AWARENESS_DOC_ID,
                Synthesis: !string.IsNullOrEmpty(row.SYNTHESIS) ? new HandoverSynthesis(row.SYNTHESIS) : null,
                ShiftName: row.SHIFT_NAME ?? "Unknown",
                CreatedBy: row.CREATED_BY ?? "system",
                AssignedTo: row.ASSIGNED_TO ?? "system",
                CreatedAt: row.CREATED_AT
            ));
        }

        return (handovers, total);
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetMyPatientsAsync(string userId, int page, int pageSize)
    {
        using IDbConnection conn = _factory.CreateConnection();

        // Get total count of assigned patients
        const string countSql = "SELECT COUNT(*) FROM USER_ASSIGNMENTS WHERE USER_ID = :userId";
        int total = await conn.ExecuteScalarAsync<int>(countSql, new { userId });

        if (total == 0)
            return (Array.Empty<PatientRecord>(), 0);

        // Get assigned patients with pagination
        int p = Math.Max(page, 1);
        int ps = Math.Max(pageSize, 1);
        int offset = (p - 1) * ps;

        const string patientsSql = @"
          SELECT p.ID AS Id, p.NAME AS Name, 'NotStarted' AS HandoverStatus, CAST(NULL AS VARCHAR(255)) AS HandoverId
          FROM PATIENTS p
          INNER JOIN USER_ASSIGNMENTS ua ON p.ID = ua.PATIENT_ID
          WHERE ua.USER_ID = :userId
          ORDER BY p.ID
          OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY";

        var patients = await conn.QueryAsync<PatientRecord>(patientsSql, new { userId, offset, pageSize });
        return (patients.ToList(), total);
    }

    public async Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetPatientHandoversAsync(string patientId, int page, int pageSize)
    {
        using IDbConnection conn = _factory.CreateConnection();

        // Get total count
        const string countSql = "SELECT COUNT(1) FROM HANDOVERS WHERE PATIENT_ID = :patientId";
        int total = await conn.ExecuteScalarAsync<int>(countSql, new { patientId });

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
          WHERE h.PATIENT_ID = :patientId
          ORDER BY h.CREATED_AT DESC
          OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY";

        var handoverRows = await conn.QueryAsync(handoverSql, new { patientId, offset, pageSize });
        var handovers = new List<HandoverRecord>();

        foreach (var row in handoverRows)
        {
            var handoverId = row.ID;

            // Get action items for the handover
            const string actionItemsSql = @"
                SELECT ID, DESCRIPTION, IS_COMPLETED
                FROM HANDOVER_ACTION_ITEMS
                WHERE HANDOVER_ID = :handoverId
                ORDER BY CREATED_AT";

            var actionItems = await conn.QueryAsync(actionItemsSql, new { handoverId });
            var actionItemsList = actionItems.Select(item => new HandoverActionItem(
                item.ID,
                item.DESCRIPTION,
                item.IS_COMPLETED == 1
            )).ToList();

            handovers.Add(new HandoverRecord(
                Id: row.ID,
                AssignmentId: row.ASSIGNMENT_ID,
                PatientId: row.PATIENT_ID,
                PatientName: row.PATIENT_NAME,
                Status: row.STATUS,
                IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
                PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
                ActionItems: actionItemsList,
                SituationAwarenessDocId: row.SITUATION_AWARENESS_DOC_ID,
                Synthesis: !string.IsNullOrEmpty(row.SYNTHESIS) ? new HandoverSynthesis(row.SYNTHESIS) : null,
                ShiftName: row.SHIFT_NAME ?? "Unknown",
                CreatedBy: row.CREATED_BY ?? "system",
                AssignedTo: row.ASSIGNED_TO ?? "system",
                CreatedAt: row.CREATED_AT
            ));
        }

        return (handovers, total);
    }

    public async Task<HandoverRecord?> GetHandoverByIdAsync(string handoverId)
    {
        using IDbConnection conn = _factory.CreateConnection();

        const string handoverSql = @"
          SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME, h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY,
                 h.SITUATION_AWARENESS_DOC_ID, h.SYNTHESIS, h.SHIFT_NAME, h.CREATED_BY, h.ASSIGNED_TO,
                 TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT
          FROM HANDOVERS h
          INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
          WHERE h.ID = :handoverId";

        var row = await conn.QueryFirstOrDefaultAsync(handoverSql, new { handoverId });

        if (row == null)
            return null;

        // Get action items for the handover
        const string actionItemsSql = @"
            SELECT ID, DESCRIPTION, IS_COMPLETED
            FROM HANDOVER_ACTION_ITEMS
            WHERE HANDOVER_ID = :handoverId
            ORDER BY CREATED_AT";

        var actionItems = await conn.QueryAsync(actionItemsSql, new { handoverId });
        var actionItemsList = actionItems.Select(item => new HandoverActionItem(
            item.ID,
            item.DESCRIPTION,
            item.IS_COMPLETED == 1
        )).ToList();

        return new HandoverRecord(
            Id: row.ID,
            AssignmentId: row.ASSIGNMENT_ID,
            PatientId: row.PATIENT_ID,
            PatientName: row.PATIENT_NAME,
            Status: row.STATUS,
            IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
            PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
            ActionItems: actionItemsList,
            SituationAwarenessDocId: row.SITUATION_AWARENESS_DOC_ID,
            Synthesis: !string.IsNullOrEmpty(row.SYNTHESIS) ? new HandoverSynthesis(row.SYNTHESIS) : null,
            ShiftName: row.SHIFT_NAME ?? "Unknown",
            CreatedBy: row.CREATED_BY ?? "system",
            AssignedTo: row.ASSIGNED_TO ?? "system",
            CreatedAt: row.CREATED_AT
        );
    }

    public async Task<HandoverRecord?> GetActiveHandoverAsync(string userId)
    {
        using IDbConnection conn = _factory.CreateConnection();

        // Find the active handover for this user's assigned patients
        const string handoverSql = @"
            SELECT * FROM (
                SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME, h.STATUS, h.ILLNESS_SEVERITY, h.PATIENT_SUMMARY,
                       h.SITUATION_AWARENESS_DOC_ID, h.SYNTHESIS, h.SHIFT_NAME, h.CREATED_BY, h.ASSIGNED_TO,
                       TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT
                FROM HANDOVERS h
                INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
                WHERE h.ASSIGNED_TO = :userId
                  AND h.STATUS IN ('Active', 'InProgress')
                ORDER BY h.CREATED_AT DESC
            ) WHERE ROWNUM = 1";

        var row = await conn.QueryFirstOrDefaultAsync(handoverSql, new { userId });

        if (row == null)
            return null;

        // Get action items for the handover
        const string actionItemsSql = @"
            SELECT ID, DESCRIPTION, IS_COMPLETED
            FROM HANDOVER_ACTION_ITEMS
            WHERE HANDOVER_ID = :handoverId
            ORDER BY CREATED_AT";

        var actionItems = await conn.QueryAsync(actionItemsSql, new { handoverId = row.ID });
        var actionItemsList = actionItems.Select(item => new HandoverActionItem(
            item.ID,
            item.DESCRIPTION,
            item.IS_COMPLETED == 1
        )).ToList();

        return new HandoverRecord(
            Id: row.ID,
            AssignmentId: row.ASSIGNMENT_ID,
            PatientId: row.PATIENT_ID,
            PatientName: row.PATIENT_NAME,
            Status: row.STATUS,
            IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
            PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
            ActionItems: actionItemsList,
            SituationAwarenessDocId: row.SITUATION_AWARENESS_DOC_ID,
            Synthesis: !string.IsNullOrEmpty(row.SYNTHESIS) ? new HandoverSynthesis(row.SYNTHESIS) : null,
            ShiftName: row.SHIFT_NAME ?? "Unknown",
            CreatedBy: row.CREATED_BY ?? "system",
            AssignedTo: row.ASSIGNED_TO ?? "system",
            CreatedAt: row.CREATED_AT
        );
    }

    public async Task<IReadOnlyList<HandoverParticipantRecord>> GetHandoverParticipantsAsync(string handoverId)
    {
        using IDbConnection conn = _factory.CreateConnection();

        const string sql = @"
            SELECT ID AS Id, USER_ID AS UserId, USER_NAME AS UserName, USER_ROLE AS UserRole,
                   STATUS, JOINED_AT AS JoinedAt, LAST_ACTIVITY AS LastActivity
            FROM HANDOVER_PARTICIPANTS
            WHERE HANDOVER_ID = :handoverId
            ORDER BY JOINED_AT";

        var participants = await conn.QueryAsync<HandoverParticipantRecord>(sql, new { handoverId });
        var participantsList = participants.ToList();

        // If no participants found, return a default list with the assigned user
        if (!participantsList.Any())
        {
            // Get the handover creator as the default participant
            const string creatorSql = @"
                SELECT ASSIGNED_TO as UserId, 'Assigned Physician' as UserName, 'Doctor' as UserRole
                FROM HANDOVERS
                WHERE ID = :handoverId";

            var creator = await conn.QueryFirstOrDefaultAsync(creatorSql, new { handoverId });

            if (creator != null)
            {
                participantsList.Add(new HandoverParticipantRecord(
                    Id: $"participant-{handoverId}-default",
                    UserId: creator.USER_ID,
                    UserName: creator.USER_NAME ?? "Assigned Physician",
                    UserRole: creator.USER_ROLE,
                    Status: "active",
                    JoinedAt: DateTime.Now,
                    LastActivity: DateTime.Now
                ));
            }
        }

        return participantsList;
    }

    public async Task<IReadOnlyList<HandoverSectionRecord>> GetHandoverSectionsAsync(string handoverId)
    {
        using IDbConnection conn = _factory.CreateConnection();

        const string sql = @"
            SELECT ID AS Id, SECTION_TYPE AS SectionType, CONTENT, STATUS, LAST_EDITED_BY AS LastEditedBy,
                   CREATED_AT AS CreatedAt, UPDATED_AT AS UpdatedAt
            FROM HANDOVER_SECTIONS
            WHERE HANDOVER_ID = :handoverId
            ORDER BY CREATED_AT";

        var sections = await conn.QueryAsync<HandoverSectionRecord>(sql, new { handoverId });
        var sectionsList = sections.ToList();

        // If no sections exist, create default empty sections
        if (!sectionsList.Any())
        {
            sectionsList = new List<HandoverSectionRecord>
            {
                new HandoverSectionRecord(
                    Id: $"section-{handoverId}-severity",
                    SectionType: "illness_severity",
                    Content: "Stable",
                    Status: "draft",
                    LastEditedBy: null,
                    CreatedAt: DateTime.Now,
                    UpdatedAt: DateTime.Now
                ),
                new HandoverSectionRecord(
                    Id: $"section-{handoverId}-summary",
                    SectionType: "patient_summary",
                    Content: "",
                    Status: "draft",
                    LastEditedBy: null,
                    CreatedAt: DateTime.Now,
                    UpdatedAt: DateTime.Now
                ),
                new HandoverSectionRecord(
                    Id: $"section-{handoverId}-actions",
                    SectionType: "action_items",
                    Content: "",
                    Status: "draft",
                    LastEditedBy: null,
                    CreatedAt: DateTime.Now,
                    UpdatedAt: DateTime.Now
                ),
                new HandoverSectionRecord(
                    Id: $"section-{handoverId}-awareness",
                    SectionType: "situation_awareness",
                    Content: "",
                    Status: "draft",
                    LastEditedBy: null,
                    CreatedAt: DateTime.Now,
                    UpdatedAt: DateTime.Now
                ),
                new HandoverSectionRecord(
                    Id: $"section-{handoverId}-synthesis",
                    SectionType: "synthesis",
                    Content: "",
                    Status: "draft",
                    LastEditedBy: null,
                    CreatedAt: DateTime.Now,
                    UpdatedAt: DateTime.Now
                )
            };
        }

        return sectionsList;
    }

    public async Task<HandoverSyncStatusRecord?> GetHandoverSyncStatusAsync(string handoverId, string userId)
    {
        using IDbConnection conn = _factory.CreateConnection();

        const string sql = @"
            SELECT ID AS Id, SYNC_STATUS AS SyncStatus, LAST_SYNC AS LastSync, VERSION
            FROM HANDOVER_SYNC_STATUS
            WHERE HANDOVER_ID = :handoverId AND USER_ID = :userId";

        var syncStatus = await conn.QueryFirstOrDefaultAsync<HandoverSyncStatusRecord>(sql, new { handoverId, userId });

        // If no sync status exists, create a default one
        if (syncStatus == null)
        {
            syncStatus = new HandoverSyncStatusRecord(
                Id: $"sync-{handoverId}-{userId}",
                SyncStatus: "synced",
                LastSync: DateTime.Now,
                Version: 1
            );
        }

        return syncStatus;
    }

    public async Task<UserPreferencesRecord?> GetUserPreferencesAsync(string userId)
    {
        using IDbConnection conn = _factory.CreateConnection();

        const string sql = @"
            SELECT ID AS Id, USER_ID AS UserId, THEME, LANGUAGE, TIMEZONE, NOTIFICATIONS_ENABLED AS NotificationsEnabled,
                   AUTO_SAVE_ENABLED AS AutoSaveEnabled, CREATED_AT AS CreatedAt, UPDATED_AT AS UpdatedAt
            FROM USER_PREFERENCES
            WHERE USER_ID = :userId";

        return await conn.QueryFirstOrDefaultAsync<UserPreferencesRecord>(sql, new { userId });
    }

    public async Task<IReadOnlyList<UserSessionRecord>> GetUserSessionsAsync(string userId)
    {
        using IDbConnection conn = _factory.CreateConnection();

        const string sql = @"
            SELECT ID AS Id, USER_ID AS UserId, SESSION_START AS SessionStart, SESSION_END AS SessionEnd,
                   IP_ADDRESS AS IpAddress, USER_AGENT AS UserAgent, IS_ACTIVE AS IsActive
            FROM USER_SESSIONS
            WHERE USER_ID = :userId
            ORDER BY SESSION_START DESC";

        var sessions = await conn.QueryAsync<UserSessionRecord>(sql, new { userId });
        return sessions.ToList();
    }

    // ISetupCommandService implementation
    public async Task<IReadOnlyList<string>> AssignPatientsAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        return await AssignAsync(userId, shiftId, patientIds);
    }


    public async Task<bool> UpdateHandoverSectionAsync(string handoverId, string sectionId, string content, string status, string userId)
    {
        return await Task.FromResult(UpdateHandoverSection(handoverId, sectionId, content, status, userId));
    }

    public async Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferencesRecord preferences)
    {
        return await Task.FromResult(UpdateUserPreferences(userId, preferences));
    }

    // Helper method
    private IReadOnlyList<string> ParseArray(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return Array.Empty<string>();

        // Simple parsing - assuming comma-separated values
        return value.Split(',').Select(s => s.Trim()).ToList();
    }
}
