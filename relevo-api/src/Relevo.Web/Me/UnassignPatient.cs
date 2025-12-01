using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Me.Assignments;

namespace Relevo.Web.Me;

public class UnassignPatient(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<UnassignPatientRequest>
{
  public override void Configure()
  {
    Delete("/me/assignments/{shiftInstanceId}/patients/{patientId}");
  }

  public override async Task HandleAsync(UnassignPatientRequest req, CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }

    var result = await _mediator.Send(
      new UnassignPatientCommand(userId, req.ShiftInstanceId, req.PatientId), 
      ct);

    if (result.IsSuccess)
    {
      await SendNoContentAsync(ct);
    }
    else
    {
      var errorMessage = result.Errors.FirstOrDefault() ?? "Failed to unassign patient";
      AddError(errorMessage);
      await SendErrorsAsync(statusCode: 400, ct);
    }
  }
}

public class UnassignPatientRequest
{
  public string ShiftInstanceId { get; set; } = string.Empty;
  public string PatientId { get; set; } = string.Empty;
}

