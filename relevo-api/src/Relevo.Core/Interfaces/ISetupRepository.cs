namespace Relevo.Core.Interfaces;

public interface ISetupRepository
{
    IReadOnlyList<UnitRecord> GetUnits();
    IReadOnlyList<ShiftRecord> GetShifts();
    (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(string unitId, int page, int pageSize);
    (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetAllPatients(int page, int pageSize);
    PatientDetailRecord? GetPatientById(string patientId);
    Task<IReadOnlyList<string>> AssignAsync(string userId, string shiftId, IEnumerable<string> patientIds);
    Task CreateHandoverForAssignmentAsync(string assignmentId, string userId);
    (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(string userId, int page, int pageSize);
    (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetMyHandovers(string userId, int page, int pageSize);
    (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetPatientHandovers(string patientId, int page, int pageSize);
    HandoverRecord? GetHandoverById(string handoverId);
    HandoverRecord? GetActiveHandover(string userId);
    IReadOnlyList<HandoverParticipantRecord> GetHandoverParticipants(string handoverId);
    IReadOnlyList<HandoverSectionRecord> GetHandoverSections(string handoverId);
    HandoverSyncStatusRecord? GetHandoverSyncStatus(string handoverId, string userId);
    bool UpdateHandoverSection(string handoverId, string sectionId, string content, string status, string userId);
    UserPreferencesRecord? GetUserPreferences(string userId);
    IReadOnlyList<UserSessionRecord> GetUserSessions(string userId);
    bool UpdateUserPreferences(string userId, UserPreferencesRecord preferences);
}

// Domain Records
public record UnitRecord(string Id, string Name);
public record ShiftRecord(string Id, string Name, string StartTime, string EndTime);
public record PatientRecord(
    string Id,
    string Name,
    string HandoverStatus,
    string? HandoverId
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
    string? CreatedAt
);
public record HandoverIllnessSeverity(string Severity);
public record HandoverPatientSummary(string Content);
public record HandoverSynthesis(string Content);
public record HandoverActionItem(string Id, string Description, bool IsCompleted);

public record HandoverParticipantRecord(
    string Id,
    string UserId,
    string UserName,
    string? UserRole,
    string Status,
    DateTime JoinedAt,
    DateTime LastActivity
)
{
    public HandoverParticipantRecord() : this("", "", "", null, "", DateTime.MinValue, DateTime.MinValue) { }
}

public record HandoverSectionRecord(
    string Id,
    string SectionType,
    string? Content,
    string Status,
    string? LastEditedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public HandoverSectionRecord() : this("", "", null, "", null, DateTime.MinValue, DateTime.MinValue) { }
}

public record HandoverSyncStatusRecord(
    string Id,
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
