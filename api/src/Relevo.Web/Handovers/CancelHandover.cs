using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.StateMachine;

namespace Relevo.Web.Handovers;

public class CancelHandover(IMediator _mediator)
  : Endpoint<CancelHandoverRequest>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/cancel");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancelHandoverRequest req, CancellationToken ct)
  {
    var userId = "dr-1"; 
    var result = await _mediator.Send(new CancelHandoverCommand(req.HandoverId, userId), ct);

    if (result.IsSuccess) await SendOkAsync(ct);
    else await SendNotFoundAsync(ct);
  }
}

public class CancelHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
}

