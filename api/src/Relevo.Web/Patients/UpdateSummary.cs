using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Patients.UpdateSummary;

namespace Relevo.Web.Patients;

public class UpdatePatientSummary(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<UpdatePatientSummaryRequest, UpdatePatientSummaryResponse>
{
  public override void Configure()
  {
    Put("/patients/{patientId}/summary");
  }

  public override async Task HandleAsync(UpdatePatientSummaryRequest req, CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }

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

