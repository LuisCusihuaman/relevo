using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.StateMachine;

namespace Relevo.Web.Handovers;

public class StartHandover(IMediator _mediator)
  : Endpoint<StartHandoverRequest>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/start");
    AllowAnonymous();
  }

  public override async Task HandleAsync(StartHandoverRequest req, CancellationToken ct)
  {
    var userId = "dr-1"; 
    var result = await _mediator.Send(new StartHandoverCommand(req.HandoverId, userId), ct);

    if (result.IsSuccess) await SendOkAsync(ct);
    else await SendNotFoundAsync(ct);
  }
}

public class StartHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
}

