using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class GetHandoverByIdUseCase
{
    private readonly ISetupRepository _repository;

    public GetHandoverByIdUseCase(ISetupRepository repository)
    {
        _repository = repository;
    }

    public HandoverRecord? Execute(string handoverId)
    {
        return _repository.GetHandoverById(handoverId);
    }
}
