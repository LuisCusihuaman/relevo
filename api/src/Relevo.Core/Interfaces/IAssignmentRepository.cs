namespace Relevo.Core.Interfaces;

public interface IAssignmentRepository
{
    Task<IReadOnlyList<string>> AssignPatientsAsync(string userId, string shiftId, IEnumerable<string> patientIds);
}

