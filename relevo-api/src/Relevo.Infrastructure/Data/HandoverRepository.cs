using System.Data;
using Dapper;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

public class HandoverRepository(DapperConnectionFactory _connectionFactory) : IHandoverRepository
{
  public async Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetPatientHandoversAsync(string patientId, int page, int pageSize)
  {
    using var conn = _connectionFactory.CreateConnection();

    var p = Math.Max(page, 1);
    var ps = Math.Max(pageSize, 1);
    var offset = (p - 1) * ps;

    // Count
    var total = await conn.ExecuteScalarAsync<int>(
        "SELECT COUNT(*) FROM HANDOVERS WHERE PATIENT_ID = :PatientId",
        new { PatientId = patientId });

    if (total == 0)
        return (Array.Empty<HandoverRecord>(), 0);

    // Query
    const string sql = @"
      SELECT * FROM (
        SELECT
            h.ID,
            NULL as AssignmentId, -- ASSIGNMENT_ID removed from HANDOVERS, join USER_ASSIGNMENTS if needed
            h.PATIENT_ID as PatientId,
            p.NAME as PatientName,
            h.CURRENT_STATE as Status, -- Virtual column
            COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
            hc.PATIENT_SUMMARY as PatientSummary, -- Column renamed from SUMMARY_TEXT
            h.ID || '-sa' as SituationAwarenessDocId, -- Placeholder logic
            hc.SYNTHESIS as Synthesis, -- From HANDOVER_CONTENTS
            s.NAME as ShiftName, -- Join SHIFTS table
            h.FROM_USER_ID as CreatedBy, -- Column renamed
            h.TO_USER_ID as AssignedTo, -- Column renamed
            NULL as CreatedByName, -- Join users if needed
            NULL as AssignedToName, -- Join users if needed
            h.TO_USER_ID as ReceiverUserId, -- Column renamed
            h.FROM_USER_ID as ResponsiblePhysicianId, -- RESPONSIBLE_PHYSICIAN_ID removed, use FROM_USER_ID
            'Dr. Name' as ResponsiblePhysicianName, -- Join users if needed
            TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
            TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
            TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
            NULL as AcknowledgedAt, -- Column removed
            TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcceptedAt,
            TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
            TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
            TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as RejectedAt,
            h.REJECTION_REASON as RejectionReason,
            TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as ExpiredAt,
            NULL as HandoverType, -- Column removed
            TRUNC(h.WINDOW_START_AT) as HandoverWindowDate, -- Extract date from TIMESTAMP
            h.FROM_SHIFT_ID as FromShiftId,
            h.TO_SHIFT_ID as ToShiftId,
            h.TO_USER_ID as ToDoctorId, -- Column renamed
            h.CURRENT_STATE as StateName, -- Virtual column
            1 as Version,
            ROW_NUMBER() OVER (ORDER BY h.CREATED_AT DESC) AS RN
        FROM HANDOVERS h
        JOIN PATIENTS p ON h.PATIENT_ID = p.ID
        LEFT JOIN SHIFTS s ON h.FROM_SHIFT_ID = s.ID -- For ShiftName
        LEFT JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID -- Merged table
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
          NULL as AssignmentId, -- ASSIGNMENT_ID removed from HANDOVERS
          h.PATIENT_ID as PatientId,
          p.NAME as PatientName,
          h.CURRENT_STATE as Status, -- Virtual column
          COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
          hc.PATIENT_SUMMARY as PatientSummary, -- Column renamed
          h.ID || '-sa' as SituationAwarenessDocId, -- Placeholder logic
          hc.SYNTHESIS as Synthesis, -- From HANDOVER_CONTENTS
          s.NAME as ShiftName, -- Join SHIFTS table
          h.FROM_USER_ID as CreatedBy, -- Column renamed
          h.TO_USER_ID as AssignedTo, -- Column renamed
          NULL as CreatedByName, -- Join users if needed
          NULL as AssignedToName, -- Join users if needed
          h.TO_USER_ID as ReceiverUserId, -- Column renamed
          h.FROM_USER_ID as ResponsiblePhysicianId, -- RESPONSIBLE_PHYSICIAN_ID removed
          'Dr. Name' as ResponsiblePhysicianName, -- Join users if needed
          TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
          TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
          TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
          NULL as AcknowledgedAt, -- Column removed
          TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcceptedAt,
          TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
          TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
          TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as RejectedAt,
          h.REJECTION_REASON as RejectionReason,
          TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as ExpiredAt,
          NULL as HandoverType, -- Column removed
          TRUNC(h.WINDOW_START_AT) as HandoverWindowDate, -- Extract date
          h.FROM_SHIFT_ID as FromShiftId,
          h.TO_SHIFT_ID as ToShiftId,
          h.TO_USER_ID as ToDoctorId, -- Column renamed
          h.CURRENT_STATE as StateName, -- Virtual column
          1 as Version
      FROM HANDOVERS h
      JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      LEFT JOIN SHIFTS s ON h.FROM_SHIFT_ID = s.ID -- For ShiftName
      LEFT JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID -- Merged table
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
          h.CURRENT_STATE as STATUS, -- Virtual column
          h.FROM_USER_ID as CREATED_BY, -- Column renamed
          h.TO_USER_ID as RECEIVER_USER_ID, -- Column renamed
          NULL as CreatedByName, -- Join USERS if needed, or fetch separately
          NULL as ReceiverName, -- Join USERS if needed
          p.NAME,
          TO_CHAR(p.DATE_OF_BIRTH, 'YYYY-MM-DD') as Dob,
          p.MRN,
          TO_CHAR(p.ADMISSION_DATE, 'YYYY-MM-DD HH24:MI:SS') as AdmissionDate,
          p.ROOM_NUMBER,
          p.DIAGNOSIS,
          u.NAME as UnitName,
          hc.ILLNESS_SEVERITY, -- From HANDOVER_CONTENTS
          hc.PATIENT_SUMMARY as SUMMARY_TEXT, -- Column renamed
          hc.LAST_EDITED_BY, -- From HANDOVER_CONTENTS
          TO_CHAR(hc.UPDATED_AT, 'YYYY-MM-DD HH24:MI:SS') as UpdatedAt
      FROM HANDOVERS h
      JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      JOIN UNITS u ON p.UNIT_ID = u.ID
      LEFT JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID -- Merged table
      WHERE h.ID = :HandoverId";

    var data = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { HandoverId = handoverId });

    if (data == null) return null;

    string createdBy = (string)data.CREATED_BY;
    string receiverId = (string?)data.RECEIVER_USER_ID ?? "";
    
    // Fetch Physician Names (Assuming stored in USERS table or just mocking for now if table not populated/joined)
    // Better to join USERS in the main query if possible, but let's do separate for clarity or if USERS is in another service (it is in DB here)
    
    var creator = await GetPhysicianInfo(conn, createdBy, (string)data.STATUS ?? "Draft", "creator");
    var receiver = !string.IsNullOrEmpty(receiverId) ? await GetPhysicianInfo(conn, receiverId, (string)data.STATUS ?? "Draft", "assignee") : null;

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
        creator,
        receiver,
        (string?)data.ILLNESS_SEVERITY,
        (string?)data.SUMMARY_TEXT,
        (string?)data.LAST_EDITED_BY,
        (string?)data.UPDATEDAT
    );
  }

  public async Task<HandoverRecord> CreateHandoverAsync(CreateHandoverRequest request)
  {
    try 
    {
        using var conn = _connectionFactory.CreateConnection();

        // Generate IDs
        var handoverId = $"handover-{Guid.NewGuid().ToString().Substring(0, 8)}";
        var assignmentId = $"assign-{Guid.NewGuid().ToString().Substring(0, 8)}";

        // Create user assignment if it doesn't exist (using MERGE for Oracle)
        await conn.ExecuteAsync(@"
          MERGE INTO USER_ASSIGNMENTS ua
          USING (SELECT :assignmentId AS ASSIGNMENT_ID, :toDoctorId AS USER_ID, :toShiftId AS SHIFT_ID, :patientId AS PATIENT_ID FROM DUAL) src
          ON (ua.ASSIGNMENT_ID = src.ASSIGNMENT_ID)
          WHEN MATCHED THEN
            UPDATE SET ua.ASSIGNED_AT = LOCALTIMESTAMP
          WHEN NOT MATCHED THEN
            INSERT (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
            VALUES (src.ASSIGNMENT_ID, src.USER_ID, src.SHIFT_ID, src.PATIENT_ID, LOCALTIMESTAMP)",
          new { assignmentId, toDoctorId = request.ToDoctorId, toShiftId = request.ToShiftId, patientId = request.PatientId });

        // Create handover (CURRENT_STATE is virtual, calculated automatically)
        // WINDOW_START_AT is required, set to current time
        await conn.ExecuteAsync(@"
          INSERT INTO HANDOVERS (
            ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID,
            WINDOW_START_AT, CREATED_AT, UPDATED_AT
          ) VALUES (
            :handoverId, :patientId, :fromShiftId, :toShiftId, :fromDoctorId, :toDoctorId,
            LOCALTIMESTAMP, LOCALTIMESTAMP, LOCALTIMESTAMP
          )",
          new {
            handoverId,
            patientId = request.PatientId,
            fromShiftId = request.FromShiftId,
            toShiftId = request.ToShiftId,
            fromDoctorId = request.FromDoctorId,
            toDoctorId = request.ToDoctorId
          });

        // Add participants (USER_NAME and USER_ROLE removed, use USER_ID only)
        await conn.ExecuteAsync(@"
          INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, STATUS, JOINED_AT, LAST_ACTIVITY)
          VALUES (:participantId1, :handoverId, :fromDoctorId, 'active', LOCALTIMESTAMP, LOCALTIMESTAMP)",
          new {
            handoverId,
            fromDoctorId = request.FromDoctorId,
            participantId1 = $"participant-{Guid.NewGuid().ToString().Substring(0, 8)}"
          });
        
        await conn.ExecuteAsync(@"
          INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, STATUS, JOINED_AT, LAST_ACTIVITY)
          VALUES (:participantId2, :handoverId, :toDoctorId, 'active', LOCALTIMESTAMP, LOCALTIMESTAMP)",
          new {
            handoverId,
            toDoctorId = request.ToDoctorId,
            participantId2 = $"participant-{Guid.NewGuid().ToString().Substring(0, 8)}"
          });

        // Create default HANDOVER_CONTENTS (merged table replaces HANDOVER_PATIENT_DATA, HANDOVER_SYNTHESIS, HANDOVER_SITUATION_AWARENESS)
        // Note: HANDOVER_CONTENTS does not have CREATED_AT, only UPDATED_AT
        // Use NULL instead of empty string - Oracle treats empty string as NULL for VARCHAR2
        await conn.ExecuteAsync(@"
          INSERT INTO HANDOVER_CONTENTS (
            HANDOVER_ID, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS, SYNTHESIS,
            PATIENT_SUMMARY_STATUS, SA_STATUS, SYNTHESIS_STATUS, LAST_EDITED_BY, UPDATED_AT
          ) VALUES (
            :handoverId, 'Stable', NULL, NULL, NULL,
            'Draft', 'Draft', 'Draft', :initiatedBy, LOCALTIMESTAMP
          )",
          new { handoverId, initiatedBy = request.InitiatedBy });

        // Use the SAME connection to fetch the created handover to avoid isolation/consistency issues
        const string fetchSql = @"
          SELECT
              h.ID,
              NULL as AssignmentId, -- ASSIGNMENT_ID removed
              h.PATIENT_ID as PatientId,
              p.NAME as PatientName,
              h.CURRENT_STATE as Status, -- Virtual column
              COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
              hc.PATIENT_SUMMARY as PatientSummary, -- Column renamed
              h.ID || '-sa' as SituationAwarenessDocId,
              hc.SYNTHESIS as Synthesis, -- From HANDOVER_CONTENTS
              s.NAME as ShiftName, -- Join SHIFTS
              h.FROM_USER_ID as CreatedBy, -- Column renamed
              h.TO_USER_ID as AssignedTo, -- Column renamed
              NULL as CreatedByName,
              NULL as AssignedToName,
              h.TO_USER_ID as ReceiverUserId, -- Column renamed
              h.FROM_USER_ID as ResponsiblePhysicianId, -- RESPONSIBLE_PHYSICIAN_ID removed
              'Dr. Name' as ResponsiblePhysicianName,
              TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
              TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
              TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
              NULL as AcknowledgedAt, -- Column removed
              TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcceptedAt,
              TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
              TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
              TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as RejectedAt,
              h.REJECTION_REASON as RejectionReason,
              TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as ExpiredAt,
              NULL as HandoverType, -- Column removed
              TRUNC(h.WINDOW_START_AT) as HandoverWindowDate, -- Extract date
              h.FROM_SHIFT_ID as FromShiftId,
              h.TO_SHIFT_ID as ToShiftId,
              h.TO_USER_ID as ToDoctorId, -- Column renamed
              h.CURRENT_STATE as StateName, -- Virtual column
              1 as Version
          FROM HANDOVERS h
          LEFT JOIN PATIENTS p ON h.PATIENT_ID = p.ID
          LEFT JOIN SHIFTS s ON h.FROM_SHIFT_ID = s.ID -- For ShiftName
          LEFT JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID -- Merged table
          WHERE h.ID = :HandoverId";

        var handover = await conn.QueryFirstOrDefaultAsync<HandoverRecord>(fetchSql, new { HandoverId = handoverId });

        if (handover == null) throw new InvalidOperationException($"Failed to retrieve created handover (HandoverId: {handoverId} not found).");
        
        return handover;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating handover: {ex.Message} {ex.StackTrace}");
        throw;
    }
  }

  public async Task<IReadOnlyList<ContingencyPlanRecord>> GetContingencyPlansAsync(string handoverId)
  {
    using var conn = _connectionFactory.CreateConnection();
    const string sql = @"
        SELECT ID, HANDOVER_ID as HandoverId, CONDITION_TEXT as ConditionText,
               ACTION_TEXT as ActionText, PRIORITY, STATUS, CREATED_BY as CreatedBy,
               CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
        FROM HANDOVER_CONTINGENCY
        WHERE HANDOVER_ID = :handoverId
        ORDER BY CREATED_AT ASC";

    var plans = await conn.QueryAsync<ContingencyPlanRecord>(sql, new { handoverId });
    return plans.ToList();
  }

  public async Task<ContingencyPlanRecord> CreateContingencyPlanAsync(string handoverId, string condition, string action, string priority, string createdBy)
  {
    using var conn = _connectionFactory.CreateConnection();
    var id = Guid.NewGuid().ToString();

    const string sql = @"
        INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT, UPDATED_AT)
        VALUES (:id, :handoverId, :condition, :action, :priority, 'active', :createdBy, LOCALTIMESTAMP, LOCALTIMESTAMP)";

    await conn.ExecuteAsync(sql, new { id, handoverId, condition, action, priority, createdBy });

    return new ContingencyPlanRecord(
        id, handoverId, condition, action, priority, "active",
        createdBy, DateTime.UtcNow, DateTime.UtcNow);
  }

  public async Task<bool> DeleteContingencyPlanAsync(string handoverId, string contingencyId)
  {
    using var conn = _connectionFactory.CreateConnection();
    const string sql = @"
        DELETE FROM HANDOVER_CONTINGENCY
        WHERE ID = :contingencyId AND HANDOVER_ID = :handoverId";

    var rows = await conn.ExecuteAsync(sql, new { contingencyId, handoverId });
        return rows > 0;
  }

  public async Task<HandoverSynthesisRecord?> GetSynthesisAsync(string handoverId)
  {
    using var conn = _connectionFactory.CreateConnection();
    const string sql = @"
        SELECT HANDOVER_ID as HandoverId, SYNTHESIS as Content, SYNTHESIS_STATUS as STATUS,
               LAST_EDITED_BY as LastEditedBy, UPDATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
        FROM HANDOVER_CONTENTS
        WHERE HANDOVER_ID = :handoverId";

    var result = await conn.QueryFirstOrDefaultAsync<HandoverSynthesisRecord>(sql, new { handoverId });

    // If no record exists but handover does, create default (mimicking legacy behavior)
    if (result == null)
    {
        // Check if handover exists first to avoid FK error or creating orphan data
        var createdBy = await conn.ExecuteScalarAsync<string>(
            "SELECT FROM_USER_ID FROM HANDOVERS WHERE ID = :handoverId",
            new { handoverId });

        if (createdBy == null) return null; // Handover doesn't exist

        await conn.ExecuteAsync(@"
            INSERT INTO HANDOVER_CONTENTS (
                HANDOVER_ID, SYNTHESIS, SYNTHESIS_STATUS, LAST_EDITED_BY, UPDATED_AT,
                ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS, PATIENT_SUMMARY_STATUS, SA_STATUS
            ) VALUES (
                :handoverId, '', 'Draft', :createdBy, LOCALTIMESTAMP,
                'Stable', '', '', 'Draft', 'Draft'
            )", new { handoverId, createdBy });

        // Fetch again
        result = await conn.QueryFirstOrDefaultAsync<HandoverSynthesisRecord>(sql, new { handoverId });
    }

    return result;
  }

  public async Task<bool> UpdateSynthesisAsync(string handoverId, string? content, string status, string userId)
  {
    try
    {
        using var conn = _connectionFactory.CreateConnection();
        
        // Verify handover exists first
        var handoverExists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM HANDOVERS WHERE ID = :handoverId", 
            new { handoverId }) > 0;
        
        if (!handoverExists)
        {
            return false;
        }

        // Use MERGE to update or insert HANDOVER_CONTENTS
        // Oracle MERGE returns number of rows affected (matched + inserted)
        const string sql = @"
            MERGE INTO HANDOVER_CONTENTS hc
            USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (hc.HANDOVER_ID = src.HANDOVER_ID)
            WHEN MATCHED THEN
                UPDATE SET SYNTHESIS = :content, SYNTHESIS_STATUS = :status, LAST_EDITED_BY = :userId, UPDATED_AT = LOCALTIMESTAMP
            WHEN NOT MATCHED THEN
                INSERT (HANDOVER_ID, SYNTHESIS, SYNTHESIS_STATUS, LAST_EDITED_BY, UPDATED_AT,
                        ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS, PATIENT_SUMMARY_STATUS, SA_STATUS)
                VALUES (:handoverId, :content, :status, :userId, LOCALTIMESTAMP,
                        'Stable', '', '', 'Draft', 'Draft')";

        var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, content = content ?? "", status, userId });
        
        // MERGE returns 1 if matched (updated) or inserted, 0 if no match and no insert
        // In Oracle, MERGE can return 0 if no rows were affected, but should return 1 for INSERT or UPDATE
        return rowsAffected > 0;
    }
    catch (Exception)
    {
        return false;
    }
  }

  public async Task<HandoverSituationAwarenessRecord?> GetSituationAwarenessAsync(string handoverId)
  {
    using var conn = _connectionFactory.CreateConnection();
    const string sql = @"
        SELECT HANDOVER_ID as HandoverId, SITUATION_AWARENESS as Content, SA_STATUS as STATUS,
               LAST_EDITED_BY as LastEditedBy, UPDATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
        FROM HANDOVER_CONTENTS
        WHERE HANDOVER_ID = :handoverId";

    var result = await conn.QueryFirstOrDefaultAsync<HandoverSituationAwarenessRecord>(sql, new { handoverId });

    if (result == null)
    {
        // Check if handover exists first
        var createdBy = await conn.ExecuteScalarAsync<string>(
            "SELECT FROM_USER_ID FROM HANDOVERS WHERE ID = :handoverId",
            new { handoverId });

        if (createdBy == null) return null; 

        await conn.ExecuteAsync(@"
            INSERT INTO HANDOVER_CONTENTS (
                HANDOVER_ID, SITUATION_AWARENESS, SA_STATUS, LAST_EDITED_BY, UPDATED_AT,
                ILLNESS_SEVERITY, PATIENT_SUMMARY, SYNTHESIS, PATIENT_SUMMARY_STATUS, SYNTHESIS_STATUS
            ) VALUES (
                :handoverId, '', 'Draft', :createdBy, LOCALTIMESTAMP,
                'Stable', '', '', 'Draft', 'Draft'
            )", new { handoverId, createdBy });

        result = await conn.QueryFirstOrDefaultAsync<HandoverSituationAwarenessRecord>(sql, new { handoverId });
    }

    return result;
  }

  public async Task<bool> UpdateSituationAwarenessAsync(string handoverId, string? content, string status, string userId)
  {
    try
    {
        using var conn = _connectionFactory.CreateConnection();
        
        // Verify handover exists first
        var handoverExists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM HANDOVERS WHERE ID = :handoverId", 
            new { handoverId }) > 0;
        
        if (!handoverExists)
        {
            return false;
        }

        // Use MERGE to update or insert HANDOVER_CONTENTS
        const string sql = @"
            MERGE INTO HANDOVER_CONTENTS hc
            USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (hc.HANDOVER_ID = src.HANDOVER_ID)
            WHEN MATCHED THEN
                UPDATE SET SITUATION_AWARENESS = :content, SA_STATUS = :status, LAST_EDITED_BY = :userId, UPDATED_AT = LOCALTIMESTAMP
            WHEN NOT MATCHED THEN
                INSERT (HANDOVER_ID, SITUATION_AWARENESS, SA_STATUS, LAST_EDITED_BY, UPDATED_AT,
                        ILLNESS_SEVERITY, PATIENT_SUMMARY, SYNTHESIS, PATIENT_SUMMARY_STATUS, SYNTHESIS_STATUS)
                VALUES (:handoverId, :content, :status, :userId, LOCALTIMESTAMP,
                        'Stable', '', '', 'Draft', 'Draft')";

        var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, content = content ?? "", status, userId });
        
        // MERGE returns 1 if matched (updated) or inserted, 0 if no match and no insert
        // In Oracle, MERGE can return 0 if no rows were affected, but should return 1 for INSERT or UPDATE
        return rowsAffected > 0;
    }
    catch (Exception)
    {
        return false;
    }
  }

  public async Task<bool> MarkAsReadyAsync(string handoverId, string userId)
  {
    try
    {
        using var conn = _connectionFactory.CreateConnection();
        
        // Verify handover exists first
        var handoverExists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM HANDOVERS WHERE ID = :handoverId", 
            new { handoverId }) > 0;
        
        if (!handoverExists)
        {
            return false;
        }

        // CURRENT_STATE is virtual, calculated automatically from READY_AT
        // Constraint CHK_HO_RD_AFTER_CR ensures READY_AT >= CREATED_AT
        // Allow updating READY_AT even if already set (idempotent operation)
        // Only update if handover is in Draft state (READY_AT IS NULL) to avoid unnecessary updates
        const string sql = @"
            UPDATE HANDOVERS
            SET READY_AT = LOCALTIMESTAMP, 
                UPDATED_AT = LOCALTIMESTAMP
            WHERE ID = :handoverId
              AND READY_AT IS NULL";

        var rows = await conn.ExecuteAsync(sql, new { handoverId });
        // If handover is already Ready, return true (idempotent)
        if (rows == 0)
        {
            // Check if handover is already Ready
            var alreadyReady = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM HANDOVERS WHERE ID = :handoverId AND READY_AT IS NOT NULL",
                new { handoverId }) > 0;
            return alreadyReady;
        }
        return rows > 0;
    }
    catch (Exception)
    {
        // Handle constraint violations or other errors
        return false;
    }
  }

  public async Task<HandoverClinicalDataRecord?> GetClinicalDataAsync(string handoverId)
  {
    using var conn = _connectionFactory.CreateConnection();
    const string sql = @"
        SELECT HANDOVER_ID as HandoverId, ILLNESS_SEVERITY as IllnessSeverity, 
               PATIENT_SUMMARY as SummaryText, LAST_EDITED_BY as LastEditedBy, 
               PATIENT_SUMMARY_STATUS as STATUS, UPDATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
        FROM HANDOVER_CONTENTS
        WHERE HANDOVER_ID = :handoverId";

    var result = await conn.QueryFirstOrDefaultAsync<HandoverClinicalDataRecord>(sql, new { handoverId });

    if (result == null)
    {
        // Check if handover exists
        var exists = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM HANDOVERS WHERE ID = :handoverId", new { handoverId }) > 0;
        if (!exists) return null;

        // Create default if missing
        var userId = await conn.ExecuteScalarAsync<string>("SELECT FROM_USER_ID FROM HANDOVERS WHERE ID = :handoverId", new { handoverId });
        
        await conn.ExecuteAsync(@"
            INSERT INTO HANDOVER_CONTENTS (
                HANDOVER_ID, ILLNESS_SEVERITY, PATIENT_SUMMARY, PATIENT_SUMMARY_STATUS, LAST_EDITED_BY, UPDATED_AT,
                SITUATION_AWARENESS, SYNTHESIS, SA_STATUS, SYNTHESIS_STATUS
            ) VALUES (
                :handoverId, 'Stable', '', 'Draft', :userId, LOCALTIMESTAMP,
                '', '', 'Draft', 'Draft'
            )",
            new { handoverId, userId });

        result = await conn.QueryFirstOrDefaultAsync<HandoverClinicalDataRecord>(sql, new { handoverId });
    }

    return result;
  }

  public async Task<bool> UpdateClinicalDataAsync(string handoverId, string illnessSeverity, string summaryText, string userId)
  {
    try
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            MERGE INTO HANDOVER_CONTENTS hc
            USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (hc.HANDOVER_ID = src.HANDOVER_ID)
            WHEN MATCHED THEN
                UPDATE SET ILLNESS_SEVERITY = :illnessSeverity, PATIENT_SUMMARY = :summaryText, 
                           LAST_EDITED_BY = :userId, UPDATED_AT = LOCALTIMESTAMP
            WHEN NOT MATCHED THEN
                INSERT (HANDOVER_ID, ILLNESS_SEVERITY, PATIENT_SUMMARY, PATIENT_SUMMARY_STATUS, LAST_EDITED_BY, UPDATED_AT,
                        SITUATION_AWARENESS, SYNTHESIS, SA_STATUS, SYNTHESIS_STATUS)
                VALUES (:handoverId, :illnessSeverity, :summaryText, 'Draft', :userId, LOCALTIMESTAMP,
                        '', '', 'Draft', 'Draft')";

        var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, illnessSeverity, summaryText, userId });
        return rowsAffected > 0;
    }
    catch (Exception)
    {
        return false;
    }
  }

  private async Task<PhysicianRecord> GetPhysicianInfo(IDbConnection conn, string userId, string handoverStatus, string relationship)
  {
      // Get Name
      var name = await conn.ExecuteScalarAsync<string>("SELECT FULL_NAME FROM USERS WHERE ID = :UserId", new { UserId = userId }) ?? "Unknown";
      
      // Get Shift
      const string shiftSql = @"
        SELECT * FROM (
            SELECT s.START_TIME, s.END_TIME
            FROM USER_ASSIGNMENTS ua
            JOIN SHIFTS s ON ua.SHIFT_ID = s.ID
            WHERE ua.USER_ID = :UserId
        ) WHERE ROWNUM <= 1";
      
      var shift = await conn.QueryFirstOrDefaultAsync<dynamic>(shiftSql, new { UserId = userId });
      
      string status = CalculatePhysicianStatus(handoverStatus, relationship);

      return new PhysicianRecord(
          name,
          "Doctor",
          "", // Color
          (string?)shift?.START_TIME,
          (string?)shift?.END_TIME,
          status,
          relationship == "creator" ? "assigned" : "receiving"
      );
  }

  public async Task<bool> StartHandoverAsync(string handoverId, string userId)
  {
    try
    {
      return await UpdateHandoverStatus(handoverId, "InProgress", "STARTED_AT", userId);
    }
    catch (Oracle.ManagedDataAccess.Client.OracleException ex) when (ex.Number == 2290)
    {
      // ORA-02290: check constraint violated - state machine constraint
      return false;
    }
  }

  public async Task<bool> AcceptHandoverAsync(string handoverId, string userId)
  {
    try
    {
      return await UpdateHandoverStatus(handoverId, "Accepted", "ACCEPTED_AT", userId);
    }
    catch (Oracle.ManagedDataAccess.Client.OracleException ex) when (ex.Number == 2290)
    {
      // ORA-02290: check constraint violated - state machine constraint
      return false;
    }
  }

  public async Task<bool> RejectHandoverAsync(string handoverId, string reason, string userId)
  {
    try
    {
      using var conn = _connectionFactory.CreateConnection();
      // CURRENT_STATE is virtual, calculated automatically from REJECTED_AT
      const string sql = @"
          UPDATE HANDOVERS
          SET REJECTION_REASON = :reason,
              REJECTED_AT = LOCALTIMESTAMP, 
              UPDATED_AT = LOCALTIMESTAMP
          WHERE ID = :handoverId";

      var rows = await conn.ExecuteAsync(sql, new { handoverId, reason });
      return rows > 0;
    }
    catch (Oracle.ManagedDataAccess.Client.OracleException ex) when (ex.Number == 2290)
    {
      // ORA-02290: check constraint violated - single terminal state constraint
      return false;
    }
  }

  public async Task<bool> CancelHandoverAsync(string handoverId, string userId)
  {
    try
    {
      return await UpdateHandoverStatus(handoverId, "Cancelled", "CANCELLED_AT", userId);
    }
    catch (Oracle.ManagedDataAccess.Client.OracleException ex) when (ex.Number == 2290)
    {
      // ORA-02290: check constraint violated - state machine constraint
      return false;
    }
  }

  public async Task<bool> CompleteHandoverAsync(string handoverId, string userId)
  {
    try
    {
      using var conn = _connectionFactory.CreateConnection();
      // CURRENT_STATE is virtual, calculated automatically from COMPLETED_AT
      // COMPLETED_BY column doesn't exist in new schema, removed
      const string sql = @"
          UPDATE HANDOVERS
          SET COMPLETED_AT = LOCALTIMESTAMP, 
              UPDATED_AT = LOCALTIMESTAMP
          WHERE ID = :handoverId";

      var rows = await conn.ExecuteAsync(sql, new { handoverId, userId });
      return rows > 0;
    }
    catch (Oracle.ManagedDataAccess.Client.OracleException ex) when (ex.Number == 2290)
    {
      // ORA-02290: check constraint violated - state machine constraint
      return false;
    }
  }

  public async Task<IReadOnlyList<HandoverRecord>> GetPendingHandoversAsync(string userId)
  {
    using var conn = _connectionFactory.CreateConnection();
    
    // Assuming "Pending" means assigned to me and not completed/cancelled/rejected
    // And typically in 'Ready' or 'InProgress' state for the receiver to see?
    // Or 'Draft' if I am the creator? 
    // For simplicity, let's say anything where I am involved and it's active.
    // But the endpoint description says "Get pending handovers". 
    // Let's assume it means handovers assigned to the user that are actionable.
    
    const string sql = @"
        SELECT
            h.ID,
            NULL as AssignmentId, -- ASSIGNMENT_ID removed
            h.PATIENT_ID as PatientId,
            p.NAME as PatientName,
            h.CURRENT_STATE as Status, -- Virtual column
            COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
            hc.PATIENT_SUMMARY as PatientSummary, -- Column renamed
            h.ID || '-sa' as SituationAwarenessDocId,
            hc.SYNTHESIS as Synthesis, -- From HANDOVER_CONTENTS
            s.NAME as ShiftName, -- Join SHIFTS
            h.FROM_USER_ID as CreatedBy, -- Column renamed
            h.TO_USER_ID as AssignedTo, -- Column renamed
            NULL as CreatedByName,
            NULL as AssignedToName,
            h.TO_USER_ID as ReceiverUserId, -- Column renamed
            h.FROM_USER_ID as ResponsiblePhysicianId, -- RESPONSIBLE_PHYSICIAN_ID removed
            'Dr. Name' as ResponsiblePhysicianName,
            TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
            TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
            TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
            NULL as AcknowledgedAt, -- Column removed
            TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcceptedAt,
            TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
            TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
            TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as RejectedAt,
            h.REJECTION_REASON as RejectionReason,
            TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as ExpiredAt,
            NULL as HandoverType, -- Column removed
            TRUNC(h.WINDOW_START_AT) as HandoverWindowDate, -- Extract date
            h.FROM_SHIFT_ID as FromShiftId,
            h.TO_SHIFT_ID as ToShiftId,
            h.TO_USER_ID as ToDoctorId, -- Column renamed
            h.CURRENT_STATE as StateName, -- Virtual column
            1 as Version
        FROM HANDOVERS h
        JOIN PATIENTS p ON h.PATIENT_ID = p.ID
        LEFT JOIN SHIFTS s ON h.FROM_SHIFT_ID = s.ID
        LEFT JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID
        WHERE h.TO_USER_ID = :userId
          AND h.CURRENT_STATE IN ('Draft', 'Ready', 'InProgress')";

    var handovers = await conn.QueryAsync<HandoverRecord>(sql, new { userId });
    return handovers.ToList();
  }

  private async Task<bool> UpdateHandoverStatus(string handoverId, string status, string timestampColumn, string userId)
  {
    using var conn = _connectionFactory.CreateConnection();
    // CURRENT_STATE is virtual, calculated automatically from timestamp columns
    // Only update the timestamp column, not STATUS
    string sql = $@"
        UPDATE HANDOVERS
        SET {timestampColumn} = LOCALTIMESTAMP, 
            UPDATED_AT = LOCALTIMESTAMP
        WHERE ID = :handoverId";

    var rows = await conn.ExecuteAsync(sql, new { handoverId });
    return rows > 0;
  }

  private static string CalculatePhysicianStatus(string state, string relationship)
  {
    state = state?.ToLower() ?? "";
    return state switch
    {
      "completed" => "completed",
      "cancelled" => "cancelled",
      "rejected" => "rejected",
      "expired" => "expired",
      "accepted" => relationship == "creator" ? "handed-off" : "accepted",
      "draft" => relationship == "creator" ? "handing-off" : "pending",
      "ready" => relationship == "creator" ? "handing-off" : "ready-to-receive",
      "inprogress" => relationship == "creator" ? "handing-off" : "receiving",
      _ => "unknown"
    };
  }


  // Action Items
  public async Task<IReadOnlyList<HandoverActionItemFullRecord>> GetActionItemsAsync(string handoverId)
  {
    using var conn = _connectionFactory.CreateConnection();

    const string sql = @"
      SELECT ID, HANDOVER_ID as HandoverId, DESCRIPTION, IS_COMPLETED as IsCompleted,
             CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt, COMPLETED_AT as CompletedAt
      FROM HANDOVER_ACTION_ITEMS
      WHERE HANDOVER_ID = :handoverId
      ORDER BY CREATED_AT DESC";

    var items = await conn.QueryAsync<HandoverActionItemFullRecord>(sql, new { handoverId });
    return items.ToList();
  }

  public async Task<HandoverActionItemFullRecord> CreateActionItemAsync(string handoverId, string description, string priority)
  {
    using var conn = _connectionFactory.CreateConnection();
    var id = $"action-{Guid.NewGuid().ToString()[..8]}";
    var now = DateTime.UtcNow;

    const string sql = @"
      INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED, CREATED_AT, UPDATED_AT)
      VALUES (:id, :handoverId, :description, 0, LOCALTIMESTAMP, LOCALTIMESTAMP)";

    await conn.ExecuteAsync(sql, new { id, handoverId, description });

    return new HandoverActionItemFullRecord(
        id,
        handoverId,
        description,
        false,
        now,
        now,
        null
    );
  }

  public async Task<bool> UpdateActionItemAsync(string handoverId, string itemId, bool isCompleted)
  {
    using var conn = _connectionFactory.CreateConnection();

    const string sql = @"
      UPDATE HANDOVER_ACTION_ITEMS
      SET IS_COMPLETED = :isCompleted,
          COMPLETED_AT = CASE WHEN :isCompleted = 1 THEN LOCALTIMESTAMP ELSE NULL END,
          UPDATED_AT = LOCALTIMESTAMP
      WHERE ID = :itemId AND HANDOVER_ID = :handoverId";

    var result = await conn.ExecuteAsync(sql, new { itemId, handoverId, isCompleted = isCompleted ? 1 : 0 });
    return result > 0;
  }

  public async Task<bool> DeleteActionItemAsync(string handoverId, string itemId)
  {
    using var conn = _connectionFactory.CreateConnection();

    const string sql = @"DELETE FROM HANDOVER_ACTION_ITEMS WHERE ID = :itemId AND HANDOVER_ID = :handoverId";

    var result = await conn.ExecuteAsync(sql, new { itemId, handoverId });
    return result > 0;
  }

  // Activity Log
  public async Task<IReadOnlyList<HandoverActivityRecord>> GetActivityLogAsync(string handoverId)
  {
    using var conn = _connectionFactory.CreateConnection();

    const string sql = @"
      SELECT hal.ID, hal.HANDOVER_ID as HandoverId, hal.USER_ID as UserId,
             COALESCE(u.FULL_NAME, u.FIRST_NAME || ' ' || u.LAST_NAME, 'Unknown') as UserName,
             hal.ACTIVITY_TYPE as ActivityType, hal.DESCRIPTION as ActivityDescription,
             NULL as SectionAffected, -- Column removed, use DESCRIPTION or METADATA instead
             hal.METADATA, hal.CREATED_AT as CreatedAt
      FROM HANDOVER_ACTIVITY_LOG hal
      LEFT JOIN USERS u ON hal.USER_ID = u.ID
      WHERE hal.HANDOVER_ID = :handoverId
      ORDER BY hal.CREATED_AT DESC";

    var activities = await conn.QueryAsync<HandoverActivityRecord>(sql, new { handoverId });
    return activities.ToList();
  }


  // Messages
  public async Task<IReadOnlyList<HandoverMessageRecord>> GetMessagesAsync(string handoverId)
  {
    using var conn = _connectionFactory.CreateConnection();

    const string sql = @"
      SELECT m.ID, m.HANDOVER_ID as HandoverId, m.USER_ID as UserId,
             COALESCE(u.FULL_NAME, u.FIRST_NAME || ' ' || u.LAST_NAME, 'Unknown') as UserName, -- Join USERS for name
             m.MESSAGE_TEXT as MessageText, m.MESSAGE_TYPE as MessageType,
             m.CREATED_AT as CreatedAt, m.UPDATED_AT as UpdatedAt
      FROM HANDOVER_MESSAGES m
      LEFT JOIN USERS u ON m.USER_ID = u.ID -- Join for USER_NAME (column removed)
      WHERE m.HANDOVER_ID = :handoverId
      ORDER BY m.CREATED_AT ASC";

    var messages = await conn.QueryAsync<HandoverMessageRecord>(sql, new { handoverId });
    return messages.ToList();
  }

  public async Task<HandoverMessageRecord> CreateMessageAsync(string handoverId, string userId, string userName, string messageText, string messageType)
  {
    using var conn = _connectionFactory.CreateConnection();
    var id = Guid.NewGuid().ToString();
    var now = DateTime.UtcNow;

    // USER_NAME removed from HANDOVER_MESSAGES, use USER_ID only (join USERS if name needed)
    const string sql = @"
      INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT, UPDATED_AT)
      VALUES (:id, :handoverId, :userId, :messageText, :messageType, LOCALTIMESTAMP, LOCALTIMESTAMP)";

    await conn.ExecuteAsync(sql, new { id, handoverId, userId, messageText, messageType });

    return new HandoverMessageRecord(id, handoverId, userId, userName, messageText, messageType, now, now);
  }

  // My Handovers
  public async Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetMyHandoversAsync(string userId, int page, int pageSize)
  {
    using var conn = _connectionFactory.CreateConnection();

    // Get total count - handovers where user is involved (FROM_USER_ID or TO_USER_ID)
    const string countSql = @"
        SELECT COUNT(1)
        FROM HANDOVERS h
        WHERE h.FROM_USER_ID = :userId OR h.TO_USER_ID = :userId";

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
                NULL as AssignmentId, -- ASSIGNMENT_ID removed
                h.PATIENT_ID as PatientId,
                pt.NAME as PatientName,
                h.CURRENT_STATE as Status, -- Virtual column
                COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
                hc.PATIENT_SUMMARY as PatientSummary, -- Column renamed
                h.ID || '-sa' as SituationAwarenessDocId,
                hc.SYNTHESIS as Synthesis, -- From HANDOVER_CONTENTS
                s.NAME as ShiftName, -- Join SHIFTS
                h.FROM_USER_ID as CreatedBy, -- Column renamed
                h.TO_USER_ID as AssignedTo, -- Column renamed
                cb.FULL_NAME as CreatedByName,
                td.FULL_NAME as AssignedToName,
                h.TO_USER_ID as ReceiverUserId, -- Column renamed
                h.FROM_USER_ID as ResponsiblePhysicianId, -- RESPONSIBLE_PHYSICIAN_ID removed
                rp.FULL_NAME as ResponsiblePhysicianName,
                TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
                TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
                TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
                NULL as AcknowledgedAt, -- Column removed
                TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcceptedAt,
                TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
                TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
                TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as RejectedAt,
                h.REJECTION_REASON as RejectionReason,
                TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as ExpiredAt,
                NULL as HandoverType, -- Column removed
                TRUNC(h.WINDOW_START_AT) as HandoverWindowDate, -- Extract date
                h.FROM_SHIFT_ID as FromShiftId,
                h.TO_SHIFT_ID as ToShiftId,
                h.TO_USER_ID as ToDoctorId, -- Column renamed
                h.CURRENT_STATE as StateName, -- Virtual column
                1 as Version,
                ROW_NUMBER() OVER (ORDER BY h.CREATED_AT DESC) AS RN
            FROM HANDOVERS h
            LEFT JOIN PATIENTS pt ON h.PATIENT_ID = pt.ID
            LEFT JOIN SHIFTS s ON h.FROM_SHIFT_ID = s.ID -- For ShiftName
            LEFT JOIN USERS cb ON h.FROM_USER_ID = cb.ID -- Column renamed
            LEFT JOIN USERS td ON h.TO_USER_ID = td.ID -- Column renamed
            LEFT JOIN USERS rp ON rp.ID = h.FROM_USER_ID -- RESPONSIBLE_PHYSICIAN_ID removed
            LEFT JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID -- Merged table
            WHERE h.FROM_USER_ID = :userId OR h.TO_USER_ID = :userId
        )
        WHERE RN > :offset AND RN <= :maxRow";

    var handovers = await conn.QueryAsync<HandoverRecord>(sql, new { userId, offset, maxRow = offset + ps });

    return (handovers.ToList(), total);
  }

  // Get current handover for patient (read-only, no side effects)
  // Returns the handover ID of the latest non-terminal handover, or null if none exists
  public async Task<string?> GetCurrentHandoverIdAsync(string patientId)
  {
    using var conn = _connectionFactory.CreateConnection();

    // Get latest non-terminal handover for patient
    const string getLatestSql = @"
      SELECT * FROM (
        SELECT h.ID
        FROM HANDOVERS h
        WHERE h.PATIENT_ID = :patientId
          AND h.CURRENT_STATE NOT IN ('Completed', 'Cancelled', 'Rejected', 'Expired')
        ORDER BY h.WINDOW_START_AT DESC, h.CREATED_AT DESC
      ) WHERE ROWNUM <= 1";

    var existingHandoverId = await conn.ExecuteScalarAsync<string>(getLatestSql, new { patientId });
    return existingHandoverId;
  }

  // Get or create current handover for patient (for Patient Summary write operations)
  // Returns the handover ID of the latest non-terminal handover, or creates a new Draft handover if none exists
  public async Task<string?> GetOrCreateCurrentHandoverIdAsync(string patientId, string userId)
  {
    using var conn = _connectionFactory.CreateConnection();

    // Try to get existing handover first
    var existingHandoverId = await GetCurrentHandoverIdAsync(patientId);

    if (!string.IsNullOrEmpty(existingHandoverId))
    {
      // Verify HANDOVER_CONTENTS exists
      var contentsExists = await conn.ExecuteScalarAsync<int>(
        "SELECT COUNT(*) FROM HANDOVER_CONTENTS WHERE HANDOVER_ID = :handoverId",
        new { handoverId = existingHandoverId });

      if (contentsExists == 0)
      {
        // If an existing handover is found but HANDOVER_CONTENTS row is missing, create a default contents row
        try
        {
          await conn.ExecuteAsync(@"
            INSERT INTO HANDOVER_CONTENTS (
              HANDOVER_ID, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS, SYNTHESIS,
              PATIENT_SUMMARY_STATUS, SA_STATUS, SYNTHESIS_STATUS, LAST_EDITED_BY, UPDATED_AT
            ) VALUES (
              :handoverId, 'Stable', NULL, NULL, NULL,
              'Draft', 'Draft', 'Draft', :userId, LOCALTIMESTAMP
            )",
            new { handoverId = existingHandoverId, userId });
        }
        catch (Exception)
        {
          // Ignore errors - may be a race condition
        }
      }

      return existingHandoverId;
    }

    // No active handover exists, need to create one
    // Get current assignment for patient to determine shift and user
    const string getAssignmentSql = @"
      SELECT * FROM (
        SELECT ua.USER_ID, ua.SHIFT_ID, s.ID as SHIFT_ID_FROM
        FROM USER_ASSIGNMENTS ua
        JOIN SHIFTS s ON ua.SHIFT_ID = s.ID
        WHERE ua.PATIENT_ID = :patientId
        ORDER BY ua.ASSIGNED_AT DESC
      ) WHERE ROWNUM <= 1";

    var assignment = await conn.QueryFirstOrDefaultAsync<dynamic>(getAssignmentSql, new { patientId });

    if (assignment == null)
    {
      // No assignment exists, cannot create handover
      return null;
    }

    string assignedUserId = assignment.USER_ID;
    string shiftId = assignment.SHIFT_ID;
    
    // Use same shift for FROM and TO (self-handover scenario, or use a default "next" shift)
    // For simplicity, use the same shift for both FROM and TO
    var handoverId = $"handover-{Guid.NewGuid().ToString().Substring(0, 8)}";

    // Create handover
    await conn.ExecuteAsync(@"
      INSERT INTO HANDOVERS (ID, PATIENT_ID, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_USER_ID, TO_USER_ID, WINDOW_START_AT, CREATED_AT, UPDATED_AT)
      VALUES (:handoverId, :patientId, :shiftId, :shiftId, :userId, :assignedUserId, LOCALTIMESTAMP, LOCALTIMESTAMP, LOCALTIMESTAMP)",
      new { handoverId, patientId, shiftId, userId, assignedUserId });

    // Create HANDOVER_CONTENTS entry - use NULL instead of empty string for VARCHAR2 columns
    // Oracle treats empty string as NULL, so we explicitly use NULL
    await conn.ExecuteAsync(@"
      INSERT INTO HANDOVER_CONTENTS (
        HANDOVER_ID, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS, SYNTHESIS,
        PATIENT_SUMMARY_STATUS, SA_STATUS, SYNTHESIS_STATUS, LAST_EDITED_BY, UPDATED_AT
      ) VALUES (
        :handoverId, 'Stable', NULL, NULL, NULL,
        'Draft', 'Draft', 'Draft', :userId, LOCALTIMESTAMP
      )",
      new { handoverId, userId });

    return handoverId;
  }
}
