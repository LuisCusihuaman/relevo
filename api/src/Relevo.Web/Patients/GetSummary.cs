using FastEndpoints;
using MediatR;
using Relevo.UseCases.Patients.GetSummary;
using Relevo.Core.Models;

namespace Relevo.Web.Patients;

public class GetPatientSummary(IMediator _mediator)
  : Endpoint<GetPatientSummaryRequest, GetPatientSummaryResponse>
{
  public override void Configure()
  {
    Get("/patients/{patientId}/summary");
  }

  public override async Task HandleAsync(GetPatientSummaryRequest req, CancellationToken ct)
  {
    var result = await _mediator.Send(new GetPatientSummaryQuery(req.PatientId), ct);

    // Legacy behavior: returns 200 OK with null summary if not found, or we can return 404.
    // The legacy code returns 200 with null.
    // Let's support both: if result is NotFound, we return response with null.
    
    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
        Response = new GetPatientSummaryResponse { Summary = null };
        await SendAsync(Response, cancellation: ct);
        return;
    }

    if (result.IsSuccess)
    {
      var summary = result.Value;
      Response = new GetPatientSummaryResponse
      {
        Summary = new PatientSummaryDto
        {
            Id = summary.Id,
            PatientId = summary.PatientId,
            PhysicianId = summary.PhysicianId,
            SummaryText = summary.SummaryText,
            CreatedAt = summary.CreatedAt,
            UpdatedAt = summary.UpdatedAt,
            LastEditedBy = summary.LastEditedBy
        }
      };
      await SendAsync(Response, cancellation: ct);
    }
  }
}

public class GetPatientSummaryRequest
{
  public string PatientId { get; set; } = string.Empty;
}

public class GetPatientSummaryResponse
{
  public PatientSummaryDto? Summary { get; set; }
}

public class PatientSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string PhysicianId { get; set; } = string.Empty;
    public string SummaryText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string LastEditedBy { get; set; } = string.Empty;
}

