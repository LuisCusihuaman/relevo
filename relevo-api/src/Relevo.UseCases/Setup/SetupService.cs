using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class SetupService : ISetupService
{
    private readonly ISetupQueryService _queryService;
    private readonly ISetupCommandService _commandService;

    public SetupService(ISetupQueryService queryService, ISetupCommandService commandService)
    {
        _queryService = queryService;
        _commandService = commandService;
    }

    public async Task AssignPatientsAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        await _commandService.AssignPatientsAsync(userId, shiftId, patientIds);
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetMyPatientsAsync(
        string userId,
        int page,
        int pageSize)
    {
        return await _queryService.GetMyPatientsAsync(userId, page, pageSize);
    }

    public async Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetMyHandoversAsync(
        string userId,
        int page,
        int pageSize)
    {
        return await _queryService.GetMyHandoversAsync(userId, page, pageSize);
    }

    public async Task<IReadOnlyList<UnitRecord>> GetUnitsAsync()
    {
        return await _queryService.GetUnitsAsync();
    }

    public async Task<IReadOnlyList<ShiftRecord>> GetShiftsAsync()
    {
        return await _queryService.GetShiftsAsync();
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(
        string unitId,
        int page,
        int pageSize)
    {
        return await _queryService.GetPatientsByUnitAsync(unitId, page, pageSize);
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetAllPatientsAsync(
        int page,
        int pageSize)
    {
        return await _queryService.GetAllPatientsAsync(page, pageSize);
    }

    public async Task<PatientDetailRecord?> GetPatientByIdAsync(string patientId)
    {
        return await _queryService.GetPatientByIdAsync(patientId);
    }

    public async Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetPatientHandoversAsync(
        string patientId,
        int page,
        int pageSize)
    {
        return await _queryService.GetPatientHandoversAsync(patientId, page, pageSize);
    }

    public async Task<HandoverRecord?> GetHandoverByIdAsync(string handoverId)
    {
        return await _queryService.GetHandoverByIdAsync(handoverId);
    }

    public async Task<HandoverRecord?> GetActiveHandoverAsync(string userId)
    {
        return await _queryService.GetActiveHandoverAsync(userId);
    }

    public async Task<IReadOnlyList<HandoverParticipantRecord>> GetHandoverParticipantsAsync(string handoverId)
    {
        return await _queryService.GetHandoverParticipantsAsync(handoverId);
    }

    public async Task<IReadOnlyList<HandoverSectionRecord>> GetHandoverSectionsAsync(string handoverId)
    {
        return await _queryService.GetHandoverSectionsAsync(handoverId);
    }

    public async Task<HandoverSyncStatusRecord?> GetHandoverSyncStatusAsync(string handoverId, string userId)
    {
        return await _queryService.GetHandoverSyncStatusAsync(handoverId, userId);
    }

    public async Task<bool> UpdateHandoverSectionAsync(string handoverId, string sectionId, string content, string status, string userId)
    {
        return await _commandService.UpdateHandoverSectionAsync(handoverId, sectionId, content, status, userId);
    }

    public async Task<UserPreferencesRecord?> GetUserPreferencesAsync(string userId)
    {
        return await _queryService.GetUserPreferencesAsync(userId);
    }

    public async Task<IReadOnlyList<UserSessionRecord>> GetUserSessionsAsync(string userId)
    {
        return await _queryService.GetUserSessionsAsync(userId);
    }

    public async Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferencesRecord preferences)
    {
        return await _commandService.UpdateUserPreferencesAsync(userId, preferences);
    }
}
