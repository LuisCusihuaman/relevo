using System.Collections.Generic;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class GetShiftsUseCase
{
    private readonly IShiftRepository _repository;

    public GetShiftsUseCase(IShiftRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<ShiftRecord> Execute()
    {
        return _repository.GetShifts();
    }
}
