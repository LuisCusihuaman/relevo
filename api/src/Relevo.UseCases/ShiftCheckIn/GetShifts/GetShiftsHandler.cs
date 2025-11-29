using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.ShiftCheckIn.GetShifts;

public class GetShiftsHandler(IShiftRepository _repository)
  : IQueryHandler<GetShiftsQuery, Result<GetShiftsResult>>
{
  public async Task<Result<GetShiftsResult>> Handle(GetShiftsQuery request, CancellationToken cancellationToken)
  {
    var shifts = await _repository.GetShiftsAsync();
    return new GetShiftsResult(shifts);
  }
}

