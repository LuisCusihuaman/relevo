using FastEndpoints;
using MediatR;
using Relevo.UseCases.Patients.UpdateSummary;
using System.ComponentModel.DataAnnotations;

namespace Relevo.Web.Patients;

public class UpdatePatientSummary(IMediator _mediator)
  : Endpoint<UpdatePatientSummaryRequest, UpdatePatientSummaryResponse>
{
  public override void Configure()
  {
    Put("/patients/{patientId}/summary");
    AllowAnonymous();
  }

  public override async Task HandleAsync(UpdatePatientSummaryRequest req, CancellationToken ct)
  {
    // Mock user ID
    var userId = "dr-1";

    var command = new UpdatePatientSummaryCommand(req.PatientId, req.SummaryText, userId);
    var result = await _mediator.Send(command, ct);

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
        await SendNotFoundAsync(ct);
        return;
    }

    if (result.IsSuccess)
    {
        Response = new UpdatePatientSummaryResponse
        {
            Success = true,
            Message = "Patient summary updated successfully"
        };
        await SendAsync(Response, cancellation: ct);
    }
    else
    {
        AddError(result.Errors.FirstOrDefault() ?? "Error updating summary");
        await SendErrorsAsync(cancellation: ct);
    }
  }
}

public class UpdatePatientSummaryRequest
{
    public string PatientId { get; set; } = string.Empty;
    public string SummaryText { get; set; } = string.Empty;
}

public class UpdatePatientSummaryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

