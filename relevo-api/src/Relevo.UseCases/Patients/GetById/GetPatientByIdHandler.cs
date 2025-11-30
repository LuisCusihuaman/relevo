using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.GetById;

public class GetPatientByIdHandler(IPatientRepository _repository)
  : IQueryHandler<GetPatientByIdQuery, Result<PatientDetailRecord>>
{
  public async Task<Result<PatientDetailRecord>> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
  {
    var patient = await _repository.GetPatientByIdAsync(request.PatientId);

    if (patient == null)
    {
      return Result.NotFound();
    }

    return patient;
  }
}

