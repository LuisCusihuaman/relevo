using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetClinicalData;

public class GetHandoverClinicalDataHandler(IHandoverRepository _repository)
  : IQueryHandler<GetHandoverClinicalDataQuery, Result<HandoverClinicalDataRecord>>
{
  public async Task<Result<HandoverClinicalDataRecord>> Handle(GetHandoverClinicalDataQuery request, CancellationToken cancellationToken)
  {
    var data = await _repository.GetClinicalDataAsync(request.HandoverId);

    if (data == null)
    {
        return Result.NotFound();
    }

    return data;
  }
}

