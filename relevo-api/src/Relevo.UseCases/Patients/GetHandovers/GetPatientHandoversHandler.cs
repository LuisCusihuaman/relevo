using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Patients.GetHandovers;

public class GetPatientHandoversHandler(IHandoverRepository _repository)
  : IQueryHandler<GetPatientHandoversQuery, Result<GetPatientHandoversResult>>
{
  public async Task<Result<GetPatientHandoversResult>> Handle(GetPatientHandoversQuery request, CancellationToken cancellationToken)
  {
    var (handovers, total) = await _repository.GetPatientHandoversAsync(request.PatientId, request.Page, request.PageSize);
    return new GetPatientHandoversResult(handovers, total, request.Page, request.PageSize);
  }
}

