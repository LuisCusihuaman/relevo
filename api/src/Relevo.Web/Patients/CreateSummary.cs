using FastEndpoints;
using MediatR;
using Relevo.UseCases.Patients.CreateSummary;
using Relevo.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Relevo.Web.Patients;

public class CreatePatientSummary(IMediator _mediator)
  : Endpoint<CreatePatientSummaryRequest, CreatePatientSummaryResponse>
{
  public override void Configure()
  {
    Post("/patients/{patientId}/summary");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CreatePatientSummaryRequest req, CancellationToken ct)
  {
    // Mock user ID until auth is implemented, or use a header/body field if testing
    var userId = "dr-1"; // Mock user

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

