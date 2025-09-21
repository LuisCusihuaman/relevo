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
        GetPatientByIdUseCase getPatientByIdUseCase)
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
        return await Task.FromResult(_getPatientHandoversUseCase.Execute(patientId, page, pageSize));
    }

    public async Task<HandoverRecord?> GetHandoverByIdAsync(string handoverId)
    {
        return await Task.FromResult(_getHandoverByIdUseCase.Execute(handoverId));
    }

    public async Task<HandoverRecord?> GetActiveHandoverAsync(string userId)
    {
        // This would need a new use case - for now return null
        // TODO: Implement GetActiveHandoverUseCase
        return await Task.FromResult<HandoverRecord?>(null);
    }

    public async Task<IReadOnlyList<HandoverParticipantRecord>> GetHandoverParticipantsAsync(string handoverId)
    {
        // This would need a new use case - for now return empty list
        // TODO: Implement GetHandoverParticipantsUseCase
        return await Task.FromResult(Array.Empty<HandoverParticipantRecord>());
    }

    public async Task<IReadOnlyList<HandoverSectionRecord>> GetHandoverSectionsAsync(string handoverId)
    {
        // This would need a new use case - for now return empty list
        // TODO: Implement GetHandoverSectionsUseCase
        return await Task.FromResult(Array.Empty<HandoverSectionRecord>());
    }

    public async Task<HandoverSyncStatusRecord?> GetHandoverSyncStatusAsync(string handoverId, string userId)
    {
        // This would need a new use case - for now return null
        // TODO: Implement GetHandoverSyncStatusUseCase
        return await Task.FromResult<HandoverSyncStatusRecord?>(null);
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
}
