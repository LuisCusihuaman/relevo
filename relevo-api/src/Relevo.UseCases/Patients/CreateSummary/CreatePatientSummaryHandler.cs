using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.CreateSummary;

public class CreatePatientSummaryHandler(IHandoverRepository _handoverRepository, IPatientRepository _patientRepository)
  : ICommandHandler<CreatePatientSummaryCommand, Result<PatientSummaryRecord>>
{
  public async Task<Result<PatientSummaryRecord>> Handle(CreatePatientSummaryCommand request, CancellationToken cancellationToken)
  {
    // Get or create current handover for patient
    var handoverId = await _handoverRepository.GetOrCreateCurrentHandoverIdAsync(request.PatientId, request.UserId);

    if (string.IsNullOrEmpty(handoverId))
    {
      return Result.Error("Cannot create summary: patient has no assignment");
    }

    // Create/update patient summary in HANDOVER_CONTENTS
    var summary = await _patientRepository.CreatePatientSummaryAsync(
        handoverId,
        request.SummaryText,
        request.UserId
    );

    return Result.Success(summary);
  }
}

