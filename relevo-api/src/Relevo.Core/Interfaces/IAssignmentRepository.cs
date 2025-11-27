using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relevo.Core.Interfaces;

public interface IAssignmentRepository
{
    Task<IReadOnlyList<string>> AssignAsync(string userId, string shiftId, IEnumerable<string> patientIds);
    (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(string userId, int page, int pageSize);
    AssignmentRecord? GetAssignment(string userId, string shiftId, string patientId);
    void CreateAssignment(string assignmentId, string userId, string shiftId, string patientId);
}

public record AssignmentRecord(string Id, string UserId, string ShiftId, string PatientId);
