using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class GetUnitsUseCase
{
    private readonly ISetupRepository _repository;

    public GetUnitsUseCase(ISetupRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<UnitRecord>> ExecuteAsync()
    {
        return await Task.FromResult(_repository.GetUnits());
    }
}
