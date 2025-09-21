namespace Relevo.Core.Interfaces;

public interface ISetupService
{
    Task AssignPatientsAsync(string userId, string shiftId, IEnumerable<string> patientIds);
    Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetMyPatientsAsync(string userId, int page, int pageSize);
    Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetMyHandoversAsync(string userId, int page, int pageSize);
    Task<IReadOnlyList<UnitRecord>> GetUnitsAsync();
    Task<IReadOnlyList<ShiftRecord>> GetShiftsAsync();
    Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(string unitId, int page, int pageSize);
    Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetAllPatientsAsync(int page, int pageSize);
    Task<PatientDetailRecord?> GetPatientByIdAsync(string patientId);
    Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetPatientHandoversAsync(string patientId, int page, int pageSize);
    Task<HandoverRecord?> GetHandoverByIdAsync(string handoverId);
    Task<HandoverRecord?> GetActiveHandoverAsync(string userId);
    Task<IReadOnlyList<HandoverParticipantRecord>> GetHandoverParticipantsAsync(string handoverId);
    Task<IReadOnlyList<HandoverSectionRecord>> GetHandoverSectionsAsync(string handoverId);
    Task<HandoverSyncStatusRecord?> GetHandoverSyncStatusAsync(string handoverId, string userId);
    Task<bool> UpdateHandoverSectionAsync(string handoverId, string sectionId, string content, string status, string userId);
    Task<UserPreferencesRecord?> GetUserPreferencesAsync(string userId);
    Task<IReadOnlyList<UserSessionRecord>> GetUserSessionsAsync(string userId);
    Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferencesRecord preferences);
}

// Domain Query Services (following Contributors pattern)
public interface ISetupQueryService
{
    Task<IReadOnlyList<UnitRecord>> GetUnitsAsync();
    Task<IReadOnlyList<ShiftRecord>> GetShiftsAsync();
    Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(string unitId, int page, int pageSize);
    Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetAllPatientsAsync(int page, int pageSize);
    Task<PatientDetailRecord?> GetPatientByIdAsync(string patientId);
    Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetMyHandoversAsync(string userId, int page, int pageSize);
    Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetMyPatientsAsync(string userId, int page, int pageSize);
    Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetPatientHandoversAsync(string patientId, int page, int pageSize);
    Task<HandoverRecord?> GetHandoverByIdAsync(string handoverId);
    Task<HandoverRecord?> GetActiveHandoverAsync(string userId);
    Task<IReadOnlyList<HandoverParticipantRecord>> GetHandoverParticipantsAsync(string handoverId);
    Task<IReadOnlyList<HandoverSectionRecord>> GetHandoverSectionsAsync(string handoverId);
    Task<HandoverSyncStatusRecord?> GetHandoverSyncStatusAsync(string handoverId, string userId);
    Task<UserPreferencesRecord?> GetUserPreferencesAsync(string userId);
    Task<IReadOnlyList<UserSessionRecord>> GetUserSessionsAsync(string userId);
}

// Domain Command Services
public interface ISetupCommandService
{
    Task<IReadOnlyList<string>> AssignPatientsAsync(string userId, string shiftId, IEnumerable<string> patientIds);
    Task CreateHandoverForAssignmentAsync(string assignmentId, string userId);
    Task<bool> UpdateHandoverSectionAsync(string handoverId, string sectionId, string content, string status, string userId);
    Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferencesRecord preferences);
}
