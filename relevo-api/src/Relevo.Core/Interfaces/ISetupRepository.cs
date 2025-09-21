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

public record HandoverParticipantRecord
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserRole { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime LastActivity { get; set; }

    public HandoverParticipantRecord() { }

    public HandoverParticipantRecord(
        string Id,
        string UserId,
        string UserName,
        string? UserRole,
        string Status,
        DateTime JoinedAt,
        DateTime LastActivity)
    {
        this.Id = Id;
        this.UserId = UserId;
        this.UserName = UserName;
        this.UserRole = UserRole;
        this.Status = Status;
        this.JoinedAt = JoinedAt;
        this.LastActivity = LastActivity;
    }
}

public record HandoverSectionRecord
{
    public string Id { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LastEditedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public HandoverSectionRecord() { }

    public HandoverSectionRecord(
        string Id,
        string SectionType,
        string? Content,
        string Status,
        string? LastEditedBy,
        DateTime CreatedAt,
        DateTime UpdatedAt)
    {
        this.Id = Id;
        this.SectionType = SectionType;
        this.Content = Content;
        this.Status = Status;
        this.LastEditedBy = LastEditedBy;
        this.CreatedAt = CreatedAt;
        this.UpdatedAt = UpdatedAt;
    }
}

public record HandoverSyncStatusRecord
{
    public string Id { get; set; } = string.Empty;
    public string SyncStatus { get; set; } = string.Empty;
    public DateTime LastSync { get; set; }
    public int Version { get; set; }

    public HandoverSyncStatusRecord() { }

    public HandoverSyncStatusRecord(
        string Id,
        string SyncStatus,
        DateTime LastSync,
        int Version)
    {
        this.Id = Id;
        this.SyncStatus = SyncStatus;
        this.LastSync = LastSync;
        this.Version = Version;
    }
}

public record UserPreferencesRecord
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public bool NotificationsEnabled { get; set; }
    public bool AutoSaveEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserPreferencesRecord() { }

    public UserPreferencesRecord(
        string Id,
        string UserId,
        string Theme,
        string Language,
        string Timezone,
        bool NotificationsEnabled,
        bool AutoSaveEnabled,
        DateTime CreatedAt,
        DateTime UpdatedAt)
    {
        this.Id = Id;
        this.UserId = UserId;
        this.Theme = Theme;
        this.Language = Language;
        this.Timezone = Timezone;
        this.NotificationsEnabled = NotificationsEnabled;
        this.AutoSaveEnabled = AutoSaveEnabled;
        this.CreatedAt = CreatedAt;
        this.UpdatedAt = UpdatedAt;
    }
}

public record UserSessionRecord
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime SessionStart { get; set; }
    public DateTime? SessionEnd { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsActive { get; set; }

    public UserSessionRecord() { }

    public UserSessionRecord(
        string Id,
        string UserId,
        DateTime SessionStart,
        DateTime? SessionEnd,
        string? IpAddress,
        string? UserAgent,
        bool IsActive)
    {
        this.Id = Id;
        this.UserId = UserId;
        this.SessionStart = SessionStart;
        this.SessionEnd = SessionEnd;
        this.IpAddress = IpAddress;
        this.UserAgent = UserAgent;
        this.IsActive = IsActive;
    }
}
