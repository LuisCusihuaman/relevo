using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.StateMachine;

namespace Relevo.Web.Handovers;

public class RejectHandover(IMediator _mediator)
  : Endpoint<RejectHandoverRequest>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/reject");
    AllowAnonymous();
  }

  public override async Task HandleAsync(RejectHandoverRequest req, CancellationToken ct)
  {
    var userId = "dr-1"; 
    var result = await _mediator.Send(new RejectHandoverCommand(req.HandoverId, req.Reason, userId), ct);

    if (result.IsSuccess) await SendOkAsync(ct);
    else await SendNotFoundAsync(ct); // Or BadRequest if reason missing
  }
}

public class RejectHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

