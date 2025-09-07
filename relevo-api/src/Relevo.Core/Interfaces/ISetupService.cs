namespace Relevo.Core.Interfaces;

public interface ISetupService
{
    Task AssignPatientsAsync(string userId, string shiftId, IEnumerable<string> patientIds);
    Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetMyPatientsAsync(string userId, int page, int pageSize);
    Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetMyHandoversAsync(string userId, int page, int pageSize);
    Task<IReadOnlyList<UnitRecord>> GetUnitsAsync();
    Task<IReadOnlyList<ShiftRecord>> GetShiftsAsync();
    Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(string unitId, int page, int pageSize);
}
