using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.StateMachine;

namespace Relevo.Web.Handovers;

public class AcceptHandover(IMediator _mediator)
  : Endpoint<AcceptHandoverRequest>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/accept");
    AllowAnonymous();
  }

  public override async Task HandleAsync(AcceptHandoverRequest req, CancellationToken ct)
  {
    var userId = "dr-1"; 
    var result = await _mediator.Send(new AcceptHandoverCommand(req.HandoverId, userId), ct);

    if (result.IsSuccess) await SendOkAsync(ct);
    else await SendNotFoundAsync(ct);
  }
}

public class AcceptHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
}

