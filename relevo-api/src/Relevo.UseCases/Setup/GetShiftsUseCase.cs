using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class GetShiftsUseCase
{
    private readonly ISetupRepository _repository;

    public GetShiftsUseCase(ISetupRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ShiftRecord>> ExecuteAsync()
    {
        return await Task.FromResult(_repository.GetShifts());
    }
}
