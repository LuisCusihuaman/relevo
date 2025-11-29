using Relevo.Core.Interfaces;
using Relevo.UseCases.ShiftCheckIn;
using System.Threading.Tasks;

namespace Relevo.UseCases.ShiftCheckIn;

public class ShiftCheckInService : IShiftCheckInService
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
    private readonly IHandoverParticipantsRepository _handoverParticipantsRepository;
    private readonly IHandoverSyncStatusRepository _handoverSyncStatusRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHandoverMessagingRepository _handoverMessagingRepository;
    private readonly IHandoverActivityRepository _handoverActivityRepository;
    private readonly IHandoverChecklistRepository _handoverChecklistRepository;
    private readonly IHandoverContingencyRepository _handoverContingencyRepository;
    private readonly IHandoverActionItemsRepository _handoverActionItemsRepository;
    private readonly IPatientSummaryRepository _patientSummaryRepository;
    private readonly IHandoverRepository _handoverRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IHandoverSectionsRepository _handoverSectionsRepository;

    public ShiftCheckInService(
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
        IHandoverParticipantsRepository handoverParticipantsRepository,
        IHandoverSyncStatusRepository handoverSyncStatusRepository,
        IUserRepository userRepository,
        IHandoverMessagingRepository handoverMessagingRepository,
        IHandoverActivityRepository handoverActivityRepository,
        IHandoverChecklistRepository handoverChecklistRepository,
        IHandoverContingencyRepository handoverContingencyRepository,
        IHandoverActionItemsRepository handoverActionItemsRepository,
        IPatientSummaryRepository patientSummaryRepository,
        IHandoverRepository handoverRepository,
        IAssignmentRepository assignmentRepository,
        IHandoverSectionsRepository handoverSectionsRepository)
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
        _handoverParticipantsRepository = handoverParticipantsRepository;
        _handoverSyncStatusRepository = handoverSyncStatusRepository;
        _userRepository = userRepository;
        _handoverMessagingRepository = handoverMessagingRepository;
        _handoverActivityRepository = handoverActivityRepository;
        _handoverChecklistRepository = handoverChecklistRepository;
        _handoverContingencyRepository = handoverContingencyRepository;
        _handoverActionItemsRepository = handoverActionItemsRepository;
        _patientSummaryRepository = patientSummaryRepository;
        _handoverRepository = handoverRepository;
        _assignmentRepository = assignmentRepository;
        _handoverSectionsRepository = handoverSectionsRepository;
    }

    public async Task AssignPatientsAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        await _assignPatientsUseCase.ExecuteAsync(userId, shiftId, patientIds);
    }

    public Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetMyPatientsAsync(
        string userId,
        int page,
        int pageSize)
    {
        return Task.FromResult(_getMyPatientsUseCase.Execute(userId, page, pageSize));
    }

    public Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetMyHandoversAsync(
        string userId,
        int page,
        int pageSize)
    {
        return Task.FromResult(_getMyHandoversUseCase.Execute(userId, page, pageSize));
    }

    public Task<IReadOnlyList<UnitRecord>> GetUnitsAsync()
    {
        return Task.FromResult(_getUnitsUseCase.Execute());
    }

    public Task<IReadOnlyList<ShiftRecord>> GetShiftsAsync()
    {
        return Task.FromResult(_getShiftsUseCase.Execute());
    }

    public Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(
        string unitId,
        int page,
        int pageSize)
    {
        return Task.FromResult(_getPatientsByUnitUseCase.Execute(unitId, page, pageSize));
    }

    public Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetAllPatientsAsync(
        int page,
        int pageSize)
    {
        return Task.FromResult(_getAllPatientsUseCase.Execute(page, pageSize));
    }

    public Task<PatientDetailRecord?> GetPatientByIdAsync(string patientId)
    {
        return Task.FromResult(_getPatientByIdUseCase.Execute(patientId));
    }

    public Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetPatientHandoversAsync(
        string patientId,
        int page,
        int pageSize)
    {
        var result = _getPatientHandoversUseCase.Execute(patientId, page, pageSize);

        // Auto-ready logic: when patient is selected (receiver selects patient),
        // mark Draft handovers as Ready if they have minimum content and are within window
        // TEMPORARILY DISABLED FOR TESTING
        // ApplyAutoReadyLogic(result.Handovers);

        return Task.FromResult(result);
    }

    private void ApplyAutoReadyLogic(IReadOnlyList<HandoverRecord> handovers)
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
                _handoverRepository.ReadyHandover(handover.Id, "system"); // Using system as the user who triggers auto-ready
            }
        }
    }

    public Task<HandoverRecord?> GetHandoverByIdAsync(string handoverId)
    {
        return Task.FromResult(_getHandoverByIdUseCase.Execute(handoverId));
    }

    public Task<IReadOnlyList<HandoverParticipantRecord>> GetHandoverParticipantsAsync(string handoverId)
    {
        return Task.FromResult(_handoverParticipantsRepository.GetHandoverParticipants(handoverId));
    }

    public Task<HandoverSyncStatusRecord?> GetHandoverSyncStatusAsync(string handoverId, string userId)
    {
        return Task.FromResult(_handoverSyncStatusRepository.GetHandoverSyncStatus(handoverId, userId));
    }

    public Task<UserPreferencesRecord?> GetUserPreferencesAsync(string userId)
    {
        return Task.FromResult(_userRepository.GetUserPreferences(userId));
    }

    public Task<IReadOnlyList<UserSessionRecord>> GetUserSessionsAsync(string userId)
    {
        return Task.FromResult(_userRepository.GetUserSessions(userId));
    }

    public Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferencesRecord preferences)
    {
        return Task.FromResult(_userRepository.UpdateUserPreferences(userId, preferences));
    }

    // Handover Messages
    public Task<IReadOnlyList<HandoverMessageRecord>> GetHandoverMessagesAsync(string handoverId)
    {
        return Task.FromResult(_handoverMessagingRepository.GetHandoverMessages(handoverId));
    }

    public Task<HandoverMessageRecord> CreateHandoverMessageAsync(string handoverId, string userId, string userName, string messageText, string messageType)
    {
        return Task.FromResult(_handoverMessagingRepository.CreateHandoverMessage(handoverId, userId, userName, messageText, messageType));
    }

    // Handover Activity Log
    public Task<IReadOnlyList<HandoverActivityItemRecord>> GetHandoverActivityLogAsync(string handoverId)
    {
        return Task.FromResult(_handoverActivityRepository.GetHandoverActivityLog(handoverId));
    }

    // Handover Checklists
    public Task<IReadOnlyList<HandoverChecklistItemRecord>> GetHandoverChecklistsAsync(string handoverId)
    {
        return Task.FromResult(_handoverChecklistRepository.GetHandoverChecklists(handoverId));
    }

    public Task<bool> UpdateChecklistItemAsync(string handoverId, string itemId, bool isChecked, string userId)
    {
        return Task.FromResult(_handoverChecklistRepository.UpdateChecklistItem(handoverId, itemId, isChecked, userId));
    }

    // Handover Contingency Plans
    public Task<IReadOnlyList<HandoverContingencyPlanRecord>> GetHandoverContingencyPlansAsync(string handoverId)
    {
        return Task.FromResult(_handoverContingencyRepository.GetHandoverContingencyPlans(handoverId));
    }

    public Task<HandoverContingencyPlanRecord> CreateContingencyPlanAsync(string handoverId, string conditionText, string actionText, string priority, string createdBy)
    {
        return Task.FromResult(_handoverContingencyRepository.CreateContingencyPlan(handoverId, conditionText, actionText, priority, createdBy));
    }

    public Task<bool> DeleteContingencyPlanAsync(string handoverId, string contingencyId)
    {
        return Task.FromResult(_handoverContingencyRepository.DeleteContingencyPlan(handoverId, contingencyId));
    }

    // Action Items
    public Task<IReadOnlyList<HandoverActionItemRecord>> GetHandoverActionItemsAsync(string handoverId)
    {
        return Task.FromResult(_handoverActionItemsRepository.GetHandoverActionItems(handoverId));
    }

    public Task<string> CreateHandoverActionItemAsync(string handoverId, string description, string priority)
    {
        return Task.FromResult(_handoverActionItemsRepository.CreateHandoverActionItem(handoverId, description, priority));
    }

    public Task<bool> UpdateHandoverActionItemAsync(string handoverId, string itemId, bool isCompleted)
    {
        return Task.FromResult(_handoverActionItemsRepository.UpdateHandoverActionItem(handoverId, itemId, isCompleted));
    }

    public Task<bool> DeleteHandoverActionItemAsync(string handoverId, string itemId)
    {
        return Task.FromResult(_handoverActionItemsRepository.DeleteHandoverActionItem(handoverId, itemId));
    }

    // Patient Summaries
    public Task<PatientSummaryRecord?> GetPatientSummaryAsync(string patientId)
    {
        return Task.FromResult(_patientSummaryRepository.GetPatientSummary(patientId));
    }

    public Task<PatientSummaryRecord> CreatePatientSummaryAsync(string patientId, string physicianId, string summaryText, string createdBy)
    {
        return Task.FromResult(_patientSummaryRepository.CreatePatientSummary(patientId, physicianId, summaryText, createdBy));
    }

    public Task<bool> UpdatePatientSummaryAsync(string summaryId, string summaryText, string lastEditedBy)
    {
        return Task.FromResult(_patientSummaryRepository.UpdatePatientSummary(summaryId, summaryText, lastEditedBy));
    }

    // Handover Creation and Management
    public async Task<HandoverRecord> CreateHandoverAsync(CreateHandoverRequest request)
    {
        // Get fromDoctor details (implement GetUserById if needed, or skip validation for now)
        // var fromDoctor = _userRepository.GetUserById(request.FromDoctorId);
        // if (fromDoctor == null)
        //     throw new ArgumentException($"FromDoctor {request.FromDoctorId} not found");

        // Get patient
        var patient = _getPatientByIdUseCase.Execute(request.PatientId);
        if (patient == null)
            throw new ArgumentException($"Patient {request.PatientId} not found");

        // Get or create assignment for fromDoctor, patient, fromShift
        var existingAssignment = _assignmentRepository.GetAssignment(request.FromDoctorId, request.FromShiftId, request.PatientId);
        string assignmentId;
        if (existingAssignment == null)
        {
            // Create assignment
            var newAssignmentId = Guid.NewGuid().ToString("N")[..8]; // Simple ID generation
            _assignmentRepository.CreateAssignment(newAssignmentId, request.FromDoctorId, request.FromShiftId, request.PatientId);
            assignmentId = newAssignmentId;
        }
        else
        {
            assignmentId = existingAssignment.Id;
        }

        // Create handover using the repository (note: CreateHandoverForAssignmentAsync expects userName; use dummy for now)
        await _handoverRepository.CreateHandoverForAssignmentAsync(
            assignmentId, 
            request.InitiatedBy, 
            "Test Doctor", // Dummy full name; implement proper lookup if needed
            DateTime.UtcNow.Date, // Handover window is current date
            request.FromShiftId, 
            request.ToShiftId
        );

        // If notes, update patient summary
        if (!string.IsNullOrEmpty(request.Notes))
        {
            var summary = _patientSummaryRepository.GetPatientSummary(request.PatientId) ?? 
                _patientSummaryRepository.CreatePatientSummary(request.PatientId, request.FromDoctorId, request.Notes, request.InitiatedBy);
            _patientSummaryRepository.UpdatePatientSummary(summary.Id, request.Notes, request.InitiatedBy);
        }

        // Return the created handover (query by patientId or adjust; for now, get recent by patient)
        var recentHandovers = _getPatientHandoversUseCase.Execute(request.PatientId, 1, 1);
        return recentHandovers.Handovers.FirstOrDefault() ?? throw new InvalidOperationException("Failed to create handover");
    }

    public async Task<bool> AcceptHandoverAsync(string handoverId, string userId)
    {
        return await _handoverRepository.AcceptHandover(handoverId, userId);
    }

    public async Task<bool> CompleteHandoverAsync(string handoverId, string userId)
    {
        return await _handoverRepository.CompleteHandover(handoverId, userId);
    }

    // Optimistic locking overloads (removed - using simple versions)
    public async Task<bool> ReadyHandoverAsync(string handoverId, string userId, int expectedVersion)
    {
        return await _handoverRepository.ReadyHandover(handoverId, userId);
    }

    public async Task<bool> StartHandoverAsync(string handoverId, string userId, int expectedVersion)
    {
        return await _handoverRepository.StartHandover(handoverId, userId);
    }

    public async Task<bool> AcceptHandoverAsync(string handoverId, string userId, int expectedVersion)
    {
        return await _handoverRepository.AcceptHandover(handoverId, userId);
    }

    public async Task<bool> CompleteHandoverAsync(string handoverId, string userId, int expectedVersion)
    {
        return await _handoverRepository.CompleteHandover(handoverId, userId);
    }

    public async Task<bool> CancelHandoverAsync(string handoverId, string userId, int expectedVersion)
    {
        return await _handoverRepository.CancelHandover(handoverId, userId);
    }

    public async Task<bool> RejectHandoverAsync(string handoverId, string userId, string reason, int expectedVersion)
    {
        return await _handoverRepository.RejectHandover(handoverId, userId, reason);
    }

    public async Task<IReadOnlyList<HandoverRecord>> GetPendingHandoversForUserAsync(string userId)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("GetPendingHandoversForUserAsync should be implemented with proper use cases");
    }

    public async Task<IReadOnlyList<HandoverRecord>> GetHandoversByPatientAsync(string patientId)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("GetHandoversByPatientAsync should be implemented with proper use cases");
    }

    public async Task<IReadOnlyList<HandoverRecord>> GetShiftTransitionHandoversAsync(string fromDoctorId, string toDoctorId)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("GetShiftTransitionHandoversAsync should be implemented with proper use cases");
    }

    public async Task<bool> StartHandoverAsync(string handoverId, string userId)
    {
        return await _handoverRepository.StartHandover(handoverId, userId);
    }

    public async Task<bool> ReadyHandoverAsync(string handoverId, string userId)
    {
        return await _handoverRepository.ReadyHandover(handoverId, userId);
    }

    public async Task<bool> CancelHandoverAsync(string handoverId, string userId)
    {
        return await _handoverRepository.CancelHandover(handoverId, userId);
    }

    public async Task<bool> RejectHandoverAsync(string handoverId, string userId, string reason)
    {
        return await _handoverRepository.RejectHandover(handoverId, userId, reason);
    }

    // Singleton Sections
    public async Task<HandoverPatientDataRecord?> GetPatientDataAsync(string handoverId)
    {
        return await _handoverSectionsRepository.GetPatientDataAsync(handoverId);
    }

    public async Task<HandoverSituationAwarenessRecord?> GetSituationAwarenessAsync(string handoverId)
    {
        return await _handoverSectionsRepository.GetSituationAwarenessAsync(handoverId);
    }

    public async Task<HandoverSynthesisRecord?> GetSynthesisAsync(string handoverId)
    {
        return await _handoverSectionsRepository.GetSynthesisAsync(handoverId);
    }

    public async Task<bool> UpdatePatientDataAsync(string handoverId, string illnessSeverity, string? summaryText, string status, string userId)
    {
        return await _handoverSectionsRepository.UpdatePatientDataAsync(handoverId, illnessSeverity, summaryText, status, userId);
    }

    public async Task<bool> UpdateSituationAwarenessAsync(string handoverId, string? content, string status, string userId)
    {
        return await _handoverSectionsRepository.UpdateSituationAwarenessAsync(handoverId, content, status, userId);
    }

    public async Task<bool> UpdateSynthesisAsync(string handoverId, string? content, string status, string userId)
    {
        return await _handoverSectionsRepository.UpdateSynthesisAsync(handoverId, content, status, userId);
    }
}
