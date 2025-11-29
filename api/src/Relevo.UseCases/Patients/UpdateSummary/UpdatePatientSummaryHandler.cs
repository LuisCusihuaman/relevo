using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Patients.UpdateSummary;

public class UpdatePatientSummaryHandler(IPatientRepository _repository)
  : ICommandHandler<UpdatePatientSummaryCommand, Result>
{
  public async Task<Result> Handle(UpdatePatientSummaryCommand request, CancellationToken cancellationToken)
  {
    // First, we need to find the existing summary for the patient to get the ID
    // The command uses PatientId, but the repository Update method might expect SummaryId or we find it first.
    // The legacy code finds it first.
    var existingSummary = await _repository.GetPatientSummaryAsync(request.PatientId);

    if (existingSummary == null)
    {
      return Result.NotFound();
    }

    var success = await _repository.UpdatePatientSummaryAsync(
        existingSummary.Id,
        request.SummaryText,
        request.UserId
    );

    if (!success)
    {
      // This could happen if concurrent updates deleted it or something else went wrong
      return Result.Error("Failed to update summary");
    }

    return Result.Success();
  }
}

