namespace Relevo.Core.Models;

public record HandoverRecord(
    string Id,
    string PatientId,
    string? PatientName,
    string Status,
    string? IllnessSeverity,
    string? PatientSummary,
    string? SituationAwarenessDocId,
    string? Synthesis,
    string? ShiftName,
    string? CreatedBy, // Maps to CREATED_BY_USER_ID
    string? AssignedTo, // Maps to RECEIVER_USER_ID or COMPLETED_BY_USER_ID
    string? CreatedByName,
    string? AssignedToName,
    string? ReceiverUserId, // Maps to RECEIVER_USER_ID
    string? ResponsiblePhysicianId, // Maps to SENDER_USER_ID
    string? ResponsiblePhysicianName,
    string? CreatedAt,
    string? ReadyAt,
    string? StartedAt,
    string? CompletedAt,
    string? CancelledAt,
    DateTime? HandoverWindowDate, // Derived from SHIFT_WINDOWS
    string StateName,
    int Version,
    // V3 Fields
    string? ShiftWindowId,
    string? PreviousHandoverId,
    string? SenderUserId, // SENDER_USER_ID
    string? ReadyByUserId, // READY_BY_USER_ID
    string? StartedByUserId, // STARTED_BY_USER_ID
    string? CompletedByUserId, // COMPLETED_BY_USER_ID
    string? CancelledByUserId, // CANCELLED_BY_USER_ID
    string? CancelReason // CANCEL_REASON
)
{
    // Parameterless constructor for Dapper
    public HandoverRecord() : this("", "", null, "", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "", 0, null, null, null, null, null, null, null, null) { }
}


