using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Patients.UpdateSummary;

public class UpdatePatientSummaryHandler(IHandoverRepository _handoverRepository, IPatientRepository _patientRepository)
  : ICommandHandler<UpdatePatientSummaryCommand, Result>
{
  public async Task<Result> Handle(UpdatePatientSummaryCommand request, CancellationToken cancellationToken)
  {
    // V3: Get current handover (no side effects - GET should not create)
    var handoverId = await _handoverRepository.GetCurrentHandoverIdAsync(request.PatientId);

    if (string.IsNullOrEmpty(handoverId))
    {
      return Result.Error("Cannot update summary: patient has no active handover. Create a handover first.");
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

