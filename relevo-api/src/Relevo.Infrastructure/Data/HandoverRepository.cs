using System.Data;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

public class HandoverRepository(
    DapperConnectionFactory _connectionFactory,
    IShiftInstanceRepository _shiftInstanceRepository, // Will be used in CreateHandoverAsync and other methods
    IShiftWindowRepository _shiftWindowRepository) : IHandoverRepository // Will be used in CreateHandoverAsync and other methods
{
    private IShiftInstanceRepository ShiftInstanceRepository => _shiftInstanceRepository;
    private IShiftWindowRepository ShiftWindowRepository => _shiftWindowRepository;
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

    // Query - V3 Schema: Uses SHIFT_WINDOW_ID, SENDER_USER_ID, RECEIVER_USER_ID, etc.
    const string sql = @"
      SELECT * FROM (
        SELECT
            h.ID,
            h.PATIENT_ID as PatientId,
            p.NAME as PatientName,
            h.CURRENT_STATE as Status, -- Virtual column
            COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
            hc.PATIENT_SUMMARY as PatientSummary,
            h.ID || '-sa' as SituationAwarenessDocId,
            hc.SYNTHESIS as Synthesis,
            s_from.NAME as ShiftName, -- From shift name
            h.CREATED_BY_USER_ID as CreatedBy, -- V3: CREATED_BY_USER_ID
            COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) as AssignedTo, -- V3: COMPLETED_BY_USER_ID or RECEIVER_USER_ID
            NULL as CreatedByName, -- Join users if needed
            NULL as AssignedToName, -- Join users if needed
            h.RECEIVER_USER_ID as ReceiverUserId, -- V3: RECEIVER_USER_ID
            h.SENDER_USER_ID as ResponsiblePhysicianId, -- V3: SENDER_USER_ID
            NULL as ResponsiblePhysicianName, -- Join users if needed
            TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
            TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
            TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
            TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
            TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
            TRUNC(si_from.START_AT) as HandoverWindowDate, -- V3: From SHIFT_INSTANCES.START_AT
            h.CURRENT_STATE as StateName,
            1 as Version,
            -- V3 Fields
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
        LEFT JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID -- V3: Join SHIFT_WINDOWS
        LEFT JOIN SHIFT_INSTANCES si_from ON sw.FROM_SHIFT_INSTANCE_ID = si_from.ID -- V3: From shift instance
        LEFT JOIN SHIFT_INSTANCES si_to ON sw.TO_SHIFT_INSTANCE_ID = si_to.ID -- V3: To shift instance
        LEFT JOIN SHIFTS s_from ON si_from.SHIFT_ID = s_from.ID -- V3: From shift template
        LEFT JOIN SHIFTS s_to ON si_to.SHIFT_ID = s_to.ID -- V3: To shift template
        LEFT JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID
        WHERE h.PATIENT_ID = :PatientId
      )
      WHERE RN BETWEEN :StartRow AND :EndRow";

    var handovers = await conn.QueryAsync<HandoverRecord>(sql, new { PatientId = patientId, StartRow = offset + 1, EndRow = offset + ps });

    return (handovers.ToList(), total);
  }

  public async Task<HandoverDetailRecord?> GetHandoverByIdAsync(string handoverId)
  {
    using var conn = _connectionFactory.CreateConnection();

    // V3 Schema: Uses SHIFT_WINDOW_ID, SENDER_USER_ID, RECEIVER_USER_ID, etc.
    const string sql = @"
      SELECT
          h.ID,
          h.PATIENT_ID as PatientId,
          p.NAME as PatientName,
          h.CURRENT_STATE as Status, -- Virtual column
          COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
          hc.PATIENT_SUMMARY as PatientSummary,
          h.ID || '-sa' as SituationAwarenessDocId,
          hc.SYNTHESIS as Synthesis,
          s_from.NAME as ShiftName, -- V3: From shift name
          h.CREATED_BY_USER_ID as CreatedBy, -- V3: CREATED_BY_USER_ID
          COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) as AssignedTo, -- V3: COMPLETED_BY_USER_ID or RECEIVER_USER_ID
          NULL as CreatedByName, -- Join users if needed
          NULL as AssignedToName, -- Join users if needed
          h.RECEIVER_USER_ID as ReceiverUserId, -- V3: RECEIVER_USER_ID
          h.SENDER_USER_ID as ResponsiblePhysicianId, -- V3: SENDER_USER_ID
          NULL as ResponsiblePhysicianName, -- Join users if needed
          TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
          TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
          TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
          TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
          TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
          TRUNC(si_from.START_AT) as HandoverWindowDate, -- V3: From SHIFT_INSTANCES.START_AT
          h.CURRENT_STATE as StateName,
          1 as Version,
          -- V3 Fields
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
      LEFT JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID -- V3: Join SHIFT_WINDOWS
      LEFT JOIN SHIFT_INSTANCES si_from ON sw.FROM_SHIFT_INSTANCE_ID = si_from.ID -- V3: From shift instance
      LEFT JOIN SHIFT_INSTANCES si_to ON sw.TO_SHIFT_INSTANCE_ID = si_to.ID -- V3: To shift instance
      LEFT JOIN SHIFTS s_from ON si_from.SHIFT_ID = s_from.ID -- V3: From shift template
      LEFT JOIN SHIFTS s_to ON si_to.SHIFT_ID = s_to.ID -- V3: To shift template
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

    // V3 Schema: Uses SENDER_USER_ID for AssignedPhysician, RECEIVER_USER_ID for ReceivingPhysician
    const string sql = @"
      SELECT
          h.ID,
          h.PATIENT_ID,
          h.CURRENT_STATE as STATUS, -- Virtual column
          h.SENDER_USER_ID, -- V3: The responsible sender (primary of FROM shift)
          COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) as RECEIVER_USER_ID, -- V3: COMPLETED_BY_USER_ID or RECEIVER_USER_ID
          h.SHIFT_WINDOW_ID,
          p.NAME,
          TO_CHAR(p.DATE_OF_BIRTH, 'YYYY-MM-DD') as Dob,
          p.MRN,
          TO_CHAR(p.ADMISSION_DATE, 'YYYY-MM-DD HH24:MI:SS') as AdmissionDate,
          p.ROOM_NUMBER,
          p.DIAGNOSIS,
          u.NAME as UnitName,
          hc.ILLNESS_SEVERITY, -- From HANDOVER_CONTENTS
          hc.PATIENT_SUMMARY as SUMMARY_TEXT,
          hc.LAST_EDITED_BY, -- From HANDOVER_CONTENTS
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

  public async Task<HandoverRecord> CreateHandoverAsync(CreateHandoverRequest request)
  {
    using var conn = _connectionFactory.CreateConnection();

    // V3: Get patient's UNIT_ID
    var unitId = await conn.ExecuteScalarAsync<string>(
      "SELECT UNIT_ID FROM PATIENTS WHERE ID = :patientId",
      new { patientId = request.PatientId });

    if (string.IsNullOrEmpty(unitId))
    {
      throw new InvalidOperationException($"Patient {request.PatientId} not found or has no unit");
    }

    // V3: Calculate shift instance dates using shared service
    var today = DateTime.Today;
    var shiftDates = await ShiftInstanceCalculationService.CalculateShiftInstanceDatesFromDbAsync(
      conn, request.FromShiftId, request.ToShiftId, today);

    if (shiftDates == null)
    {
      throw new InvalidOperationException($"Shift templates not found: FromShiftId={request.FromShiftId}, ToShiftId={request.ToShiftId}");
    }

    var (fromShiftStartAt, fromShiftEndAt, toShiftStartAt, toShiftEndAt) = shiftDates.Value;

    // V3: Get or create shift instances
    var fromShiftInstanceId = await _shiftInstanceRepository.GetOrCreateShiftInstanceAsync(
      request.FromShiftId, unitId, fromShiftStartAt, fromShiftEndAt);
    
    var toShiftInstanceId = await _shiftInstanceRepository.GetOrCreateShiftInstanceAsync(
      request.ToShiftId, unitId, toShiftStartAt, toShiftEndAt);

    // V3: Get or create shift window
    var shiftWindowId = await _shiftWindowRepository.GetOrCreateShiftWindowAsync(
      fromShiftInstanceId, toShiftInstanceId, unitId);

    // V3: Get SENDER_USER_ID from SHIFT_COVERAGE (primary responsible for FROM shift)
    var senderUserId = await conn.ExecuteScalarAsync<string>(
      @"SELECT RESPONSIBLE_USER_ID
        FROM (
          SELECT RESPONSIBLE_USER_ID
          FROM SHIFT_COVERAGE
          WHERE PATIENT_ID = :patientId
            AND SHIFT_INSTANCE_ID = :shiftInstanceId
            AND IS_PRIMARY = 1
          ORDER BY ASSIGNED_AT ASC
        ) WHERE ROWNUM <= 1",
      new { patientId = request.PatientId, shiftInstanceId = fromShiftInstanceId });

    // If no primary, get the first one
    if (string.IsNullOrEmpty(senderUserId))
    {
      senderUserId = await conn.ExecuteScalarAsync<string>(
        @"SELECT RESPONSIBLE_USER_ID
          FROM (
            SELECT RESPONSIBLE_USER_ID
            FROM SHIFT_COVERAGE
            WHERE PATIENT_ID = :patientId
              AND SHIFT_INSTANCE_ID = :shiftInstanceId
            ORDER BY ASSIGNED_AT ASC
          ) WHERE ROWNUM <= 1",
        new { patientId = request.PatientId, shiftInstanceId = fromShiftInstanceId });
    }

    // V3_PLAN.md regla #10: Cannot create handover without coverage
    // No fallback - throw exception if no coverage exists
    if (string.IsNullOrEmpty(senderUserId))
    {
      throw new InvalidOperationException(
        $"Cannot create handover: patient {request.PatientId} has no coverage in FROM shift instance {fromShiftInstanceId}. " +
        "A handover cannot exist without coverage.");
    }

    // V3: Find previous handover for the same patient (most recent completed handover)
    // The first handover for a patient will have PREVIOUS_HANDOVER_ID = NULL
    var previousHandoverId = await conn.ExecuteScalarAsync<string>(@"
      SELECT ID FROM (
        SELECT ID
        FROM HANDOVERS
        WHERE PATIENT_ID = :patientId
          AND COMPLETED_AT IS NOT NULL
          AND CANCELLED_AT IS NULL
        ORDER BY COMPLETED_AT DESC
      ) WHERE ROWNUM <= 1",
      new { patientId = request.PatientId });

    // V3_PLAN.md Regla #36: Copy PATIENT_SUMMARY from previous handover
    string? previousPatientSummary = null;
    if (!string.IsNullOrEmpty(previousHandoverId))
    {
      previousPatientSummary = await conn.ExecuteScalarAsync<string>(@"
        SELECT PATIENT_SUMMARY
        FROM HANDOVER_CONTENTS
        WHERE HANDOVER_ID = :previousHandoverId",
        new { previousHandoverId });
    }

    // V3_PLAN.md Regla #16: Idempotency via DB constraint UQ_HO_PAT_WINDOW
    // V3_PLAN.md Regla #9: máximo 1 handover activo por paciente por ventana
    // Use MERGE for idempotent insert (Oracle idiom)
    var handoverId = Guid.NewGuid().ToString();
    const string mergeSql = @"
      MERGE INTO HANDOVERS h
      USING (
        SELECT :patientId AS PATIENT_ID, :shiftWindowId AS SHIFT_WINDOW_ID FROM DUAL
      ) src ON (
        h.PATIENT_ID = src.PATIENT_ID AND h.SHIFT_WINDOW_ID = src.SHIFT_WINDOW_ID
      )
      WHEN NOT MATCHED THEN
        INSERT (
          ID, PATIENT_ID, SHIFT_WINDOW_ID, UNIT_ID,
          PREVIOUS_HANDOVER_ID, SENDER_USER_ID, RECEIVER_USER_ID, CREATED_BY_USER_ID,
          CREATED_AT, UPDATED_AT
        ) VALUES (
          :id, :patientId, :shiftWindowId, :unitId,
          :previousHandoverId, :senderUserId, :receiverUserId, :createdByUserId,
          LOCALTIMESTAMP, LOCALTIMESTAMP
        )";

    await conn.ExecuteAsync(mergeSql, new
    {
      id = handoverId,
      patientId = request.PatientId,
      shiftWindowId = shiftWindowId,
      unitId = unitId,
      previousHandoverId = previousHandoverId,
      senderUserId = senderUserId,
      receiverUserId = request.ToDoctorId,
      createdByUserId = request.InitiatedBy
    });

    // Get the actual handover ID (might be existing one if MERGE matched)
    const string getHandoverIdSql = @"
      SELECT ID FROM HANDOVERS
      WHERE PATIENT_ID = :patientId AND SHIFT_WINDOW_ID = :shiftWindowId
        AND ROWNUM <= 1";
    
    var actualHandoverId = await conn.ExecuteScalarAsync<string>(getHandoverIdSql, new
    {
      patientId = request.PatientId,
      shiftWindowId = shiftWindowId
    });

    if (string.IsNullOrEmpty(actualHandoverId))
    {
      throw new InvalidOperationException($"Handover not found after MERGE for PatientId={request.PatientId}, ShiftWindowId={shiftWindowId}");
    }

    // Only create HANDOVER_CONTENTS if this is a new handover (not existing one)
    if (actualHandoverId == handoverId)
    {
      // V3_PLAN.md Regla #36: Copy PATIENT_SUMMARY from previous handover if available
      await conn.ExecuteAsync(@"
        INSERT INTO HANDOVER_CONTENTS (
          HANDOVER_ID, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS, SYNTHESIS,
          PATIENT_SUMMARY_STATUS, SA_STATUS, SYNTHESIS_STATUS, LAST_EDITED_BY, UPDATED_AT
        ) VALUES (
          :handoverId, 'Stable', :patientSummary, NULL, NULL,
          'Draft', 'Draft', 'Draft', :userId, LOCALTIMESTAMP
        )",
        new { handoverId = actualHandoverId, patientSummary = previousPatientSummary, userId = request.InitiatedBy });
    }

    // Fetch and return the handover (existing or newly created)
    var handoverDetail = await GetHandoverByIdAsync(actualHandoverId);
    if (handoverDetail == null)
    {
      throw new InvalidOperationException($"Failed to retrieve handover {actualHandoverId}");
    }

    return handoverDetail.Handover;
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

    // If no record exists but handover does, create default
    if (result == null)
    {
        // Check if handover exists first to avoid FK error or creating orphan data
        var createdBy = await conn.ExecuteScalarAsync<string>(
            "SELECT CREATED_BY_USER_ID FROM HANDOVERS WHERE ID = :handoverId",
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
            "SELECT CREATED_BY_USER_ID FROM HANDOVERS WHERE ID = :handoverId",
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
        
        // Get handover with SHIFT_WINDOW_ID
        const string getHandoverSql = @"
            SELECT SHIFT_WINDOW_ID, PATIENT_ID, SENDER_USER_ID
            FROM HANDOVERS
            WHERE ID = :handoverId";
        
        var handover = await conn.QueryFirstOrDefaultAsync<dynamic>(getHandoverSql, new { handoverId });
        
        if (handover == null)
        {
            return false;
        }

        string? shiftWindowId = handover.SHIFT_WINDOW_ID;
        string patientId = handover.PATIENT_ID;
        string? existingSenderUserId = handover.SENDER_USER_ID;

        // V3_PLAN.md regla #10: Validate coverage >= 1 before Ready (atomic validation)
        string? fromShiftInstanceId = null;
        if (!string.IsNullOrEmpty(shiftWindowId))
        {
            // Get FROM_SHIFT_INSTANCE_ID from SHIFT_WINDOWS
            const string getWindowSql = @"
                SELECT FROM_SHIFT_INSTANCE_ID, UNIT_ID
                FROM SHIFT_WINDOWS
                WHERE ID = :shiftWindowId";
            
            var window = await conn.QueryFirstOrDefaultAsync<dynamic>(getWindowSql, new { shiftWindowId });
            if (window != null)
            {
                fromShiftInstanceId = window.FROM_SHIFT_INSTANCE_ID;
                
                // Validate that coverage >= 1 exists (atomic check)
                var coverageCount = await conn.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(*)
                    FROM SHIFT_COVERAGE
                    WHERE PATIENT_ID = :patientId
                      AND SHIFT_INSTANCE_ID = :fromShiftInstanceId",
                    new { patientId, fromShiftInstanceId });

                if (coverageCount == 0)
                {
                    // V3_PLAN.md regla #10: Cannot pass to Ready without coverage
                    return false;
                }
            }
        }

        // If SENDER_USER_ID is not set, get it from SHIFT_COVERAGE (primary of FROM shift)
        string? senderUserId = existingSenderUserId;
        if (string.IsNullOrEmpty(senderUserId) && !string.IsNullOrEmpty(fromShiftInstanceId))
        {
            // Get primary coverage for this patient and shift instance
            const string getPrimarySql = @"
                SELECT RESPONSIBLE_USER_ID
                FROM SHIFT_COVERAGE
                WHERE PATIENT_ID = :patientId
                  AND SHIFT_INSTANCE_ID = :shiftInstanceId
                  AND IS_PRIMARY = 1
                  AND ROWNUM <= 1";
            
            senderUserId = await conn.ExecuteScalarAsync<string>(getPrimarySql, new { patientId, shiftInstanceId = fromShiftInstanceId });
            
            // If no primary, get the first one by ASSIGNED_AT
            if (string.IsNullOrEmpty(senderUserId))
            {
                const string getFirstSql = @"
                    SELECT RESPONSIBLE_USER_ID
                    FROM (
                        SELECT RESPONSIBLE_USER_ID
                        FROM SHIFT_COVERAGE
                        WHERE PATIENT_ID = :patientId
                          AND SHIFT_INSTANCE_ID = :shiftInstanceId
                        ORDER BY ASSIGNED_AT ASC
                    ) WHERE ROWNUM <= 1";
                
                senderUserId = await conn.ExecuteScalarAsync<string>(getFirstSql, new { patientId, shiftInstanceId = fromShiftInstanceId });
            }
        }

        // Update handover with READY_AT, READY_BY_USER_ID, and SENDER_USER_ID (if not set)
        // Use SYSTIMESTAMP for consistency with test data
        const string sql = @"
            UPDATE HANDOVERS
            SET READY_AT = SYSTIMESTAMP,
                READY_BY_USER_ID = :userId,
                SENDER_USER_ID = COALESCE(SENDER_USER_ID, :senderUserId),
                UPDATED_AT = SYSTIMESTAMP
            WHERE ID = :handoverId
              AND READY_AT IS NULL";

        var rows = await conn.ExecuteAsync(sql, new { handoverId, userId, senderUserId });
        
        // If handover is already Ready, return true (idempotent)
        if (rows == 0)
        {
            var alreadyReady = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM HANDOVERS WHERE ID = :handoverId AND READY_AT IS NOT NULL",
                new { handoverId }) > 0;
            return alreadyReady;
        }
        return rows > 0;
    }
    catch (Exception)
    {
        return false;
    }
  }

  public async Task<bool> ReturnForChangesAsync(string handoverId, string userId)
  {
    try
    {
      using var conn = _connectionFactory.CreateConnection();
      
      // V3_PLAN.md regla #21: ReturnForChanges vuelve a Draft limpiando READY_AT
      // Solo permite si está en Ready (no completado ni cancelado)
      // Use SYSTIMESTAMP for consistency with test data
      const string sql = @"
          UPDATE HANDOVERS
          SET READY_AT = NULL,
              READY_BY_USER_ID = NULL,
              UPDATED_AT = SYSTIMESTAMP
          WHERE ID = :handoverId
            AND READY_AT IS NOT NULL
            AND COMPLETED_AT IS NULL
            AND CANCELLED_AT IS NULL";
      
      var rows = await conn.ExecuteAsync(sql, new { handoverId });
      return rows > 0;
    }
    catch (Exception)
    {
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
        var userId = await conn.ExecuteScalarAsync<string>("SELECT CREATED_BY_USER_ID FROM HANDOVERS WHERE ID = :handoverId", new { handoverId });
        
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
      
      // Get Shift from SHIFT_COVERAGE -> SHIFT_INSTANCES -> SHIFTS
      // Note: This requires a shift instance context, which may not always be available
      // For now, return null shift times if we can't determine them
      const string shiftSql = @"
        SELECT * FROM (
            SELECT s.START_TIME, s.END_TIME
            FROM SHIFT_COVERAGE sc
            JOIN SHIFT_INSTANCES si ON sc.SHIFT_INSTANCE_ID = si.ID
            JOIN SHIFTS s ON si.SHIFT_ID = s.ID
            WHERE sc.RESPONSIBLE_USER_ID = :UserId
            ORDER BY sc.ASSIGNED_AT DESC
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
      using var conn = _connectionFactory.CreateConnection();
      
      // V3: Set STARTED_AT and STARTED_BY_USER_ID
      // Constraint CHK_HO_STARTED_NE_SENDER ensures STARTED_BY_USER_ID <> SENDER_USER_ID
      // Use SYSTIMESTAMP for consistency with test data
      const string sql = @"
          UPDATE HANDOVERS
          SET STARTED_AT = SYSTIMESTAMP,
              STARTED_BY_USER_ID = :userId,
              UPDATED_AT = SYSTIMESTAMP
          WHERE ID = :handoverId
            AND STARTED_AT IS NULL
            AND READY_AT IS NOT NULL";

      var rows = await conn.ExecuteAsync(sql, new { handoverId, userId });
      return rows > 0;
    }
    catch (Oracle.ManagedDataAccess.Client.OracleException ex) when (ex.Number == 2290)
    {
      // ORA-02290: check constraint violated - state machine constraint or STARTED_BY_USER_ID <> SENDER_USER_ID
      return false;
    }
    catch (Exception ex)
    {
      // Log other exceptions for debugging
      System.Diagnostics.Debug.WriteLine($"StartHandoverAsync exception: {ex.Message}");
      return false;
    }
  }

  public async Task<bool> RejectHandoverAsync(string handoverId, string cancelReason, string userId)
  {
    // V3: Reject uses Cancel with CANCEL_REASON='ReceiverRefused'
    // The cancelReason parameter should be 'ReceiverRefused' or similar
    return await CancelHandoverAsync(handoverId, cancelReason, userId);
  }

  public async Task<bool> CancelHandoverAsync(string handoverId, string cancelReason, string userId)
  {
    try
    {
      using var conn = _connectionFactory.CreateConnection();
      
      // V3: Set CANCELLED_AT, CANCELLED_BY_USER_ID, and CANCEL_REASON
      // Constraint CHK_HO_CAN_BY_REQ and CHK_HO_CAN_RSN_REQ require both
      // Use SYSTIMESTAMP for consistency with test data
      const string sql = @"
          UPDATE HANDOVERS
          SET CANCELLED_AT = SYSTIMESTAMP,
              CANCELLED_BY_USER_ID = :userId,
              CANCEL_REASON = :cancelReason,
              UPDATED_AT = SYSTIMESTAMP
          WHERE ID = :handoverId
            AND CANCELLED_AT IS NULL
            AND COMPLETED_AT IS NULL";

      var rows = await conn.ExecuteAsync(sql, new { handoverId, userId, cancelReason });
      return rows > 0;
    }
    catch (Oracle.ManagedDataAccess.Client.OracleException ex) when (ex.Number == 2290)
    {
      // ORA-02290: check constraint violated
      return false;
    }
    catch (Exception ex)
    {
      // Log other exceptions for debugging
      System.Diagnostics.Debug.WriteLine($"CancelHandoverAsync exception: {ex.Message}");
      return false;
    }
  }

  public async Task<bool> CompleteHandoverAsync(string handoverId, string userId)
  {
    try
    {
      using var conn = _connectionFactory.CreateConnection();
      
      // V3: Set COMPLETED_AT and COMPLETED_BY_USER_ID
      // Constraint CHK_HO_COMPLETED_NE_SENDER ensures COMPLETED_BY_USER_ID <> SENDER_USER_ID
      // App-enforced: validate that userId has coverage in TO shift (not done here, should be in handler)
      // Use SYSTIMESTAMP for consistency with test data
      const string sql = @"
          UPDATE HANDOVERS
          SET COMPLETED_AT = SYSTIMESTAMP,
              COMPLETED_BY_USER_ID = :userId,
              UPDATED_AT = SYSTIMESTAMP
          WHERE ID = :handoverId
            AND COMPLETED_AT IS NULL
            AND CANCELLED_AT IS NULL
            AND STARTED_AT IS NOT NULL";

      var rows = await conn.ExecuteAsync(sql, new { handoverId, userId });
      return rows > 0;
    }
    catch (Oracle.ManagedDataAccess.Client.OracleException ex) when (ex.Number == 2290)
    {
      // ORA-02290: check constraint violated - state machine constraint or COMPLETED_BY_USER_ID <> SENDER_USER_ID
      return false;
    }
    catch (Exception ex)
    {
      // Log other exceptions for debugging
      System.Diagnostics.Debug.WriteLine($"CompleteHandoverAsync exception: {ex.Message}");
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
    
    // V3 Schema: Uses SHIFT_WINDOW_ID, SENDER_USER_ID, RECEIVER_USER_ID, etc.
    const string sql = @"
        SELECT
            h.ID,
            h.PATIENT_ID as PatientId,
            p.NAME as PatientName,
            h.CURRENT_STATE as Status, -- Virtual column
            COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
            hc.PATIENT_SUMMARY as PatientSummary,
            h.ID || '-sa' as SituationAwarenessDocId,
            hc.SYNTHESIS as Synthesis,
            s_from.NAME as ShiftName, -- V3: From shift name
            h.CREATED_BY_USER_ID as CreatedBy, -- V3: CREATED_BY_USER_ID
            COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) as AssignedTo, -- V3: COMPLETED_BY_USER_ID or RECEIVER_USER_ID
            NULL as CreatedByName,
            NULL as AssignedToName,
            h.RECEIVER_USER_ID as ReceiverUserId, -- V3: RECEIVER_USER_ID
            h.SENDER_USER_ID as ResponsiblePhysicianId, -- V3: SENDER_USER_ID
            NULL as ResponsiblePhysicianName,
            TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
            TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
            TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
            TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
            TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
            TRUNC(si_from.START_AT) as HandoverWindowDate, -- V3: From SHIFT_INSTANCES.START_AT
            h.CURRENT_STATE as StateName,
            1 as Version,
            -- V3 Fields
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
        LEFT JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID -- V3: Join SHIFT_WINDOWS
        LEFT JOIN SHIFT_INSTANCES si_from ON sw.FROM_SHIFT_INSTANCE_ID = si_from.ID -- V3: From shift instance
        LEFT JOIN SHIFT_INSTANCES si_to ON sw.TO_SHIFT_INSTANCE_ID = si_to.ID -- V3: To shift instance
        LEFT JOIN SHIFTS s_from ON si_from.SHIFT_ID = s_from.ID -- V3: From shift template
        LEFT JOIN SHIFTS s_to ON si_to.SHIFT_ID = s_to.ID -- V3: To shift template
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
    // V3: Only mechanical states exist (Draft, Ready, InProgress, Completed, Cancelled)
    // Rejected and Expired were removed - rejection is modeled as Cancelled with CANCEL_REASON='ReceiverRefused'
    state = state?.ToLower() ?? "";
    return state switch
    {
      "completed" => "completed",
      "cancelled" => "cancelled",
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
      LEFT JOIN USERS u ON m.USER_ID = u.ID -- Join USERS to get UserName (FULL_NAME or FIRST_NAME + LAST_NAME)
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

    // V3 Schema: Get handovers where user is involved (SENDER_USER_ID, RECEIVER_USER_ID, CREATED_BY_USER_ID, COMPLETED_BY_USER_ID)
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

    // V3 Schema: Uses SHIFT_WINDOW_ID, SENDER_USER_ID, RECEIVER_USER_ID, etc.
    const string sql = @"
        SELECT * FROM (
            SELECT
                h.ID,
                h.PATIENT_ID as PatientId,
                pt.NAME as PatientName,
                h.CURRENT_STATE as Status, -- Virtual column
                COALESCE(hc.ILLNESS_SEVERITY, 'Stable') as IllnessSeverity,
                hc.PATIENT_SUMMARY as PatientSummary,
                h.ID || '-sa' as SituationAwarenessDocId,
                hc.SYNTHESIS as Synthesis,
                s_from.NAME as ShiftName, -- V3: From shift name
                h.CREATED_BY_USER_ID as CreatedBy, -- V3: CREATED_BY_USER_ID
                COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) as AssignedTo, -- V3: COMPLETED_BY_USER_ID or RECEIVER_USER_ID
                cb.FULL_NAME as CreatedByName,
                td.FULL_NAME as AssignedToName,
                h.RECEIVER_USER_ID as ReceiverUserId, -- V3: RECEIVER_USER_ID
                h.SENDER_USER_ID as ResponsiblePhysicianId, -- V3: SENDER_USER_ID
                rp.FULL_NAME as ResponsiblePhysicianName,
                TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CreatedAt,
                TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as ReadyAt,
                TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as StartedAt,
                TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as CompletedAt,
                TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CancelledAt,
                TRUNC(si_from.START_AT) as HandoverWindowDate, -- V3: From SHIFT_INSTANCES.START_AT
                h.CURRENT_STATE as StateName,
                1 as Version,
                -- V3 Fields
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
            LEFT JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID -- V3: Join SHIFT_WINDOWS
            LEFT JOIN SHIFT_INSTANCES si_from ON sw.FROM_SHIFT_INSTANCE_ID = si_from.ID -- V3: From shift instance
            LEFT JOIN SHIFT_INSTANCES si_to ON sw.TO_SHIFT_INSTANCE_ID = si_to.ID -- V3: To shift instance
            LEFT JOIN SHIFTS s_from ON si_from.SHIFT_ID = s_from.ID -- V3: From shift template
            LEFT JOIN SHIFTS s_to ON si_to.SHIFT_ID = s_to.ID -- V3: To shift template
            LEFT JOIN USERS cb ON h.CREATED_BY_USER_ID = cb.ID -- V3: CREATED_BY_USER_ID
            LEFT JOIN USERS td ON COALESCE(h.COMPLETED_BY_USER_ID, h.RECEIVER_USER_ID) = td.ID -- V3: COMPLETED_BY_USER_ID or RECEIVER_USER_ID
            LEFT JOIN USERS rp ON h.SENDER_USER_ID = rp.ID -- V3: SENDER_USER_ID
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

  // Get current handover for patient (read-only, no side effects)
  // Returns the handover ID of the latest non-terminal handover, or null if none exists
  // V3 Schema: Uses SHIFT_WINDOWS to get window start date
  public async Task<string?> GetCurrentHandoverIdAsync(string patientId)
  {
    using var conn = _connectionFactory.CreateConnection();

    // V3: Get latest non-terminal handover for patient
    // Terminal states in V3: 'Completed', 'Cancelled' (Rejected and Expired removed)
    // Order by SHIFT_INSTANCES.START_AT from SHIFT_WINDOWS
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

    // V3: Get TO_SHIFT_INSTANCE_ID from SHIFT_WINDOWS
    // Then verify that userId has coverage in SHIFT_COVERAGE for that shift instance and patient
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

}
