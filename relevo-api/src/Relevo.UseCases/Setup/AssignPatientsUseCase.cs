using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class AssignPatientsUseCase
{
    private readonly ISetupRepository _repository;

    public AssignPatientsUseCase(ISetupRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        // First, assign patients to the user for the shift
        var assignmentIds = await _repository.AssignAsync(userId, shiftId, patientIds);

        // Then, create handovers for each assignment
        foreach (var assignmentId in assignmentIds)
        {
            await _repository.CreateHandoverForAssignmentAsync(assignmentId, userId);
        }
    }
}
