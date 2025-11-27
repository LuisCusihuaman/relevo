using System.Collections.Generic;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class GetUnitsUseCase
{
    private readonly IUnitRepository _repository;

    public GetUnitsUseCase(IUnitRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<UnitRecord> Execute()
    {
        return _repository.GetUnits();
    }
}
