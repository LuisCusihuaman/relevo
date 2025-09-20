using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class GetAllPatientsUseCase
{
    private readonly ISetupRepository _repository;

    public GetAllPatientsUseCase(ISetupRepository repository)
    {
        _repository = repository;
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> ExecuteAsync(
        int page,
        int pageSize)
    {
        return await Task.FromResult(_repository.GetAllPatients(page, pageSize));
    }
}
