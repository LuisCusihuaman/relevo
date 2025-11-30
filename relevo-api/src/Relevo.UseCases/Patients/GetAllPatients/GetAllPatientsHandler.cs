using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Patients.GetAllPatients;

public class GetAllPatientsHandler(IPatientRepository _repository)
  : IQueryHandler<GetAllPatientsQuery, Result<GetAllPatientsResult>>
{
  public async Task<Result<GetAllPatientsResult>> Handle(GetAllPatientsQuery request, CancellationToken cancellationToken)
  {
    var (patients, total) = await _repository.GetAllPatientsAsync(request.Page, request.PageSize);
    return new GetAllPatientsResult(patients, total, request.Page, request.PageSize);
  }
}

