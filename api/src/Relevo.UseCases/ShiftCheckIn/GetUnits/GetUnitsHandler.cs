using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.ShiftCheckIn.GetUnits;

public class GetUnitsHandler(IUnitRepository _repository)
  : IQueryHandler<GetUnitsQuery, Result<GetUnitsResult>>
{
  public async Task<Result<GetUnitsResult>> Handle(GetUnitsQuery request, CancellationToken cancellationToken)
  {
    var units = await _repository.GetUnitsAsync();
    return new GetUnitsResult(units);
  }
}

