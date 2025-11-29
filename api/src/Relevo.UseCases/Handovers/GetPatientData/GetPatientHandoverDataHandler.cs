using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetPatientData;

public class GetPatientHandoverDataHandler(IHandoverRepository _repository)
  : IQueryHandler<GetPatientHandoverDataQuery, Result<PatientHandoverDataRecord>>
{
  public async Task<Result<PatientHandoverDataRecord>> Handle(GetPatientHandoverDataQuery request, CancellationToken cancellationToken)
  {
    var data = await _repository.GetPatientHandoverDataAsync(request.HandoverId);

    if (data == null)
    {
      return Result.NotFound();
    }

    return data;
  }
}

