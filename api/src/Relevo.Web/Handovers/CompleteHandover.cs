using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.StateMachine;

namespace Relevo.Web.Handovers;

public class CompleteHandover(IMediator _mediator)
  : Endpoint<CompleteHandoverRequest>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/complete");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CompleteHandoverRequest req, CancellationToken ct)
  {
    var userId = "dr-1"; 
    var result = await _mediator.Send(new CompleteHandoverCommand(req.HandoverId, userId), ct);

    if (result.IsSuccess) await SendOkAsync(ct);
    else await SendNotFoundAsync(ct);
  }
}

public class CompleteHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
}

