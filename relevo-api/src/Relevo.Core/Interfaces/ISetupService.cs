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

    // Handover Messages
    Task<IReadOnlyList<HandoverMessageRecord>> GetHandoverMessagesAsync(string handoverId);
    Task<HandoverMessageRecord> CreateHandoverMessageAsync(string handoverId, string userId, string userName, string messageText, string messageType);

    // Handover Activity Log
    Task<IReadOnlyList<HandoverActivityItemRecord>> GetHandoverActivityLogAsync(string handoverId);

    // Handover Checklists
    Task<IReadOnlyList<HandoverChecklistItemRecord>> GetHandoverChecklistsAsync(string handoverId);
    Task<bool> UpdateChecklistItemAsync(string handoverId, string itemId, bool isChecked, string userId);

    // Handover Contingency Plans
    Task<IReadOnlyList<HandoverContingencyPlanRecord>> GetHandoverContingencyPlansAsync(string handoverId);
    Task<HandoverContingencyPlanRecord> CreateContingencyPlanAsync(string handoverId, string conditionText, string actionText, string priority, string createdBy);
}
