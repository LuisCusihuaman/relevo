using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.GetSummary;

public class GetPatientSummaryHandler(IPatientRepository _repository)
  : IQueryHandler<GetPatientSummaryQuery, Result<PatientSummaryRecord>>
{
  public async Task<Result<PatientSummaryRecord>> Handle(GetPatientSummaryQuery request, CancellationToken cancellationToken)
  {
    var summary = await _repository.GetPatientSummaryAsync(request.PatientId);

    if (summary == null)
    {
      // It is acceptable to return null for summary if none exists, 
      // but for API consistency, we might want to return empty object or null wrapped in success if the patient exists.
      // The legacy implementation returns null summary inside the response if not found.
      // Let's return NotFound here if we strictly follow the pattern, but if the requirement is to return null summary...
      // Let's return null/NotFound and let the controller decide how to present it.
      // However, checking legacy GetPatientSummary.cs:
      // Response = new GetPatientSummaryResponse { Summary = summary != null ? ... : null };
      // So it returns 200 OK with null summary property.
      // So we should probably return Success with null value if that's supported by Result, or handle it in controller.
      // Result<T> usually implies T is not null for success.
      // Let's return NotFound() here and let the endpoint handle it, OR return a null record?
      // Actually, the repository returning null means no summary exists.
      
      // Option 1: Return NotFound() -> Endpoint sends 404. 
      // But legacy sends 200 OK with null body for summary property.
      // So endpoint should handle NotFound status from here OR we return a "Empty" record?
      // No, let's return Result.NotFound() and let the endpoint map it to the specific response structure if needed, 
      // OR check if patient exists first?
      // Simplest: Return NotFound() here. The endpoint can check status and return 200 with null if that matches legacy behavior.
      
      return Result.NotFound();
    }

    return summary;
  }
}

