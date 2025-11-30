namespace Relevo.Core.Models;

public record HandoverRecord(
    string Id,
    string AssignmentId,
    string PatientId,
    string? PatientName,
    string Status,
    string? IllnessSeverity,
    string? PatientSummary,
    string? SituationAwarenessDocId,
    string? Synthesis,
    string ShiftName,
    string CreatedBy,
    string? AssignedTo,
    string? CreatedByName,
    string? AssignedToName,
    string? ReceiverUserId,
    string ResponsiblePhysicianId,
    string? ResponsiblePhysicianName,
    string? CreatedAt,
    string? ReadyAt,
    string? StartedAt,
    string? AcknowledgedAt,
    string? AcceptedAt,
    string? CompletedAt,
    string? CancelledAt,
    string? RejectedAt,
    string? RejectionReason,
    string? ExpiredAt,
    string? HandoverType,
    DateTime? HandoverWindowDate,
    string? FromShiftId,
    string? ToShiftId,
    string? ToDoctorId,
    string StateName,
    int Version
)
{
    // Parameterless constructor for Dapper
    public HandoverRecord() : this(
        "", "", "", null, "",
        null, null, null, null,
        "", "", null, null, null, null, "", null,
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, "", 0
    ) { }
}


