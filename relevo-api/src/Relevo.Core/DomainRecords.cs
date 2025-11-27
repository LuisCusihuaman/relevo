namespace Relevo.Core.Interfaces;

// Extracted records from ISetupRepository
public record UnitRecord(string Id, string Name);
public record ShiftRecord(string Id, string Name, string StartTime, string EndTime);
public record PatientRecord(
    string Id,
    string Name,
    string HandoverStatus,
    string? HandoverId,
    decimal? Age,
    string Room,
    string Diagnosis,
    string? Status,
    string? Severity
);

public record PatientDetailRecord(
    string Id,
    string Name,
    string Mrn,
    string Dob,
    string Gender,
    string AdmissionDate,
    string CurrentUnit,
    string RoomNumber,
    string Diagnosis,
    IReadOnlyList<string> Allergies,
    IReadOnlyList<string> Medications,
    string Notes
);
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
public record HandoverActionItem(string Id, string Description, bool IsCompleted);

// Singleton Section Records
public record HandoverPatientDataRecord(
    string HandoverId,
    string IllnessSeverity,
    string? SummaryText,
    string? LastEditedBy,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record HandoverSituationAwarenessRecord(
    string HandoverId,
    string? Content,
    string Status,
    string? LastEditedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record HandoverSynthesisRecord(
    string HandoverId,
    string? Content,
    string Status,
    string? LastEditedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record HandoverParticipantRecord(
    string Id,
    string HandoverId,
    string UserId,
    string UserName,
    string? UserRole,
    string Status,
    DateTime JoinedAt,
    DateTime LastActivity
);

public record HandoverSyncStatusRecord(
    string Id,
    string HandoverId,
    string UserId,
    string SyncStatus,
    DateTime LastSync,
    int Version
);

public record UserPreferencesRecord(
    string Id,
    string UserId,
    string Theme,
    string Language,
    string Timezone,
    bool NotificationsEnabled,
    bool AutoSaveEnabled,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record UserSessionRecord(
    string Id,
    string UserId,
    DateTime SessionStart,
    DateTime? SessionEnd,
    string? IpAddress,
    string? UserAgent,
    bool IsActive
);

public record HandoverMessageRecord(
    string Id,
    string HandoverId,
    string UserId,
    string UserName,
    string MessageText,
    string MessageType,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public HandoverMessageRecord() : this("", "", "", "", "", "message", DateTime.MinValue, DateTime.MinValue) { }
}

public record HandoverActivityItemRecord(
    string Id,
    string HandoverId,
    string UserId,
    string UserName,
    string ActivityType,
    string? ActivityDescription,
    string? SectionAffected,
    string? Metadata,
    DateTime CreatedAt
)
{
    public HandoverActivityItemRecord() : this("", "", "", "", "", null, null, null, DateTime.MinValue) { }
}

public record HandoverChecklistItemRecord(
    string Id,
    string HandoverId,
    string UserId,
    string ItemId,
    string ItemCategory,
    string ItemLabel,
    string? ItemDescription,
    bool IsRequired,
    bool IsChecked,
    DateTime? CheckedAt,
    DateTime CreatedAt
)
{
    public HandoverChecklistItemRecord() : this("", "", "", "", "", "", null, false, false, null, DateTime.MinValue) { }
}

public record HandoverContingencyPlanRecord(
    string Id,
    string HandoverId,
    string ConditionText,
    string ActionText,
    string Priority,
    string Status,
    string CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public HandoverContingencyPlanRecord() : this("", "", "", "", "medium", "active", "", DateTime.MinValue, DateTime.MinValue) { }
}

// Action Items
public record HandoverActionItemRecord(
    string Id,
    string HandoverId,
    string Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt
)
{
    public HandoverActionItemRecord() : this("", "", "", false, DateTime.MinValue, DateTime.MinValue, null) { }
}

// Patient Summaries
public record PatientSummaryRecord(
    string Id,
    string PatientId,
    string PhysicianId,
    string SummaryText,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string LastEditedBy
);

// Handover Creation Request
public record CreateHandoverRequest(
    string PatientId,
    string FromDoctorId,
    string ToDoctorId,
    string FromShiftId,
    string ToShiftId,
    string InitiatedBy,
    string? Notes
);
