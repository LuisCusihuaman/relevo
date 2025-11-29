using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.CreateSummary;

public class CreatePatientSummaryHandler(IPatientRepository _repository)
  : ICommandHandler<CreatePatientSummaryCommand, Result<PatientSummaryRecord>>
{
  public async Task<Result<PatientSummaryRecord>> Handle(CreatePatientSummaryCommand request, CancellationToken cancellationToken)
  {
    // For now, assuming PhysicianId is the same as the UserId creating it.
    var summary = await _repository.CreatePatientSummaryAsync(
        request.PatientId,
        request.UserId,
        request.SummaryText,
        request.UserId
    );

    return Result.Success(summary);
  }
}

