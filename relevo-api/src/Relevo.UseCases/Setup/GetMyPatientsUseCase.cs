using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class GetMyPatientsUseCase
{
    private readonly ISetupRepository _repository;

    public GetMyPatientsUseCase(ISetupRepository repository)
    {
        _repository = repository;
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> ExecuteAsync(
        string userId,
        int page,
        int pageSize)
    {
        return await Task.FromResult(_repository.GetMyPatients(userId, page, pageSize));
    }
}
