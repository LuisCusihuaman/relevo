using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

public interface IAssignmentRepository
{
    Task<IReadOnlyList<string>> AssignPatientsAsync(string userId, string shiftId, IEnumerable<string> patientIds);
    Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetMyPatientsAsync(string userId, int page, int pageSize);
    Task<bool> UnassignPatientAsync(string userId, string shiftInstanceId, string patientId);
    Task<bool> UnassignMyPatientAsync(string userId, string patientId);
}

