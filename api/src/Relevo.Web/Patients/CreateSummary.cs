using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Patients.CreateSummary;
using Relevo.Core.Models;

namespace Relevo.Web.Patients;

public class CreatePatientSummary(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<CreatePatientSummaryRequest, CreatePatientSummaryResponse>
{
  public override void Configure()
  {
    Post("/patients/{patientId}/summary");
  }

  public override async Task HandleAsync(CreatePatientSummaryRequest req, CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }

    var command = new CreatePatientSummaryCommand(req.PatientId, req.SummaryText, userId);
    var result = await _mediator.Send(command, ct);

    if (result.IsSuccess)
    {
        var summary = result.Value;
        Response = new CreatePatientSummaryResponse
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
    else
    {
        // Handle errors
        AddError(result.Errors.FirstOrDefault() ?? "Error creating summary");
        await SendErrorsAsync(cancellation: ct);
    }
  }
}

public class CreatePatientSummaryRequest
{
    public string PatientId { get; set; } = string.Empty;
    public string SummaryText { get; set; } = string.Empty;
}

public class CreatePatientSummaryResponse
{
    public PatientSummaryDto Summary { get; set; } = new();
}

