using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.UpdateClinicalData;

namespace Relevo.Web.Handovers;

public class PutClinicalData(IMediator _mediator)
  : Endpoint<UpdateClinicalDataRequest>
{
  public override void Configure()
  {
    Put("/handovers/{handoverId}/patient-data");
    AllowAnonymous();
  }

  public override async Task HandleAsync(UpdateClinicalDataRequest req, CancellationToken ct)
  {
    var userId = "dr-1"; // Mock user

    var result = await _mediator.Send(new UpdateHandoverClinicalDataCommand(
        req.HandoverId,
        req.IllnessSeverity,
        req.SummaryText,
        userId
    ), ct);

    if (result.IsSuccess)
    {
        await SendOkAsync(ct);
    }
    else
    {
        await SendNotFoundAsync(ct);
    }
  }
}

public class UpdateClinicalDataRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public string IllnessSeverity { get; set; } = string.Empty;
    public string SummaryText { get; set; } = string.Empty;
}

