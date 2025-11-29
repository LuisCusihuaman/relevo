namespace Relevo.Core.Interfaces;

public interface IShiftCheckInService
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
    Task<IReadOnlyList<HandoverParticipantRecord>> GetHandoverParticipantsAsync(string handoverId);
    Task<HandoverSyncStatusRecord?> GetHandoverSyncStatusAsync(string handoverId, string userId);
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
    Task<bool> DeleteContingencyPlanAsync(string handoverId, string contingencyId);

    // Action Items
    Task<IReadOnlyList<HandoverActionItemRecord>> GetHandoverActionItemsAsync(string handoverId);
    Task<string> CreateHandoverActionItemAsync(string handoverId, string description, string priority);
    Task<bool> UpdateHandoverActionItemAsync(string handoverId, string itemId, bool isCompleted);
    Task<bool> DeleteHandoverActionItemAsync(string handoverId, string itemId);

    // Patient Summaries
    Task<PatientSummaryRecord?> GetPatientSummaryAsync(string patientId);
    Task<PatientSummaryRecord> CreatePatientSummaryAsync(string patientId, string physicianId, string summaryText, string createdBy);
    Task<bool> UpdatePatientSummaryAsync(string summaryId, string summaryText, string lastEditedBy);

    // Handover Creation and Management
    Task<HandoverRecord> CreateHandoverAsync(CreateHandoverRequest request);
    Task<IReadOnlyList<HandoverRecord>> GetPendingHandoversForUserAsync(string userId);
    Task<IReadOnlyList<HandoverRecord>> GetHandoversByPatientAsync(string patientId);
    Task<IReadOnlyList<HandoverRecord>> GetShiftTransitionHandoversAsync(string fromDoctorId, string toDoctorId);
    Task<bool> ReadyHandoverAsync(string handoverId, string userId);
    Task<bool> StartHandoverAsync(string handoverId, string userId);
    Task<bool> AcceptHandoverAsync(string handoverId, string userId);
    Task<bool> CompleteHandoverAsync(string handoverId, string userId);
    Task<bool> CancelHandoverAsync(string handoverId, string userId);
    Task<bool> RejectHandoverAsync(string handoverId, string userId, string reason);

    // Optimistic locking overloads (with version parameter)
    Task<bool> ReadyHandoverAsync(string handoverId, string userId, int expectedVersion);
    Task<bool> StartHandoverAsync(string handoverId, string userId, int expectedVersion);
    Task<bool> AcceptHandoverAsync(string handoverId, string userId, int expectedVersion);
    Task<bool> CompleteHandoverAsync(string handoverId, string userId, int expectedVersion);
    Task<bool> CancelHandoverAsync(string handoverId, string userId, int expectedVersion);
    Task<bool> RejectHandoverAsync(string handoverId, string userId, string reason, int expectedVersion);

    // Singleton Sections
    Task<HandoverPatientDataRecord?> GetPatientDataAsync(string handoverId);
    Task<HandoverSituationAwarenessRecord?> GetSituationAwarenessAsync(string handoverId);
    Task<HandoverSynthesisRecord?> GetSynthesisAsync(string handoverId);

    Task<bool> UpdatePatientDataAsync(string handoverId, string illnessSeverity, string? summaryText, string status, string userId);
    Task<bool> UpdateSituationAwarenessAsync(string handoverId, string? content, string status, string userId);
    Task<bool> UpdateSynthesisAsync(string handoverId, string? content, string status, string userId);
}
