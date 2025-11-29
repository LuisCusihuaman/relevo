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
    // Need to fetch complex object structure.
    // For now, basic mapping. Will need to be expanded if HandoverRecord has many nested objects or joins.
    // HandoverRecord is huge, let's map essential fields first and expand.
    // HandoverIllnessSeverity, HandoverPatientSummary, HandoverSynthesis are records wrapping a string.
    // This requires careful Dapper mapping or custom splitting.
    
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

    var result = handovers.Select(h => new HandoverRecord(
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
    )).ToList();

    return (result, total);
  }
}

