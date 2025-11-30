using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Patients.UpdateSummary;

public class UpdatePatientSummaryHandler(IHandoverRepository _handoverRepository, IPatientRepository _patientRepository)
  : ICommandHandler<UpdatePatientSummaryCommand, Result>
{
  public async Task<Result> Handle(UpdatePatientSummaryCommand request, CancellationToken cancellationToken)
  {
    // Get or create current handover for patient
    var handoverId = await _handoverRepository.GetOrCreateCurrentHandoverIdAsync(request.PatientId, request.UserId);

    if (string.IsNullOrEmpty(handoverId))
    {
      return Result.Error("Cannot update summary: patient has no assignment");
    }

    // Update patient summary in HANDOVER_CONTENTS
    var success = await _patientRepository.UpdatePatientSummaryAsync(
        handoverId,
        request.SummaryText,
        request.UserId
    );

    if (!success)
    {
      return Result.Error("Failed to update summary");
    }

    return Result.Success();
  }
}

