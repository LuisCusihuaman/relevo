using FastEndpoints;
using MediatR;
using Relevo.UseCases.Patients.GetSummary;
using Relevo.Core.Models;
using Relevo.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Relevo.Web.Patients;

public class GetPatientSummary(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<GetPatientSummaryRequest, GetPatientSummaryResponse>
{
  public override void Configure()
  {
    Get("/patients/{patientId}/summary");
  }

  public override async Task HandleAsync(GetPatientSummaryRequest req, CancellationToken ct)
  {
    // PATIENT_SUMMARIES table removed - now uses latest handover's PATIENT_SUMMARY
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId))
    {
      await SendUnauthorizedAsync(ct);
      return;
    }
    var result = await _mediator.Send(new GetPatientSummaryQuery(req.PatientId, userId), ct);

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

