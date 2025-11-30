using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.GetSummary;

public class GetPatientSummaryHandler(IHandoverRepository _handoverRepository, IPatientRepository _patientRepository)
  : IQueryHandler<GetPatientSummaryQuery, Result<PatientSummaryRecord>>
{
  public async Task<Result<PatientSummaryRecord>> Handle(GetPatientSummaryQuery request, CancellationToken cancellationToken)
  {
    // Get current handover for patient (read-only, no side effects)
    var handoverId = await _handoverRepository.GetCurrentHandoverIdAsync(request.PatientId);

    if (string.IsNullOrEmpty(handoverId))
    {
      // No active handover exists - return null summary
      return Result.NotFound();
    }

    var summary = await _patientRepository.GetPatientSummaryFromHandoverAsync(handoverId);

    if (summary == null)
    {
      return Result.NotFound();
    }

    return Result.Success(summary);
  }
}

