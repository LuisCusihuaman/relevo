using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class SetupService : ISetupService
{
    private readonly AssignPatientsUseCase _assignPatientsUseCase;
    private readonly GetMyPatientsUseCase _getMyPatientsUseCase;
    private readonly GetMyHandoversUseCase _getMyHandoversUseCase;
    private readonly GetUnitsUseCase _getUnitsUseCase;
    private readonly GetShiftsUseCase _getShiftsUseCase;
    private readonly GetPatientsByUnitUseCase _getPatientsByUnitUseCase;
    private readonly GetAllPatientsUseCase _getAllPatientsUseCase;
    private readonly GetPatientHandoversUseCase _getPatientHandoversUseCase;
    private readonly GetHandoverByIdUseCase _getHandoverByIdUseCase;
    private readonly GetPatientByIdUseCase _getPatientByIdUseCase;
    private readonly ISetupRepository _repository;

    public SetupService(
        AssignPatientsUseCase assignPatientsUseCase,
        GetMyPatientsUseCase getMyPatientsUseCase,
        GetMyHandoversUseCase getMyHandoversUseCase,
        GetUnitsUseCase getUnitsUseCase,
        GetShiftsUseCase getShiftsUseCase,
        GetPatientsByUnitUseCase getPatientsByUnitUseCase,
        GetAllPatientsUseCase getAllPatientsUseCase,
        GetPatientHandoversUseCase getPatientHandoversUseCase,
        GetHandoverByIdUseCase getHandoverByIdUseCase,
        GetPatientByIdUseCase getPatientByIdUseCase,
        ISetupRepository repository)
    {
        _assignPatientsUseCase = assignPatientsUseCase;
        _getMyPatientsUseCase = getMyPatientsUseCase;
        _getMyHandoversUseCase = getMyHandoversUseCase;
        _getUnitsUseCase = getUnitsUseCase;
        _getShiftsUseCase = getShiftsUseCase;
        _getPatientsByUnitUseCase = getPatientsByUnitUseCase;
        _getAllPatientsUseCase = getAllPatientsUseCase;
        _getPatientHandoversUseCase = getPatientHandoversUseCase;
        _getHandoverByIdUseCase = getHandoverByIdUseCase;
        _getPatientByIdUseCase = getPatientByIdUseCase;
        _repository = repository;
    }

    public async Task AssignPatientsAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        await _assignPatientsUseCase.ExecuteAsync(userId, shiftId, patientIds);
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetMyPatientsAsync(
        string userId,
        int page,
        int pageSize)
    {
        return await _getMyPatientsUseCase.ExecuteAsync(userId, page, pageSize);
    }

    public async Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetMyHandoversAsync(
        string userId,
        int page,
        int pageSize)
    {
        return await _getMyHandoversUseCase.ExecuteAsync(userId, page, pageSize);
    }

    public async Task<IReadOnlyList<UnitRecord>> GetUnitsAsync()
    {
        return await _getUnitsUseCase.ExecuteAsync();
    }

    public async Task<IReadOnlyList<ShiftRecord>> GetShiftsAsync()
    {
        return await _getShiftsUseCase.ExecuteAsync();
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(
        string unitId,
        int page,
        int pageSize)
    {
        return await _getPatientsByUnitUseCase.ExecuteAsync(unitId, page, pageSize);
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetAllPatientsAsync(
        int page,
        int pageSize)
    {
        return await _getAllPatientsUseCase.ExecuteAsync(page, pageSize);
    }

    public async Task<PatientDetailRecord?> GetPatientByIdAsync(string patientId)
    {
        return await Task.FromResult(_getPatientByIdUseCase.Execute(patientId));
    }

    public async Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetPatientHandoversAsync(
        string patientId,
        int page,
        int pageSize)
    {
        var result = await Task.FromResult(_getPatientHandoversUseCase.Execute(patientId, page, pageSize));

        // Auto-ready logic: when patient is selected (receiver selects patient),
        // mark Draft handovers as Ready if they have minimum content and are within window
        await ApplyAutoReadyLogicAsync(result.Handovers);

        return result;
    }

    private async Task ApplyAutoReadyLogicAsync(IReadOnlyList<HandoverRecord> handovers)
    {
        var draftHandovers = handovers.Where(h => h.StateName == "Draft").ToList();

        foreach (var handover in draftHandovers)
        {
            // Check if handover has minimum content
            bool hasMinimumContent = !string.IsNullOrWhiteSpace(handover.PatientSummary.Content) ||
                                   !string.IsNullOrWhiteSpace(handover.SituationAwarenessDocId);

            // Check if handover is within window (current date matches handover window date)
            bool isWithinWindow = handover.HandoverWindowDate?.Date == DateTime.UtcNow.Date;

            if (hasMinimumContent && isWithinWindow && !string.IsNullOrWhiteSpace(handover.Id))
            {
                // Auto-ready the handover
                await _repository.ReadyHandover(handover.Id, "system"); // Using system as the user who triggers auto-ready
            }
        }
    }

    public async Task<HandoverRecord?> GetHandoverByIdAsync(string handoverId)
    {
        return await Task.FromResult(_getHandoverByIdUseCase.Execute(handoverId));
    }

    public async Task<IReadOnlyList<HandoverParticipantRecord>> GetHandoverParticipantsAsync(string handoverId)
    {
        // Call repository directly - no use case needed for simple retrieval
        return await Task.FromResult(_repository.GetHandoverParticipants(handoverId));
    }

    public async Task<IReadOnlyList<HandoverSectionRecord>> GetHandoverSectionsAsync(string handoverId)
    {
        // Call repository directly - no use case needed for simple retrieval
        return await Task.FromResult(_repository.GetHandoverSections(handoverId));
    }

    public async Task<HandoverSyncStatusRecord?> GetHandoverSyncStatusAsync(string handoverId, string userId)
    {
        // Call repository directly - no use case needed for simple retrieval
        return await Task.FromResult(_repository.GetHandoverSyncStatus(handoverId, userId));
    }

    public async Task<bool> UpdateHandoverSectionAsync(string handoverId, string sectionId, string content, string status, string userId)
    {
        // This would need a new use case - for now return false
        // TODO: Implement UpdateHandoverSectionUseCase
        return await Task.FromResult(false);
    }

    public async Task<UserPreferencesRecord?> GetUserPreferencesAsync(string userId)
    {
        // This would need a new use case - for now return null
        // TODO: Implement GetUserPreferencesUseCase
        return await Task.FromResult<UserPreferencesRecord?>(null);
    }

    public async Task<IReadOnlyList<UserSessionRecord>> GetUserSessionsAsync(string userId)
    {
        // This would need a new use case - for now return empty list
        // TODO: Implement GetUserSessionsUseCase
        return await Task.FromResult(Array.Empty<UserSessionRecord>());
    }

    public async Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferencesRecord preferences)
    {
        // This would need a new use case - for now return false
        // TODO: Implement UpdateUserPreferencesUseCase
        return await Task.FromResult(false);
    }

    // Handover Messages
    public async Task<IReadOnlyList<HandoverMessageRecord>> GetHandoverMessagesAsync(string handoverId)
    {
        return await Task.FromResult(_repository.GetHandoverMessages(handoverId));
    }

    public async Task<HandoverMessageRecord> CreateHandoverMessageAsync(string handoverId, string userId, string userName, string messageText, string messageType)
    {
        return await Task.FromResult(_repository.CreateHandoverMessage(handoverId, userId, userName, messageText, messageType));
    }

    // Handover Activity Log
    public async Task<IReadOnlyList<HandoverActivityItemRecord>> GetHandoverActivityLogAsync(string handoverId)
    {
        return await Task.FromResult(_repository.GetHandoverActivityLog(handoverId));
    }

    // Handover Checklists
    public async Task<IReadOnlyList<HandoverChecklistItemRecord>> GetHandoverChecklistsAsync(string handoverId)
    {
        return await Task.FromResult(_repository.GetHandoverChecklists(handoverId));
    }

    public async Task<bool> UpdateChecklistItemAsync(string handoverId, string itemId, bool isChecked, string userId)
    {
        return await Task.FromResult(_repository.UpdateChecklistItem(handoverId, itemId, isChecked, userId));
    }

    // Handover Contingency Plans
    public async Task<IReadOnlyList<HandoverContingencyPlanRecord>> GetHandoverContingencyPlansAsync(string handoverId)
    {
        return await Task.FromResult(_repository.GetHandoverContingencyPlans(handoverId));
    }

    public async Task<HandoverContingencyPlanRecord> CreateContingencyPlanAsync(string handoverId, string conditionText, string actionText, string priority, string createdBy)
    {
        return await Task.FromResult(_repository.CreateContingencyPlan(handoverId, conditionText, actionText, priority, createdBy));
    }

    // Action Items
    public async Task<IReadOnlyList<HandoverActionItemRecord>> GetHandoverActionItemsAsync(string handoverId)
    {
        return await Task.FromResult(_repository.GetHandoverActionItems(handoverId));
    }

    public async Task<string> CreateHandoverActionItemAsync(string handoverId, string description, string priority)
    {
        return await Task.FromResult(_repository.CreateHandoverActionItem(handoverId, description, priority));
    }

    public async Task<bool> UpdateHandoverActionItemAsync(string handoverId, string itemId, bool isCompleted)
    {
        return await Task.FromResult(_repository.UpdateHandoverActionItem(handoverId, itemId, isCompleted));
    }

    public async Task<bool> DeleteHandoverActionItemAsync(string handoverId, string itemId)
    {
        return await Task.FromResult(_repository.DeleteHandoverActionItem(handoverId, itemId));
    }

    // Handover Creation and Management
    public async Task<HandoverRecord> CreateHandoverAsync(CreateHandoverRequest request)
    {
        // For now, we'll need to create use cases for these operations
        // This is a temporary implementation that should be replaced with proper use cases
        await Task.CompletedTask; // Make the compiler happy
        throw new NotImplementedException("CreateHandoverAsync should be implemented with proper use cases");
    }

    public async Task<bool> AcceptHandoverAsync(string handoverId, string userId)
    {
        return await _repository.AcceptHandover(handoverId, userId);
    }

    public async Task<bool> CompleteHandoverAsync(string handoverId, string userId)
    {
        return await _repository.CompleteHandover(handoverId, userId);
    }


    public async Task<IReadOnlyList<HandoverRecord>> GetPendingHandoversForUserAsync(string userId)
    {
        // Temporary implementation - should use use case
        await Task.CompletedTask; // Make the compiler happy
        throw new NotImplementedException("GetPendingHandoversForUserAsync should be implemented with proper use cases");
    }

    public async Task<IReadOnlyList<HandoverRecord>> GetHandoversByPatientAsync(string patientId)
    {
        // Temporary implementation - should use use case
        await Task.CompletedTask; // Make the compiler happy
        throw new NotImplementedException("GetHandoversByPatientAsync should be implemented with proper use cases");
    }

    public async Task<IReadOnlyList<HandoverRecord>> GetShiftTransitionHandoversAsync(string fromDoctorId, string toDoctorId)
    {
        // Temporary implementation - should use use case
        await Task.CompletedTask; // Make the compiler happy
        throw new NotImplementedException("GetShiftTransitionHandoversAsync should be implemented with proper use cases");
    }

    public async Task<bool> StartHandoverAsync(string handoverId, string userId)
    {
        // Call repository directly for this state transition
        return await _repository.StartHandover(handoverId, userId);
    }

    public async Task<bool> ReadyHandoverAsync(string handoverId, string userId)
    {
        return await _repository.ReadyHandover(handoverId, userId);
    }

    public async Task<bool> CancelHandoverAsync(string handoverId, string userId)
    {
        return await _repository.CancelHandover(handoverId, userId);
    }

    public async Task<bool> RejectHandoverAsync(string handoverId, string userId, string reason)
    {
        return await _repository.RejectHandover(handoverId, userId, reason);
    }
}
