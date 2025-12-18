using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Me.Assignments;

namespace Relevo.Web.Me;

public class UnassignMyPatient(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<UnassignMyPatientRequest>
{
  public override void Configure()
  {
    Delete("/me/patients/{patientId}");
  }

  public override async Task HandleAsync(UnassignMyPatientRequest req, CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }

    var result = await _mediator.Send(
      new UnassignMyPatientCommand(userId, req.PatientId), 
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

public class UnassignMyPatientRequest
{
  public string PatientId { get; set; } = string.Empty;
}

