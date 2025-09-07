using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class GetPatientsByUnitUseCase
{
    private readonly ISetupRepository _repository;

    public GetPatientsByUnitUseCase(ISetupRepository repository)
    {
        _repository = repository;
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> ExecuteAsync(
        string unitId,
        int page,
        int pageSize)
    {
        return await Task.FromResult(_repository.GetPatientsByUnit(unitId, page, pageSize));
    }
}
