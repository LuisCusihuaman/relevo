using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;
using System.Collections.Generic;
using Relevo.Core.Exceptions;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleHandoverRepository : IHandoverRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleHandoverRepository> _logger;

    public OracleHandoverRepository(IOracleConnectionFactory factory, ILogger<OracleHandoverRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetMyHandovers(string userId, int page, int pageSize)
    {
        using IDbConnection conn = _factory.CreateConnection();

        // Get total count
        const string countSql = @"
            SELECT COUNT(1)
            FROM HANDOVERS h
            INNER JOIN USER_ASSIGNMENTS ua ON h.PATIENT_ID = ua.PATIENT_ID
            WHERE ua.USER_ID = :userId";

        int total = conn.ExecuteScalar<int>(countSql, new { userId });

        if (total == 0)
            return (Array.Empty<HandoverRecord>(), 0);

        int p = Math.Max(page, 1);
        int ps = Math.Max(pageSize, 1);
        int offset = (p - 1) * ps;
        int maxRow = p * ps;

        // Get handovers with pagination using ROWNUM for Oracle 11g
        const string handoverSql = @"
          SELECT * FROM (
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
                   h.VERSION,
                   ROWNUM as rn
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
          ) WHERE rn > :offset AND rn <= :maxRow";

        var handoverRows = conn.Query(handoverSql, new { userId, offset, maxRow }).ToList();

        var handovers = new List<HandoverRecord>();
        foreach (var row in handoverRows)
        {
            string? synthesisContent = row.SYNTHESIS_CONTENT as string;
            
            var handover = new HandoverRecord(
                Id: (string)row.ID,
                AssignmentId: (string)row.ASSIGNMENT_ID,
                PatientId: (string)row.PATIENT_ID,
                PatientName: row.PATIENT_NAME as string,
                Status: (string)row.STATUS,
                IllnessSeverity: new HandoverIllnessSeverity((row.ILLNESS_SEVERITY as string) ?? "Stable"),
                PatientSummary: new HandoverPatientSummary((row.PATIENT_SUMMARY as string) ?? ""),
                SituationAwarenessDocId: row.SITUATION_AWARENESS_EDITOR as string,
                Synthesis: !string.IsNullOrEmpty(synthesisContent) ? new HandoverSynthesis(synthesisContent) : null,
                ShiftName: (row.SHIFT_NAME as string) ?? "Unknown",
                CreatedBy: (row.CREATED_BY as string) ?? "system",
                AssignedTo: (row.ASSIGNED_TO as string) ?? "system",
                CreatedByName: row.CREATED_BY_NAME as string,
                AssignedToName: row.ASSIGNED_TO_NAME as string,
                ReceiverUserId: row.RECEIVER_USER_ID as string,
                ResponsiblePhysicianId: (row.RESPONSIBLE_PHYSICIAN_ID as string) ?? "system",
                ResponsiblePhysicianName: (row.RESPONSIBLE_PHYSICIAN_NAME as string) ?? "Unknown",
                CreatedAt: row.CREATED_AT as string,
                ReadyAt: row.READY_AT as string,
                StartedAt: row.STARTED_AT as string,
                AcknowledgedAt: row.ACKNOWLEDGED_AT as string,
                AcceptedAt: row.ACCEPTED_AT as string,
                CompletedAt: row.COMPLETED_AT as string,
                CancelledAt: row.CANCELLED_AT as string,
                RejectedAt: row.REJECTED_AT as string,
                RejectionReason: row.REJECTION_REASON as string,
                ExpiredAt: row.EXPIRED_AT as string,
                HandoverType: (row.HANDOVER_TYPE as string) ?? "ShiftToShift",
                HandoverWindowDate: row.HANDOVER_WINDOW_DATE as DateTime?,
                FromShiftId: row.FROM_SHIFT_ID as string,
                ToShiftId: row.TO_SHIFT_ID as string,
                ToDoctorId: row.TO_DOCTOR_ID as string,
                StateName: (row.STATENAME as string) ?? "Draft",
                Version: (row.VERSION as int?) ?? 1
            );
            handovers.Add(handover);
        }

        return (handovers, total);
    }

    public (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetPatientHandovers(string patientId, int page, int pageSize)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            // First, get the total count
            const string countSql = @"
                SELECT COUNT(*) 
                FROM HANDOVERS h
                WHERE h.PATIENT_ID = :patientId";
            
            var totalCount = conn.ExecuteScalar<int>(countSql, new { patientId });

            // If no handovers, return empty list
            if (totalCount == 0)
            {
                return (new List<HandoverRecord>(), 0);
            }

            // Calculate offset for pagination
            var offset = (page - 1) * pageSize;
            var maxRow = page * pageSize;

            // Get the handovers for this page using ROWNUM for Oracle 11g
            const string handoverSql = @"
                SELECT * FROM (
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
                           h.VERSION,
                           ROWNUM as rn
                    FROM HANDOVERS h
                    LEFT JOIN PATIENTS p ON h.PATIENT_ID = p.ID
                    LEFT JOIN VW_HANDOVERS_STATE vws ON h.ID = vws.HandoverId
                    LEFT JOIN USERS cb ON h.CREATED_BY = cb.ID
                    LEFT JOIN USERS td ON h.TO_DOCTOR_ID = td.ID
                    LEFT JOIN USERS rp ON rp.ID = COALESCE(h.RESPONSIBLE_PHYSICIAN_ID, h.CREATED_BY)
                    LEFT JOIN HANDOVER_PATIENT_DATA pd ON h.ID = pd.HANDOVER_ID
                    LEFT JOIN HANDOVER_SITUATION_AWARENESS sa ON h.ID = sa.HANDOVER_ID
                    LEFT JOIN HANDOVER_SYNTHESIS syn ON h.ID = syn.HANDOVER_ID
                    WHERE h.PATIENT_ID = :patientId
                    ORDER BY h.CREATED_AT DESC
                ) WHERE rn > :offset AND rn <= :maxRow";

            var rows = conn.Query(handoverSql, new { patientId, offset, maxRow });

            var handovers = new List<HandoverRecord>();
            foreach (var row in rows)
            {
                // Safely extract values from dynamic row, handling DBNull
                string? synthesisContent = row.SYNTHESIS_CONTENT as string;
                
                var handover = new HandoverRecord(
                    Id: (string)row.ID,
                    AssignmentId: (string)row.ASSIGNMENT_ID,
                    PatientId: (string)row.PATIENT_ID,
                    PatientName: row.PATIENT_NAME as string,
                    Status: (string)row.STATUS,
                    IllnessSeverity: new HandoverIllnessSeverity((row.ILLNESS_SEVERITY as string) ?? "Stable"),
                    PatientSummary: new HandoverPatientSummary((row.PATIENT_SUMMARY as string) ?? ""),
                    SituationAwarenessDocId: row.SITUATION_AWARENESS_EDITOR as string,
                    Synthesis: !string.IsNullOrEmpty(synthesisContent) ? new HandoverSynthesis(synthesisContent) : null,
                    ShiftName: (row.SHIFT_NAME as string) ?? "Unknown",
                    CreatedBy: (row.CREATED_BY as string) ?? "system",
                    AssignedTo: (row.ASSIGNED_TO as string) ?? "system",
                    CreatedByName: row.CREATED_BY_NAME as string,
                    AssignedToName: row.ASSIGNED_TO_NAME as string,
                    ReceiverUserId: row.RECEIVER_USER_ID as string,
                    ResponsiblePhysicianId: (row.RESPONSIBLE_PHYSICIAN_ID as string) ?? "system",
                    ResponsiblePhysicianName: (row.RESPONSIBLE_PHYSICIAN_NAME as string) ?? "Unknown",
                    CreatedAt: row.CREATED_AT as string,
                    ReadyAt: row.READY_AT as string,
                    StartedAt: row.STARTED_AT as string,
                    AcknowledgedAt: row.ACKNOWLEDGED_AT as string,
                    AcceptedAt: row.ACCEPTED_AT as string,
                    CompletedAt: row.COMPLETED_AT as string,
                    CancelledAt: row.CANCELLED_AT as string,
                    RejectedAt: row.REJECTED_AT as string,
                    RejectionReason: row.REJECTION_REASON as string,
                    ExpiredAt: row.EXPIRED_AT as string,
                    HandoverType: (row.HANDOVER_TYPE as string) ?? "ShiftToShift",
                    HandoverWindowDate: row.HANDOVER_WINDOW_DATE as DateTime?,
                    FromShiftId: row.FROM_SHIFT_ID as string,
                    ToShiftId: row.TO_SHIFT_ID as string,
                    ToDoctorId: row.TO_DOCTOR_ID as string,
                    StateName: (row.STATENAME as string) ?? "Draft",
                    Version: (row.VERSION as int?) ?? 1
                );

                handovers.Add(handover);
            }

            return (handovers, totalCount);
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

            // Safely extract values from dynamic row, handling DBNull
            string? synthesisContent = row.SYNTHESIS_CONTENT as string;

            return new HandoverRecord(
                Id: (string)row.ID,
                AssignmentId: (string)row.ASSIGNMENT_ID,
                PatientId: (string)row.PATIENT_ID,
                PatientName: row.PATIENT_NAME as string,
                Status: (string)row.STATUS,
                IllnessSeverity: new HandoverIllnessSeverity((row.ILLNESS_SEVERITY as string) ?? "Stable"),
                PatientSummary: new HandoverPatientSummary((row.PATIENT_SUMMARY as string) ?? ""),
                SituationAwarenessDocId: row.SITUATION_AWARENESS_EDITOR as string,
                Synthesis: !string.IsNullOrEmpty(synthesisContent) ? new HandoverSynthesis(synthesisContent) : null,
                ShiftName: (row.SHIFT_NAME as string) ?? "Unknown",
                CreatedBy: (row.CREATED_BY as string) ?? "system",
                AssignedTo: (row.ASSIGNED_TO as string) ?? "system",
                CreatedByName: row.CREATED_BY_NAME as string,
                AssignedToName: row.ASSIGNED_TO_NAME as string,
                ReceiverUserId: row.RECEIVER_USER_ID as string,
                ResponsiblePhysicianId: (row.RESPONSIBLE_PHYSICIAN_ID as string) ?? "system",
                ResponsiblePhysicianName: (row.RESPONSIBLE_PHYSICIAN_NAME as string) ?? "Unknown",
                CreatedAt: row.CREATED_AT as string,
                ReadyAt: row.READY_AT as string,
                StartedAt: row.STARTED_AT as string,
                AcknowledgedAt: row.ACKNOWLEDGED_AT as string,
                AcceptedAt: row.ACCEPTED_AT as string,
                CompletedAt: row.COMPLETED_AT as string,
                CancelledAt: row.CANCELLED_AT as string,
                RejectedAt: row.REJECTED_AT as string,
                RejectionReason: row.REJECTION_REASON as string,
                ExpiredAt: row.EXPIRED_AT as string,
                HandoverType: (row.HANDOVER_TYPE as string) ?? "ShiftToShift",
                HandoverWindowDate: row.HANDOVER_WINDOW_DATE as DateTime?,
                FromShiftId: row.FROM_SHIFT_ID as string,
                ToShiftId: row.TO_SHIFT_ID as string,
                ToDoctorId: row.TO_DOCTOR_ID as string,
                StateName: (row.STATENAME as string) ?? "Draft",
                Version: (row.VERSION as int?) ?? 1
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get handover {HandoverId}", handoverId);
            throw;
        }
    }

    public async Task CreateHandoverForAssignmentAsync(string assignmentId, string userId, string userName, DateTime windowDate, string fromShiftId, string toShiftId)
    {
        try
        {
            _logger.LogInformation("Starting handover creation for assignment {AssignmentId}, user {UserId}, window {WindowDate}, from {FromShiftId} to {ToShiftId}",
                assignmentId, userId, windowDate, fromShiftId, toShiftId);

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

    public async Task<bool> StartHandover(string handoverId, string userId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

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
}
