using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class GetPatientHandoversUseCase
{
    private readonly ISetupRepository _repository;

    public GetPatientHandoversUseCase(ISetupRepository repository)
    {
        _repository = repository;
    }

    public (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) Execute(
        string patientId,
        int page,
        int pageSize)
    {
        return _repository.GetPatientHandovers(patientId, page, pageSize);
    }
}
