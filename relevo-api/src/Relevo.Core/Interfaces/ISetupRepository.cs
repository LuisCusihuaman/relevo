namespace Relevo.Core.Interfaces;

public interface ISetupRepository
{
    IReadOnlyList<UnitRecord> GetUnits();
    IReadOnlyList<ShiftRecord> GetShifts();
    (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(string unitId, int page, int pageSize);
    (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetAllPatients(int page, int pageSize);
    PatientDetailRecord? GetPatientById(string patientId);
    Task<IReadOnlyList<string>> AssignAsync(string userId, string shiftId, IEnumerable<string> patientIds);
    void EnsureUserExists(string userId, string? email, string? firstName, string? lastName, string? fullName);
    Task CreateHandoverForAssignmentAsync(string assignmentId, string userId, DateTime windowDate, string fromShiftId, string toShiftId);
    (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(string userId, int page, int pageSize);
    (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetMyHandovers(string userId, int page, int pageSize);
    (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetPatientHandovers(string patientId, int page, int pageSize);
    HandoverRecord? GetHandoverById(string handoverId);
    IReadOnlyList<HandoverParticipantRecord> GetHandoverParticipants(string handoverId);
    IReadOnlyList<HandoverSectionRecord> GetHandoverSections(string handoverId);
    HandoverSyncStatusRecord? GetHandoverSyncStatus(string handoverId, string userId);
    bool UpdateHandoverSection(string handoverId, string sectionId, string content, string status, string userId);
    UserPreferencesRecord? GetUserPreferences(string userId);
    IReadOnlyList<UserSessionRecord> GetUserSessions(string userId);
    bool UpdateUserPreferences(string userId, UserPreferencesRecord preferences);

    // Handover Messages
    IReadOnlyList<HandoverMessageRecord> GetHandoverMessages(string handoverId);
    HandoverMessageRecord CreateHandoverMessage(string handoverId, string userId, string userName, string messageText, string messageType);

    // Handover Activity Log
    IReadOnlyList<HandoverActivityItemRecord> GetHandoverActivityLog(string handoverId);

    // Handover Checklists
    IReadOnlyList<HandoverChecklistItemRecord> GetHandoverChecklists(string handoverId);
    bool UpdateChecklistItem(string handoverId, string itemId, bool isChecked, string userId);

    // Handover Contingency Plans
    IReadOnlyList<HandoverContingencyPlanRecord> GetHandoverContingencyPlans(string handoverId);
    HandoverContingencyPlanRecord CreateContingencyPlan(string handoverId, string conditionText, string actionText, string priority, string createdBy);
    bool DeleteContingencyPlan(string handoverId, string contingencyId);

    // Action Items
    IReadOnlyList<HandoverActionItemRecord> GetHandoverActionItems(string handoverId);
    string CreateHandoverActionItem(string handoverId, string description, string priority);
    bool UpdateHandoverActionItem(string handoverId, string itemId, bool isCompleted);
    bool DeleteHandoverActionItem(string handoverId, string itemId);

    // Patient Summaries
    PatientSummaryRecord? GetPatientSummary(string patientId);
    PatientSummaryRecord CreatePatientSummary(string patientId, string physicianId, string summaryText, string createdBy);
    bool UpdatePatientSummary(string summaryId, string summaryText, string lastEditedBy);

    Task<bool> StartHandover(string handoverId, string userId);
    Task<bool> ReadyHandover(string handoverId, string userId);
    Task<bool> AcceptHandover(string handoverId, string userId);
    Task<bool> CompleteHandover(string handoverId, string userId);
    Task<bool> CancelHandover(string handoverId, string userId);
    Task<bool> RejectHandover(string handoverId, string userId, string reason);
}

// Domain Records
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
    IReadOnlyList<HandoverActionItem> ActionItems,
    string? SituationAwarenessDocId,
    HandoverSynthesis? Synthesis,
    string ShiftName,
    string CreatedBy,
    string AssignedTo,
    string? CreatedByName,
    string? AssignedToName,
    string? ReceiverUserId,
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
    string StateName
);
public record HandoverIllnessSeverity(string Severity);
public record HandoverPatientSummary(string Content);
public record HandoverSynthesis(string Content);
public record HandoverActionItem(string Id, string Description, bool IsCompleted);

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

public record HandoverSectionRecord(
    string Id,
    string HandoverId,
    string SectionType,
    string? Content,
    string Status,
    string? LastEditedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
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
