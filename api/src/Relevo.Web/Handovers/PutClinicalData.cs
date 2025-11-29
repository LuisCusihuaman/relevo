using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.UpdateClinicalData;

namespace Relevo.Web.Handovers;

public class PutClinicalData(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<UpdateClinicalDataRequest>
{
  public override void Configure()
  {
    Put("/handovers/{handoverId}/patient-data");
  }

  public override async Task HandleAsync(UpdateClinicalDataRequest req, CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }

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

