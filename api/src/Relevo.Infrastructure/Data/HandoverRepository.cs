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
            hpd.ILLNESS_SEVERITY as Severity,
            hpd.SUMMARY_TEXT as PatientSummaryContent,
            h.ID || '-sa' as SituationAwarenessDocId, -- Placeholder logic
            hs.CONTENT as SynthesisContent,
            h.SHIFT_NAME as ShiftName,
            h.CREATED_BY as CreatedBy,
            h.RECEIVER_USER_ID as AssignedTo,
            NULL as CreatedByName, -- Join users if needed
            NULL as AssignedToName, -- Join users if needed
            h.RECEIVER_USER_ID as ReceiverUserId,
            h.RESPONSIBLE_PHYSICIAN_ID as ResponsiblePhysicianId,
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

    var handovers = await conn.QueryAsync<dynamic>(sql, new { PatientId = patientId, StartRow = offset + 1, EndRow = offset + ps });

    var result = handovers.Select(MapHandoverRecord).ToList();

    return (result, total);
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
          hpd.ILLNESS_SEVERITY as Severity,
          hpd.SUMMARY_TEXT as PatientSummaryContent,
          h.ID || '-sa' as SituationAwarenessDocId, -- Placeholder logic
          hs.CONTENT as SynthesisContent,
          h.SHIFT_NAME as ShiftName,
          h.CREATED_BY as CreatedBy,
          h.RECEIVER_USER_ID as AssignedTo,
          NULL as CreatedByName, -- Join users if needed
          NULL as AssignedToName, -- Join users if needed
          h.RECEIVER_USER_ID as ReceiverUserId,
          h.RESPONSIBLE_PHYSICIAN_ID as ResponsiblePhysicianId,
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

    var handover = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { HandoverId = handoverId });

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

    var dtos = await conn.QueryAsync<ActionItemDto>(sqlActionItems, new { HandoverId = handoverId });
    var actionItems = dtos.Select(d => new ActionItemRecord(d.Id, d.Description, d.IsCompleted == 1)).ToList();

    return new HandoverDetailRecord(MapHandoverRecord(handover), actionItems);
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

  private class ActionItemDto
  {
      public string Id { get; set; } = string.Empty;
      public string Description { get; set; } = string.Empty;
      public int IsCompleted { get; set; }
  }

  private HandoverRecord MapHandoverRecord(dynamic h)
  {
      return new HandoverRecord(
        (string)h.ID,
        (string)h.ASSIGNMENTID,
        (string)h.PATIENTID,
        (string?)h.PATIENTNAME,
        (string)h.STATUS,
        new HandoverIllnessSeverity((string?)h.SEVERITY ?? "Stable"),
        new HandoverPatientSummary((string?)h.PATIENTSUMMARYCONTENT ?? ""),
        (string?)h.SITUATIONAWARENESSDOCID,
        h.SYNTHESISCONTENT != null ? new HandoverSynthesis((string)h.SYNTHESISCONTENT) : null,
        (string)h.SHIFTNAME,
        (string)h.CREATEDBY,
        (string)h.ASSIGNEDTO,
        (string?)h.CREATEDBYNAME,
        (string?)h.ASSIGNEDTONAME,
        (string?)h.RECEIVERUSERID,
        (string)h.RESPONSIBLEPHYSICIANID,
        (string)h.RESPONSIBLEPHYSICIANNAME,
        (string?)h.CREATEDAT,
        (string?)h.READYAT,
        (string?)h.STARTEDAT,
        (string?)h.ACKNOWLEDGEDAT,
        (string?)h.ACCEPTEDAT,
        (string?)h.COMPLETEDAT,
        (string?)h.CANCELLEDAT,
        (string?)h.REJECTEDAT,
        (string?)h.REJECTIONREASON,
        (string?)h.EXPIREDAT,
        (string?)h.HANDOVERTYPE,
        (DateTime?)h.HANDOVERWINDOWDATE,
        (string?)h.FROMSHIFTID,
        (string?)h.TOSHIFTID,
        (string?)h.TODOCTORID,
        (string)h.STATENAME,
        (int)h.VERSION
    );
  }
}
