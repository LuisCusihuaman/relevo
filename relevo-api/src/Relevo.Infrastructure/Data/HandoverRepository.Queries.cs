using Dapper;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

/// <summary>
/// Handover read operations - queries for retrieving handover data.
/// </summary>
public partial class HandoverRepository
{
    public async Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetPatientHandoversAsync(string patientId, int page, int pageSize)
    {
        Console.WriteLine($"[GetPatientHandoversAsync] Starting query for PatientId: {patientId}, Page: {page}, PageSize: {pageSize}");
        
        using var conn = _connectionFactory.CreateConnection();

        var p = Math.Max(page, 1);
        var ps = Math.Max(pageSize, 1);
        var offset = (p - 1) * ps;

        // Count
        var total = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM HANDOVERS WHERE PATIENT_ID = :PatientId",
            new { PatientId = patientId });

        Console.WriteLine($"[GetPatientHandoversAsync] Total handovers found for PatientId {patientId}: {total}");

        if (total == 0)
            return (Array.Empty<HandoverRecord>(), 0);

        // Query - V3 Schema: Uses SHIFT_WINDOW_ID, SENDER_USER_ID, RECEIVER_USER_ID, etc.
        const string sql = @"
            SELECT * FROM (
                SELECT
                    h.ID,
                    h.PATIENT_ID as PatientId,
                    p.NAME as PatientName,
                    h.CURRENT_STATE as Status,
                    COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
                    hc.PATIENT_SUMMARY as PatientSummary,
                    h.ID || '-sa' as SituationAwarenessDocId,
                    hc.SYNTHESIS as Synthesis,
                    s_from.NAME as ShiftName,
                    h.CREATED_BY_USER_ID as CreatedBy,
                    COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) as AssignedTo,
                    u_created.FULL_NAME as CreatedByName,
                    u_assigned.FULL_NAME as AssignedToName,
                    h.RECEIVER_USER_ID as ReceiverUserId,
                    h.SENDER_USER_ID as ResponsiblePhysicianId,
                    u_sender.FULL_NAME as ResponsiblePhysicianName,
                    TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
                    TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
                    TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
                    TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
                    TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
                    TRUNC(si_from.START_AT) as HandoverWindowDate,
                    h.CURRENT_STATE as StateName,
                    1 as Version,
                    h.SHIFT_WINDOW_ID as ShiftWindowId,
                    h.PREVIOUS_HANDOVER_ID as PreviousHandoverId,
                    h.SENDER_USER_ID as SenderUserId,
                    h.READY_BY_USER_ID as ReadyByUserId,
                    h.STARTED_BY_USER_ID as StartedByUserId,
                    h.COMPLETED_BY_USER_ID as CompletedByUserId,
                    h.CANCELLED_BY_USER_ID as CancelledByUserId,
                    h.CANCEL_REASON as CancelReason,
                    ROW_NUMBER() OVER (ORDER BY h.CREATED_AT DESC) AS RN
                FROM HANDOVERS h
                JOIN PATIENTS p ON h.PATIENT_ID = p.ID
                LEFT JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID
                LEFT JOIN SHIFT_INSTANCES si_from ON sw.FROM_SHIFT_INSTANCE_ID = si_from.ID
                LEFT JOIN SHIFT_INSTANCES si_to ON sw.TO_SHIFT_INSTANCE_ID = si_to.ID
                LEFT JOIN SHIFTS s_from ON si_from.SHIFT_ID = s_from.ID
                LEFT JOIN SHIFTS s_to ON si_to.SHIFT_ID = s_to.ID
                LEFT JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID
                LEFT JOIN USERS u_created ON h.CREATED_BY_USER_ID = u_created.ID
                LEFT JOIN USERS u_assigned ON COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) = u_assigned.ID
                LEFT JOIN USERS u_sender ON h.SENDER_USER_ID = u_sender.ID
                WHERE h.PATIENT_ID = :PatientId
            )
            WHERE RN BETWEEN :StartRow AND :EndRow";

        var handovers = await conn.QueryAsync<HandoverRecord>(sql, new { PatientId = patientId, StartRow = offset + 1, EndRow = offset + ps });

        return (handovers.ToList(), total);
    }

    public async Task<HandoverDetailRecord?> GetHandoverByIdAsync(string handoverId)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                h.ID,
                h.PATIENT_ID as PatientId,
                p.NAME as PatientName,
                h.CURRENT_STATE as Status,
                COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
                hc.PATIENT_SUMMARY as PatientSummary,
                h.ID || '-sa' as SituationAwarenessDocId,
                hc.SYNTHESIS as Synthesis,
                s_from.NAME as ShiftName,
                h.CREATED_BY_USER_ID as CreatedBy,
                COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) as AssignedTo,
                NULL as CreatedByName,
                NULL as AssignedToName,
                h.RECEIVER_USER_ID as ReceiverUserId,
                h.SENDER_USER_ID as ResponsiblePhysicianId,
                NULL as ResponsiblePhysicianName,
                TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
                TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
                TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
                TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
                TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
                TRUNC(si_from.START_AT) as HandoverWindowDate,
                h.CURRENT_STATE as StateName,
                1 as Version,
                h.SHIFT_WINDOW_ID as ShiftWindowId,
                h.PREVIOUS_HANDOVER_ID as PreviousHandoverId,
                h.SENDER_USER_ID as SenderUserId,
                h.READY_BY_USER_ID as ReadyByUserId,
                h.STARTED_BY_USER_ID as StartedByUserId,
                h.COMPLETED_BY_USER_ID as CompletedByUserId,
                h.CANCELLED_BY_USER_ID as CancelledByUserId,
                h.CANCEL_REASON as CancelReason
            FROM HANDOVERS h
            JOIN PATIENTS p ON h.PATIENT_ID = p.ID
            LEFT JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID
            LEFT JOIN SHIFT_INSTANCES si_from ON sw.FROM_SHIFT_INSTANCE_ID = si_from.ID
            LEFT JOIN SHIFT_INSTANCES si_to ON sw.TO_SHIFT_INSTANCE_ID = si_to.ID
            LEFT JOIN SHIFTS s_from ON si_from.SHIFT_ID = s_from.ID
            LEFT JOIN SHIFTS s_to ON si_to.SHIFT_ID = s_to.ID
            LEFT JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID
            WHERE h.ID = :HandoverId";

        var handover = await conn.QueryFirstOrDefaultAsync<HandoverRecord>(sql, new { HandoverId = handoverId });

        if (handover == null)
        {
            return null;
        }

        const string sqlActionItems = @"
            SELECT
                ID,
                DESCRIPTION,
                IS_COMPLETED as IsCompleted
            FROM HANDOVER_ACTION_ITEMS
            WHERE HANDOVER_ID = :HandoverId";

        var actionItems = (await conn.QueryAsync<ActionItemRecord>(sqlActionItems, new { HandoverId = handoverId })).ToList();

        return new HandoverDetailRecord(handover, actionItems);
    }

    public async Task<PatientHandoverDataRecord?> GetPatientHandoverDataAsync(string handoverId)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                h.ID,
                h.PATIENT_ID,
                h.CURRENT_STATE as STATUS,
                h.SENDER_USER_ID,
                COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) as RECEIVER_USER_ID,
                h.SHIFT_WINDOW_ID,
                p.NAME,
                TO_CHAR(p.DATE_OF_BIRTH, 'YYYY-MM-DD') as Dob,
                p.MRN,
                TO_CHAR(p.ADMISSION_DATE, 'YYYY-MM-DD HH24:MI:SS') as AdmissionDate,
                p.ROOM_NUMBER,
                p.DIAGNOSIS,
                u.NAME as UnitName,
                hc.ILLNESS_SEVERITY,
                hc.PATIENT_SUMMARY as SUMMARY_TEXT,
                hc.LAST_EDITED_BY,
                TO_CHAR(hc.UPDATED_AT, 'YYYY-MM-DD HH24:MI:SS') as UpdatedAt
            FROM HANDOVERS h
            JOIN PATIENTS p ON h.PATIENT_ID = p.ID
            JOIN UNITS u ON p.UNIT_ID = u.ID
            LEFT JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID
            WHERE h.ID = :HandoverId";

        var data = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { HandoverId = handoverId });

        if (data == null) return null;

        string? senderUserId = (string?)data.SENDER_USER_ID;
        string? receiverUserId = (string?)data.RECEIVER_USER_ID;
        string patientId = (string)data.PATIENT_ID;
        string? shiftWindowId = (string?)data.SHIFT_WINDOW_ID;
        
        // V3: AssignedPhysician = Sender (responsible from FROM shift)
        PhysicianRecord? assignedPhysician = null;
        if (!string.IsNullOrEmpty(senderUserId))
        {
            assignedPhysician = await GetPhysicianInfo(conn, senderUserId, (string)data.STATUS ?? "Draft", "creator");
        }

        // V3: ReceivingPhysician = Receiver (from RECEIVER_USER_ID or lookup from TO shift coverage)
        PhysicianRecord? receivingPhysician = null;
        if (!string.IsNullOrEmpty(receiverUserId))
        {
            receivingPhysician = await GetPhysicianInfo(conn, receiverUserId, (string)data.STATUS ?? "Draft", "assignee");
        }
        else if (!string.IsNullOrEmpty(shiftWindowId))
        {
            // Try to find someone with coverage in TO shift (potential receiver)
            const string toShiftCoverageSql = @"
                SELECT sc.RESPONSIBLE_USER_ID
                FROM SHIFT_WINDOWS sw
                JOIN SHIFT_COVERAGE sc ON sw.TO_SHIFT_INSTANCE_ID = sc.SHIFT_INSTANCE_ID
                    AND sc.PATIENT_ID = :patientId
                WHERE sw.ID = :shiftWindowId
                AND ROWNUM <= 1
                ORDER BY sc.IS_PRIMARY DESC, sc.ASSIGNED_AT ASC";
            
            var potentialReceiverId = await conn.ExecuteScalarAsync<string>(toShiftCoverageSql, new { patientId, shiftWindowId });
            if (!string.IsNullOrEmpty(potentialReceiverId))
            {
                receivingPhysician = await GetPhysicianInfo(conn, potentialReceiverId, (string)data.STATUS ?? "Draft", "assignee");
            }
        }

        return new PatientHandoverDataRecord(
            (string)data.PATIENT_ID,
            (string)data.NAME,
            (string)data.DOB,
            (string?)data.MRN ?? "",
            (string?)data.ADMISSIONDATE ?? "",
            (string)data.UNITNAME,
            (string?)data.DIAGNOSIS ?? "",
            (string?)data.ROOM_NUMBER ?? "",
            (string)data.UNITNAME,
            assignedPhysician,
            receivingPhysician,
            (string?)data.ILLNESS_SEVERITY,
            (string?)data.SUMMARY_TEXT,
            (string?)data.LAST_EDITED_BY,
            (string?)data.UPDATEDAT
        );
    }

    public async Task<IReadOnlyList<HandoverRecord>> GetPendingHandoversAsync(string userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT
                h.ID,
                h.PATIENT_ID as PatientId,
                p.NAME as PatientName,
                h.CURRENT_STATE as Status,
                COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
                hc.PATIENT_SUMMARY as PatientSummary,
                h.ID || '-sa' as SituationAwarenessDocId,
                hc.SYNTHESIS as Synthesis,
                s_from.NAME as ShiftName,
                h.CREATED_BY_USER_ID as CreatedBy,
                COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) as AssignedTo,
                NULL as CreatedByName,
                NULL as AssignedToName,
                h.RECEIVER_USER_ID as ReceiverUserId,
                h.SENDER_USER_ID as ResponsiblePhysicianId,
                NULL as ResponsiblePhysicianName,
                TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
                TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
                TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
                TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
                TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
                TRUNC(si_from.START_AT) as HandoverWindowDate,
                h.CURRENT_STATE as StateName,
                1 as Version,
                h.SHIFT_WINDOW_ID as ShiftWindowId,
                h.PREVIOUS_HANDOVER_ID as PreviousHandoverId,
                h.SENDER_USER_ID as SenderUserId,
                h.READY_BY_USER_ID as ReadyByUserId,
                h.STARTED_BY_USER_ID as StartedByUserId,
                h.COMPLETED_BY_USER_ID as CompletedByUserId,
                h.CANCELLED_BY_USER_ID as CancelledByUserId,
                h.CANCEL_REASON as CancelReason
            FROM HANDOVERS h
            JOIN PATIENTS p ON h.PATIENT_ID = p.ID
            LEFT JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID
            LEFT JOIN SHIFT_INSTANCES si_from ON sw.FROM_SHIFT_INSTANCE_ID = si_from.ID
            LEFT JOIN SHIFT_INSTANCES si_to ON sw.TO_SHIFT_INSTANCE_ID = si_to.ID
            LEFT JOIN SHIFTS s_from ON si_from.SHIFT_ID = s_from.ID
            LEFT JOIN SHIFTS s_to ON si_to.SHIFT_ID = s_to.ID
            LEFT JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID
            LEFT JOIN SHIFT_COVERAGE sc_to ON sw.TO_SHIFT_INSTANCE_ID = sc_to.SHIFT_INSTANCE_ID 
                 AND h.PATIENT_ID = sc_to.PATIENT_ID
                 AND sc_to.RESPONSIBLE_USER_ID = :userId
            WHERE (h.RECEIVER_USER_ID = :userId 
                OR h.COMPLETED_BY_USER_ID = :userId 
                OR h.SENDER_USER_ID = :userId
                OR sc_to.ID IS NOT NULL)
              AND h.CURRENT_STATE IN ('Draft', 'Ready', 'InProgress')";

        var handovers = await conn.QueryAsync<HandoverRecord>(sql, new { userId });
        return handovers.ToList();
    }

    public async Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetMyHandoversAsync(string userId, int page, int pageSize)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string countSql = @"
            SELECT COUNT(1)
            FROM HANDOVERS h
            LEFT JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID
            LEFT JOIN SHIFT_COVERAGE sc_to ON sw.TO_SHIFT_INSTANCE_ID = sc_to.SHIFT_INSTANCE_ID 
                 AND h.PATIENT_ID = sc_to.PATIENT_ID
                 AND sc_to.RESPONSIBLE_USER_ID = :userId
            WHERE h.SENDER_USER_ID = :userId 
               OR h.RECEIVER_USER_ID = :userId 
               OR h.CREATED_BY_USER_ID = :userId 
               OR h.COMPLETED_BY_USER_ID = :userId
               OR h.READY_BY_USER_ID = :userId
               OR h.STARTED_BY_USER_ID = :userId
               OR h.CANCELLED_BY_USER_ID = :userId
               OR sc_to.ID IS NOT NULL";

        var total = await conn.ExecuteScalarAsync<int>(countSql, new { userId });

        if (total == 0)
            return (Array.Empty<HandoverRecord>(), 0);

        var p = Math.Max(page, 1);
        var ps = Math.Max(pageSize, 1);
        var offset = (p - 1) * ps;

        const string sql = @"
            SELECT * FROM (
                SELECT
                    h.ID,
                    h.PATIENT_ID as PatientId,
                    pt.NAME as PatientName,
                    h.CURRENT_STATE as Status,
                    COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
                    hc.PATIENT_SUMMARY as PatientSummary,
                    h.ID || '-sa' as SituationAwarenessDocId,
                    hc.SYNTHESIS as Synthesis,
                    s_from.NAME as ShiftName,
                    h.CREATED_BY_USER_ID as CreatedBy,
                    COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) as AssignedTo,
                    cb.FULL_NAME as CreatedByName,
                    td.FULL_NAME as AssignedToName,
                    h.RECEIVER_USER_ID as ReceiverUserId,
                    h.SENDER_USER_ID as ResponsiblePhysicianId,
                    rp.FULL_NAME as ResponsiblePhysicianName,
                    TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
                    TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
                    TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
                    TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
                    TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
                    TRUNC(si_from.START_AT) as HandoverWindowDate,
                    h.CURRENT_STATE as StateName,
                    1 as Version,
                    h.SHIFT_WINDOW_ID as ShiftWindowId,
                    h.PREVIOUS_HANDOVER_ID as PreviousHandoverId,
                    h.SENDER_USER_ID as SenderUserId,
                    h.READY_BY_USER_ID as ReadyByUserId,
                    h.STARTED_BY_USER_ID as StartedByUserId,
                    h.COMPLETED_BY_USER_ID as CompletedByUserId,
                    h.CANCELLED_BY_USER_ID as CancelledByUserId,
                    h.CANCEL_REASON as CancelReason,
                    ROW_NUMBER() OVER (ORDER BY si_from.START_AT DESC, h.CREATED_AT DESC) AS RN
                FROM HANDOVERS h
                LEFT JOIN PATIENTS pt ON h.PATIENT_ID = pt.ID
                LEFT JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID
                LEFT JOIN SHIFT_INSTANCES si_from ON sw.FROM_SHIFT_INSTANCE_ID = si_from.ID
                LEFT JOIN SHIFT_INSTANCES si_to ON sw.TO_SHIFT_INSTANCE_ID = si_to.ID
                LEFT JOIN SHIFTS s_from ON si_from.SHIFT_ID = s_from.ID
                LEFT JOIN SHIFTS s_to ON si_to.SHIFT_ID = s_to.ID
                LEFT JOIN USERS cb ON h.CREATED_BY_USER_ID = cb.ID
                LEFT JOIN USERS td ON COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) = td.ID
                LEFT JOIN USERS rp ON h.SENDER_USER_ID = rp.ID
                LEFT JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID
                LEFT JOIN SHIFT_COVERAGE sc_to ON sw.TO_SHIFT_INSTANCE_ID = sc_to.SHIFT_INSTANCE_ID 
                     AND h.PATIENT_ID = sc_to.PATIENT_ID
                     AND sc_to.RESPONSIBLE_USER_ID = :userId
                WHERE h.SENDER_USER_ID = :userId 
                   OR h.RECEIVER_USER_ID = :userId 
                   OR h.CREATED_BY_USER_ID = :userId 
                   OR h.COMPLETED_BY_USER_ID = :userId
                   OR h.READY_BY_USER_ID = :userId
                   OR h.STARTED_BY_USER_ID = :userId
                   OR h.CANCELLED_BY_USER_ID = :userId
                   OR sc_to.ID IS NOT NULL
            )
            WHERE RN > :offset AND RN <= :maxRow";

        var handovers = await conn.QueryAsync<HandoverRecord>(sql, new { userId, offset, maxRow = offset + ps });

        return (handovers.ToList(), total);
    }

    public async Task<string?> GetCurrentHandoverIdAsync(string patientId)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string getLatestSql = @"
            SELECT * FROM (
                SELECT h.ID
                FROM HANDOVERS h
                LEFT JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID
                LEFT JOIN SHIFT_INSTANCES si_from ON sw.FROM_SHIFT_INSTANCE_ID = si_from.ID
                WHERE h.PATIENT_ID = :patientId
                  AND h.CURRENT_STATE NOT IN ('Completed', 'Cancelled')
                ORDER BY si_from.START_AT DESC, h.CREATED_AT DESC
            ) WHERE ROWNUM <= 1";

        var existingHandoverId = await conn.ExecuteScalarAsync<string>(getLatestSql, new { patientId });
        return existingHandoverId;
    }

    public async Task<bool> HasCoverageInToShiftAsync(string handoverId, string userId)
    {
        using var conn = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT COUNT(*)
            FROM HANDOVERS h
            JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID
            JOIN SHIFT_COVERAGE sc ON sw.TO_SHIFT_INSTANCE_ID = sc.SHIFT_INSTANCE_ID
                AND h.PATIENT_ID = sc.PATIENT_ID
            WHERE h.ID = :handoverId
                AND sc.RESPONSIBLE_USER_ID = :userId";

        var count = await conn.ExecuteScalarAsync<int>(sql, new { handoverId, userId });
        return count > 0;
    }

    public async Task<string?> GetActiveHandoverForPatientAndToShiftAsync(string patientId, string toShiftId)
    {
        using var conn = _connectionFactory.CreateConnection();

        // Find an active handover where the TO shift matches the given shift template
        // This is used to detect if an assignment is for receiving (TO shift) rather than sending (FROM shift)
        // Regla #27: Receiver assignment should NOT create a new handover
        // 
        // We only look for handovers created TODAY to avoid false positives from old handovers
        const string sql = @"
            SELECT ID FROM (
                SELECT h.ID
                FROM HANDOVERS h
                JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID
                JOIN SHIFT_INSTANCES si_to ON sw.TO_SHIFT_INSTANCE_ID = si_to.ID
                WHERE h.PATIENT_ID = :patientId
                  AND si_to.SHIFT_ID = :toShiftId
                  AND h.CURRENT_STATE NOT IN ('Completed', 'Cancelled')
                  AND TRUNC(h.CREATED_AT) = TRUNC(SYSDATE)
                ORDER BY h.CREATED_AT DESC
            ) WHERE ROWNUM <= 1";

        var handoverId = await conn.ExecuteScalarAsync<string>(sql, new { patientId, toShiftId });
        return handoverId;
    }
}
