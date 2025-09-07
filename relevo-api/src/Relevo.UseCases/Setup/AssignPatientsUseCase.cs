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
        await _repository.AssignAsync(userId, shiftId, patientIds);
    }
}
