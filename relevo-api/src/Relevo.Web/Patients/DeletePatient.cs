using FastEndpoints;
using MediatR;
using Relevo.UseCases.Patients.Delete;

namespace Relevo.Web.Patients;

public class DeletePatient(IMediator _mediator)
  : Endpoint<DeletePatientRequest>
{
  public override void Configure()
  {
    Delete("/patients/{patientId}");
  }

  public override async Task HandleAsync(DeletePatientRequest req, CancellationToken ct)
  {
    var result = await _mediator.Send(new DeletePatientCommand(req.PatientId), ct);

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    if (result.IsSuccess)
    {
      await SendNoContentAsync(ct);
    }
    else
    {
      var errorMessage = result.Errors.FirstOrDefault() ?? "Failed to delete patient";
      AddError(errorMessage);
      await SendErrorsAsync(statusCode: 400, ct);
    }
  }
}

public class DeletePatientRequest
{
  public string PatientId { get; set; } = string.Empty;
}

