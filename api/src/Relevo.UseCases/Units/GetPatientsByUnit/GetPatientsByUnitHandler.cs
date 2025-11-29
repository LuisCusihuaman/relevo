using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Units.GetPatientsByUnit;

public class GetPatientsByUnitHandler(IPatientRepository _repository)
  : IQueryHandler<GetPatientsByUnitQuery, Result<GetPatientsByUnitResult>>
{
  public async Task<Result<GetPatientsByUnitResult>> Handle(GetPatientsByUnitQuery request, CancellationToken cancellationToken)
  {
    var (patients, total) = await _repository.GetPatientsByUnitAsync(request.UnitId, request.Page, request.PageSize);

    return new GetPatientsByUnitResult(patients, total, request.Page, request.PageSize);
  }
}

