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
            h.ASSIGNMENT_ID as AssignmentId,
            h.PATIENT_ID as PatientId,
            p.NAME as PatientName,
            h.STATUS,
            COALESCE(hpd.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
            hpd.SUMMARY_TEXT as PatientSummary,
            h.ID || '-sa' as SituationAwarenessDocId, -- Placeholder logic
            hs.CONTENT as Synthesis,
            h.SHIFT_NAME as ShiftName,
            h.CREATED_BY as CreatedBy,
            h.RECEIVER_USER_ID as AssignedTo,
            NULL as CreatedByName, -- Join users if needed
            NULL as AssignedToName, -- Join users if needed
          h.RECEIVER_USER_ID as ReceiverUserId,
          COALESCE(h.RESPONSIBLE_PHYSICIAN_ID, h.CREATED_BY) as ResponsiblePhysicianId,
          'Dr. Name' as ResponsiblePhysicianName, -- Join users if needed
          TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
            TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
            TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
            TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcknowledgedAt,
            TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcceptedAt,
            TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
            TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
            TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as RejectedAt,
            h.REJECTION_REASON as RejectionReason,
            TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as ExpiredAt,
            h.HANDOVER_TYPE as HandoverType,
            h.HANDOVER_WINDOW_DATE as HandoverWindowDate,
            h.FROM_SHIFT_ID as FromShiftId,
            h.TO_SHIFT_ID as ToShiftId,
            h.TO_DOCTOR_ID as ToDoctorId,
            h.STATUS as StateName, -- Using Status as StateName for now
            1 as Version,
            ROW_NUMBER() OVER (ORDER BY h.CREATED_AT DESC) AS RN
        FROM HANDOVERS h
        JOIN PATIENTS p ON h.PATIENT_ID = p.ID
        LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
        LEFT JOIN HANDOVER_SYNTHESIS hs ON h.ID = hs.HANDOVER_ID
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
          h.ASSIGNMENT_ID as AssignmentId,
          h.PATIENT_ID as PatientId,
          p.NAME as PatientName,
          h.STATUS,
          COALESCE(hpd.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
          hpd.SUMMARY_TEXT as PatientSummary,
          h.ID || '-sa' as SituationAwarenessDocId, -- Placeholder logic
          hs.CONTENT as Synthesis,
          h.SHIFT_NAME as ShiftName,
          h.CREATED_BY as CreatedBy,
          h.RECEIVER_USER_ID as AssignedTo,
          NULL as CreatedByName, -- Join users if needed
          NULL as AssignedToName, -- Join users if needed
          h.RECEIVER_USER_ID as ReceiverUserId,
          COALESCE(h.RESPONSIBLE_PHYSICIAN_ID, h.CREATED_BY) as ResponsiblePhysicianId,
          'Dr. Name' as ResponsiblePhysicianName, -- Join users if needed
          TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
          TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
          TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
          TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcknowledgedAt,
          TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcceptedAt,
          TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
          TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
          TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as RejectedAt,
          h.REJECTION_REASON as RejectionReason,
          TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as ExpiredAt,
          h.HANDOVER_TYPE as HandoverType,
          h.HANDOVER_WINDOW_DATE as HandoverWindowDate,
          h.FROM_SHIFT_ID as FromShiftId,
          h.TO_SHIFT_ID as ToShiftId,
          h.TO_DOCTOR_ID as ToDoctorId,
          h.STATUS as StateName, -- Using Status as StateName for now
          1 as Version
      FROM HANDOVERS h
      JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
      LEFT JOIN HANDOVER_SYNTHESIS hs ON h.ID = hs.HANDOVER_ID
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
          h.STATUS,
          h.CREATED_BY,
          h.RECEIVER_USER_ID,
          NULL as CreatedByName, -- Join USERS if needed, or fetch separately
          NULL as ReceiverName, -- Join USERS if needed
          p.NAME,
          TO_CHAR(p.DATE_OF_BIRTH, 'YYYY-MM-DD') as Dob,
          p.MRN,
          TO_CHAR(p.ADMISSION_DATE, 'YYYY-MM-DD HH24:MI:SS') as AdmissionDate,
          p.ROOM_NUMBER,
          p.DIAGNOSIS,
          u.NAME as UnitName,
          hpd.ILLNESS_SEVERITY,
          hpd.SUMMARY_TEXT,
          hpd.LAST_EDITED_BY,
          TO_CHAR(hpd.UPDATED_AT, 'YYYY-MM-DD HH24:MI:SS') as UpdatedAt
      FROM HANDOVERS h
      JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      JOIN UNITS u ON p.UNIT_ID = u.ID
      LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
      WHERE h.ID = :HandoverId";

    var data = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { HandoverId = handoverId });

    if (data == null) return null;

    string createdBy = (string)data.CREATED_BY;
    string receiverId = (string?)data.RECEIVER_USER_ID ?? "";
    
    // Fetch Physician Names (Assuming stored in USERS table or just mocking for now if table not populated/joined)
    // Better to join USERS in the main query if possible, but let's do separate for clarity or if USERS is in another service (it is in DB here)
    
    var creator = await GetPhysicianInfo(conn, createdBy, (string)data.STATUS, "creator");
    var receiver = !string.IsNullOrEmpty(receiverId) ? await GetPhysicianInfo(conn, receiverId, (string)data.STATUS, "assignee") : null;

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
            UPDATE SET ua.ASSIGNED_AT = SYSTIMESTAMP
          WHEN NOT MATCHED THEN
            INSERT (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
            VALUES (src.ASSIGNMENT_ID, src.USER_ID, src.SHIFT_ID, src.PATIENT_ID, SYSTIMESTAMP)",
          new { assignmentId, toDoctorId = request.ToDoctorId, toShiftId = request.ToShiftId, patientId = request.PatientId });

        // Create handover
        await conn.ExecuteAsync(@"
          INSERT INTO HANDOVERS (
            ID, ASSIGNMENT_ID, PATIENT_ID, STATUS,
            SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID,
            CREATED_BY, CREATED_AT, INITIATED_AT, HANDOVER_TYPE, HANDOVER_WINDOW_DATE,
            RESPONSIBLE_PHYSICIAN_ID
          ) VALUES (
            :handoverId, :assignmentId, :patientId, 'Draft',
            :shiftName, :fromShiftId, :toShiftId, :fromDoctorId, :toDoctorId,
            :initiatedBy, SYSTIMESTAMP, SYSTIMESTAMP, 'ShiftToShift', TRUNC(SYSDATE),
            :initiatedBy
          )",
          new {
            handoverId,
            assignmentId,
            patientId = request.PatientId,
            shiftName = $"{request.FromShiftId} -> {request.ToShiftId}",
            fromShiftId = request.FromShiftId,
            toShiftId = request.ToShiftId,
            fromDoctorId = request.FromDoctorId,
            toDoctorId = request.ToDoctorId,
            initiatedBy = request.InitiatedBy
          });

        // Add participants
        await conn.ExecuteAsync(@"
          INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, USER_NAME, USER_ROLE, STATUS, JOINED_AT)
          VALUES (:participantId1, :handoverId, :fromDoctorId, 'Doctor A', 'Handing Over Doctor', 'active', SYSTIMESTAMP)",
          new {
            handoverId,
            fromDoctorId = request.FromDoctorId,
            participantId1 = $"participant-{Guid.NewGuid().ToString().Substring(0, 8)}"
          });
        
        await conn.ExecuteAsync(@"
          INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, USER_NAME, USER_ROLE, STATUS, JOINED_AT)
          VALUES (:participantId2, :handoverId, :toDoctorId, 'Doctor B', 'Receiving Doctor', 'active', SYSTIMESTAMP)",
          new {
            handoverId,
            toDoctorId = request.ToDoctorId,
            participantId2 = $"participant-{Guid.NewGuid().ToString().Substring(0, 8)}"
          });

        // Create default singleton sections
        await conn.ExecuteAsync(@"
          INSERT INTO HANDOVER_PATIENT_DATA (HANDOVER_ID, ILLNESS_SEVERITY, SUMMARY_TEXT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT)
          VALUES (:handoverId, 'Stable', '', 'draft', :initiatedBy, SYSTIMESTAMP, SYSTIMESTAMP)",
          new { handoverId, initiatedBy = request.InitiatedBy });

        await conn.ExecuteAsync(@"
          INSERT INTO HANDOVER_SITUATION_AWARENESS (HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT)
          VALUES (:handoverId, '', 'draft', :initiatedBy, SYSTIMESTAMP, SYSTIMESTAMP)",
          new { handoverId, initiatedBy = request.InitiatedBy });

        await conn.ExecuteAsync(@"
          INSERT INTO HANDOVER_SYNTHESIS (HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT)
          VALUES (:handoverId, '', 'draft', :initiatedBy, SYSTIMESTAMP, SYSTIMESTAMP)",
          new { handoverId, initiatedBy = request.InitiatedBy });

        // Use the SAME connection to fetch the created handover to avoid isolation/consistency issues
        const string fetchSql = @"
          SELECT
              h.ID,
              h.ASSIGNMENT_ID as AssignmentId,
              h.PATIENT_ID as PatientId,
              p.NAME as PatientName,
              h.STATUS,
              COALESCE(hpd.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
              hpd.SUMMARY_TEXT as PatientSummary,
              h.ID || '-sa' as SituationAwarenessDocId,
              hs.CONTENT as Synthesis,
              h.SHIFT_NAME as ShiftName,
              h.CREATED_BY as CreatedBy,
              h.RECEIVER_USER_ID as AssignedTo,
              NULL as CreatedByName,
              NULL as AssignedToName,
              h.RECEIVER_USER_ID as ReceiverUserId,
              COALESCE(h.RESPONSIBLE_PHYSICIAN_ID, h.CREATED_BY) as ResponsiblePhysicianId,
              'Dr. Name' as ResponsiblePhysicianName,
              TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
              TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
              TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
              TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcknowledgedAt,
              TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcceptedAt,
              TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
              TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
              TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as RejectedAt,
              h.REJECTION_REASON as RejectionReason,
              TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as ExpiredAt,
              h.HANDOVER_TYPE as HandoverType,
              h.HANDOVER_WINDOW_DATE as HandoverWindowDate,
              h.FROM_SHIFT_ID as FromShiftId,
              h.TO_SHIFT_ID as ToShiftId,
              h.TO_DOCTOR_ID as ToDoctorId,
              h.STATUS as StateName,
              1 as Version
          FROM HANDOVERS h
          LEFT JOIN PATIENTS p ON h.PATIENT_ID = p.ID
          LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
          LEFT JOIN HANDOVER_SYNTHESIS hs ON h.ID = hs.HANDOVER_ID
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
        VALUES (:id, :handoverId, :condition, :action, :priority, 'active', :createdBy, SYSTIMESTAMP, SYSTIMESTAMP)";

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
        SELECT HANDOVER_ID as HandoverId, CONTENT as Content, STATUS,
               LAST_EDITED_BY as LastEditedBy, CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
        FROM HANDOVER_SYNTHESIS
        WHERE HANDOVER_ID = :handoverId";

    var result = await conn.QueryFirstOrDefaultAsync<HandoverSynthesisRecord>(sql, new { handoverId });

    // If no record exists but handover does, create default (mimicking legacy behavior)
    if (result == null)
    {
        // Check if handover exists first to avoid FK error or creating orphan data
        var createdBy = await conn.ExecuteScalarAsync<string>(
            "SELECT CREATED_BY FROM HANDOVERS WHERE ID = :handoverId",
            new { handoverId });

        if (createdBy == null) return null; // Handover doesn't exist

        await conn.ExecuteAsync(@"
            INSERT INTO HANDOVER_SYNTHESIS (
                HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT
            ) VALUES (
                :handoverId, '', 'Draft', :createdBy, SYSTIMESTAMP, SYSTIMESTAMP
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
        const string sql = @"
            MERGE INTO HANDOVER_SYNTHESIS s
            USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (s.HANDOVER_ID = src.HANDOVER_ID)
            WHEN MATCHED THEN
                UPDATE SET CONTENT = :content, STATUS = :status, LAST_EDITED_BY = :userId, UPDATED_AT = SYSTIMESTAMP
            WHEN NOT MATCHED THEN
                INSERT (HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT)
                VALUES (:handoverId, :content, :status, :userId, SYSTIMESTAMP, SYSTIMESTAMP)";

        var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, content, status, userId });
        return rowsAffected > 0;
    }
    catch (Exception)
    {
        // Log error?
        // If FK violation (handover doesn't exist), it might throw.
        // For MERGE, if handover doesn't exist in HANDOVERS table, the INSERT might fail due to FK constraint if defined.
        // Assuming FK exists on HANDOVER_SYNTHESIS.HANDOVER_ID -> HANDOVERS.ID
        return false;
    }
  }

  public async Task<HandoverSituationAwarenessRecord?> GetSituationAwarenessAsync(string handoverId)
  {
    using var conn = _connectionFactory.CreateConnection();
    const string sql = @"
        SELECT HANDOVER_ID as HandoverId, CONTENT as Content, STATUS,
               LAST_EDITED_BY as LastEditedBy, CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
        FROM HANDOVER_SITUATION_AWARENESS
        WHERE HANDOVER_ID = :handoverId";

    var result = await conn.QueryFirstOrDefaultAsync<HandoverSituationAwarenessRecord>(sql, new { handoverId });

    if (result == null)
    {
        // Check if handover exists first
        var createdBy = await conn.ExecuteScalarAsync<string>(
            "SELECT CREATED_BY FROM HANDOVERS WHERE ID = :handoverId",
            new { handoverId });

        if (createdBy == null) return null; 

        await conn.ExecuteAsync(@"
            INSERT INTO HANDOVER_SITUATION_AWARENESS (
                HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT
            ) VALUES (
                :handoverId, '', 'Draft', :createdBy, SYSTIMESTAMP, SYSTIMESTAMP
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
        const string sql = @"
            MERGE INTO HANDOVER_SITUATION_AWARENESS sa
            USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (sa.HANDOVER_ID = src.HANDOVER_ID)
            WHEN MATCHED THEN
                UPDATE SET CONTENT = :content, STATUS = :status, LAST_EDITED_BY = :userId, UPDATED_AT = SYSTIMESTAMP
            WHEN NOT MATCHED THEN
                INSERT (HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT)
                VALUES (:handoverId, :content, :status, :userId, SYSTIMESTAMP, SYSTIMESTAMP)";

        var rowsAffected = await conn.ExecuteAsync(sql, new { handoverId, content, status, userId });
        return rowsAffected > 0;
    }
    catch (Exception)
    {
        return false;
    }
  }

  public async Task<bool> MarkAsReadyAsync(string handoverId, string userId)
  {
    using var conn = _connectionFactory.CreateConnection();
    const string sql = @"
        UPDATE HANDOVERS
        SET STATUS = 'Ready', 
            READY_AT = SYSTIMESTAMP, 
            UPDATED_AT = SYSTIMESTAMP
        WHERE ID = :handoverId";

    var rows = await conn.ExecuteAsync(sql, new { handoverId });
    return rows > 0;
  }

  public async Task<HandoverClinicalDataRecord?> GetClinicalDataAsync(string handoverId)
  {
    using var conn = _connectionFactory.CreateConnection();
    const string sql = @"
        SELECT HANDOVER_ID as HandoverId, ILLNESS_SEVERITY as IllnessSeverity, 
               SUMMARY_TEXT as SummaryText, LAST_EDITED_BY as LastEditedBy, 
               STATUS, CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
        FROM HANDOVER_PATIENT_DATA
        WHERE HANDOVER_ID = :handoverId";

    var result = await conn.QueryFirstOrDefaultAsync<HandoverClinicalDataRecord>(sql, new { handoverId });

    if (result == null)
    {
        // Check if handover exists
        var exists = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM HANDOVERS WHERE ID = :handoverId", new { handoverId }) > 0;
        if (!exists) return null;

        // Create default if missing
        var userId = await conn.ExecuteScalarAsync<string>("SELECT CREATED_BY FROM HANDOVERS WHERE ID = :handoverId", new { handoverId });
        
        await conn.ExecuteAsync(@"
            INSERT INTO HANDOVER_PATIENT_DATA (HANDOVER_ID, ILLNESS_SEVERITY, SUMMARY_TEXT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT)
            VALUES (:handoverId, 'Stable', '', 'draft', :userId, SYSTIMESTAMP, SYSTIMESTAMP)",
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
            MERGE INTO HANDOVER_PATIENT_DATA pd
            USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (pd.HANDOVER_ID = src.HANDOVER_ID)
            WHEN MATCHED THEN
                UPDATE SET ILLNESS_SEVERITY = :illnessSeverity, SUMMARY_TEXT = :summaryText, 
                           LAST_EDITED_BY = :userId, UPDATED_AT = SYSTIMESTAMP
            WHEN NOT MATCHED THEN
                INSERT (HANDOVER_ID, ILLNESS_SEVERITY, SUMMARY_TEXT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT)
                VALUES (:handoverId, :illnessSeverity, :summaryText, 'draft', :userId, SYSTIMESTAMP, SYSTIMESTAMP)";

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
      const string sql = @"
          UPDATE HANDOVERS
          SET STATUS = 'Rejected', 
              REJECTION_REASON = :reason,
              REJECTED_AT = SYSTIMESTAMP, 
              UPDATED_AT = SYSTIMESTAMP
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
      const string sql = @"
          UPDATE HANDOVERS
          SET STATUS = 'Completed', 
              COMPLETED_AT = SYSTIMESTAMP, 
              COMPLETED_BY = :userId,
              UPDATED_AT = SYSTIMESTAMP
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
            h.ASSIGNMENT_ID as AssignmentId,
            h.PATIENT_ID as PatientId,
            p.NAME as PatientName,
            h.STATUS,
          COALESCE(hpd.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
          hpd.SUMMARY_TEXT as PatientSummary,
            h.ID || '-sa' as SituationAwarenessDocId,
            hs.CONTENT as Synthesis,
            h.SHIFT_NAME as ShiftName,
            h.CREATED_BY as CreatedBy,
            h.RECEIVER_USER_ID as AssignedTo,
            NULL as CreatedByName,
            NULL as AssignedToName,
            h.RECEIVER_USER_ID as ReceiverUserId,
            COALESCE(h.RESPONSIBLE_PHYSICIAN_ID, h.CREATED_BY) as ResponsiblePhysicianId,
            'Dr. Name' as ResponsiblePhysicianName,
            TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
            TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
            TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
            TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcknowledgedAt,
            TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcceptedAt,
            TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
            TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
            TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as RejectedAt,
            h.REJECTION_REASON as RejectionReason,
            TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as ExpiredAt,
            h.HANDOVER_TYPE as HandoverType,
            h.HANDOVER_WINDOW_DATE as HandoverWindowDate,
            h.FROM_SHIFT_ID as FromShiftId,
            h.TO_SHIFT_ID as ToShiftId,
            h.TO_DOCTOR_ID as ToDoctorId,
            h.STATUS as StateName,
            1 as Version
        FROM HANDOVERS h
        JOIN PATIENTS p ON h.PATIENT_ID = p.ID
        LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
        LEFT JOIN HANDOVER_SYNTHESIS hs ON h.ID = hs.HANDOVER_ID
        WHERE (h.TO_DOCTOR_ID = :userId OR h.RECEIVER_USER_ID = :userId)
          AND h.STATUS IN ('Draft', 'Ready', 'InProgress')";

    var handovers = await conn.QueryAsync<HandoverRecord>(sql, new { userId });
    return handovers.ToList();
  }

  private async Task<bool> UpdateHandoverStatus(string handoverId, string status, string timestampColumn, string userId)
  {
    using var conn = _connectionFactory.CreateConnection();
    // Note: userId is not always used in simple status updates unless we log it (not shown here for brevity)
    string sql = $@"
        UPDATE HANDOVERS
        SET STATUS = :status, 
            {timestampColumn} = SYSTIMESTAMP, 
            UPDATED_AT = SYSTIMESTAMP
        WHERE ID = :handoverId";

    var rows = await conn.ExecuteAsync(sql, new { status, handoverId });
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
      VALUES (:id, :handoverId, :description, 0, SYSTIMESTAMP, SYSTIMESTAMP)";

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
          COMPLETED_AT = CASE WHEN :isCompleted = 1 THEN SYSTIMESTAMP ELSE NULL END,
          UPDATED_AT = SYSTIMESTAMP
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
             NVL(u.FIRST_NAME || ' ' || u.LAST_NAME, 'Unknown') as UserName,
             hal.ACTIVITY_TYPE as ActivityType, hal.ACTIVITY_DESCRIPTION as ActivityDescription,
             hal.SECTION_AFFECTED as SectionAffected, hal.METADATA,
             hal.CREATED_AT as CreatedAt
      FROM HANDOVER_ACTIVITY_LOG hal
      LEFT JOIN USERS u ON hal.USER_ID = u.ID
      WHERE hal.HANDOVER_ID = :handoverId
      ORDER BY hal.CREATED_AT DESC";

    var activities = await conn.QueryAsync<HandoverActivityRecord>(sql, new { handoverId });
    return activities.ToList();
  }

  // Checklists
  public async Task<IReadOnlyList<HandoverChecklistRecord>> GetChecklistsAsync(string handoverId)
  {
    using var conn = _connectionFactory.CreateConnection();

    const string sql = @"
      SELECT ID, HANDOVER_ID as HandoverId, USER_ID as UserId, ITEM_ID as ItemId,
             ITEM_CATEGORY as ItemCategory, ITEM_LABEL as ItemLabel,
             ITEM_DESCRIPTION as ItemDescription, IS_REQUIRED as IsRequired,
             IS_CHECKED as IsChecked, CHECKED_AT as CheckedAt, CREATED_AT as CreatedAt
      FROM HANDOVER_CHECKLISTS
      WHERE HANDOVER_ID = :handoverId
      ORDER BY CREATED_AT ASC";

    var checklists = await conn.QueryAsync<HandoverChecklistRecord>(sql, new { handoverId });
    return checklists.ToList();
  }

  public async Task<bool> UpdateChecklistItemAsync(string handoverId, string itemId, bool isChecked, string userId)
  {
    using var conn = _connectionFactory.CreateConnection();

    const string sql = @"
      UPDATE HANDOVER_CHECKLISTS
      SET IS_CHECKED = :isChecked,
          CHECKED_AT = CASE WHEN :isChecked = 1 THEN SYSTIMESTAMP ELSE NULL END,
          UPDATED_AT = SYSTIMESTAMP
      WHERE HANDOVER_ID = :handoverId AND ITEM_ID = :itemId AND USER_ID = :userId";

    var result = await conn.ExecuteAsync(sql, new { handoverId, itemId, isChecked = isChecked ? 1 : 0, userId });
    return result > 0;
  }

  // Messages
  public async Task<IReadOnlyList<HandoverMessageRecord>> GetMessagesAsync(string handoverId)
  {
    using var conn = _connectionFactory.CreateConnection();

    const string sql = @"
      SELECT ID, HANDOVER_ID as HandoverId, USER_ID as UserId,
             USER_NAME as UserName,
             MESSAGE_TEXT as MessageText, MESSAGE_TYPE as MessageType,
             CREATED_AT as CreatedAt, UPDATED_AT as UpdatedAt
      FROM HANDOVER_MESSAGES
      WHERE HANDOVER_ID = :handoverId
      ORDER BY CREATED_AT ASC";

    var messages = await conn.QueryAsync<HandoverMessageRecord>(sql, new { handoverId });
    return messages.ToList();
  }

  public async Task<HandoverMessageRecord> CreateMessageAsync(string handoverId, string userId, string userName, string messageText, string messageType)
  {
    using var conn = _connectionFactory.CreateConnection();
    var id = Guid.NewGuid().ToString();
    var now = DateTime.UtcNow;

    const string sql = @"
      INSERT INTO HANDOVER_MESSAGES (ID, HANDOVER_ID, USER_ID, USER_NAME, MESSAGE_TEXT, MESSAGE_TYPE, CREATED_AT, UPDATED_AT)
      VALUES (:id, :handoverId, :userId, :userName, :messageText, :messageType, SYSTIMESTAMP, SYSTIMESTAMP)";

    await conn.ExecuteAsync(sql, new { id, handoverId, userId, userName, messageText, messageType });

    return new HandoverMessageRecord(id, handoverId, userId, userName, messageText, messageType, now, now);
  }

  // My Handovers
  public async Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetMyHandoversAsync(string userId, int page, int pageSize)
  {
    using var conn = _connectionFactory.CreateConnection();

    // Get total count - handovers where user is involved (created by or assigned to)
    const string countSql = @"
        SELECT COUNT(1)
        FROM HANDOVERS h
        INNER JOIN USER_ASSIGNMENTS ua ON h.PATIENT_ID = ua.PATIENT_ID
        WHERE ua.USER_ID = :userId";

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
                h.ASSIGNMENT_ID as AssignmentId,
                h.PATIENT_ID as PatientId,
                pt.NAME as PatientName,
                h.STATUS,
                COALESCE(hpd.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
                hpd.SUMMARY_TEXT as PatientSummary,
                h.ID || '-sa' as SituationAwarenessDocId,
            hs.CONTENT as Synthesis,
                h.SHIFT_NAME as ShiftName,
                h.CREATED_BY as CreatedBy,
                h.RECEIVER_USER_ID as AssignedTo,
                cb.FULL_NAME as CreatedByName,
                td.FULL_NAME as AssignedToName,
                h.RECEIVER_USER_ID as ReceiverUserId,
                COALESCE(h.RESPONSIBLE_PHYSICIAN_ID, h.CREATED_BY) as ResponsiblePhysicianId,
                rp.FULL_NAME as ResponsiblePhysicianName,
                TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
                TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
                TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
                TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcknowledgedAt,
                TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as AcceptedAt,
                TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
                TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
                TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as RejectedAt,
                h.REJECTION_REASON as RejectionReason,
                TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as ExpiredAt,
                h.HANDOVER_TYPE as HandoverType,
                h.HANDOVER_WINDOW_DATE as HandoverWindowDate,
                h.FROM_SHIFT_ID as FromShiftId,
                h.TO_SHIFT_ID as ToShiftId,
                h.TO_DOCTOR_ID as ToDoctorId,
                h.STATUS as StateName,
                1 as Version,
                ROW_NUMBER() OVER (ORDER BY h.CREATED_AT DESC) AS RN
            FROM HANDOVERS h
            INNER JOIN USER_ASSIGNMENTS ua ON h.PATIENT_ID = ua.PATIENT_ID
            LEFT JOIN PATIENTS pt ON h.PATIENT_ID = pt.ID
            LEFT JOIN USERS cb ON h.CREATED_BY = cb.ID
            LEFT JOIN USERS td ON h.TO_DOCTOR_ID = td.ID
            LEFT JOIN USERS rp ON rp.ID = COALESCE(h.RESPONSIBLE_PHYSICIAN_ID, h.CREATED_BY)
            LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
            LEFT JOIN HANDOVER_SYNTHESIS hs ON h.ID = hs.HANDOVER_ID
            WHERE ua.USER_ID = :userId
        )
        WHERE RN > :offset AND RN <= :maxRow";

    var handovers = await conn.QueryAsync<HandoverRecord>(sql, new { userId, offset, maxRow = offset + ps });

    return (handovers.ToList(), total);
  }
}
