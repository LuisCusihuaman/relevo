namespace Relevo.Core.Models;

public record HandoverRecord(
    string Id,
    string AssignmentId,
    string PatientId,
    string? PatientName,
    string Status,
    HandoverIllnessSeverity IllnessSeverity,
    HandoverPatientSummary PatientSummary,
    string? SituationAwarenessDocId,
    HandoverSynthesis? Synthesis,
    string ShiftName,
    string CreatedBy,
    string AssignedTo,
    string? CreatedByName,
    string? AssignedToName,
    string? ReceiverUserId,
    string ResponsiblePhysicianId,
    string ResponsiblePhysicianName,
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
);

public record HandoverIllnessSeverity(string Severity);
public record HandoverPatientSummary(string Content);
public record HandoverSynthesis(string Content);

