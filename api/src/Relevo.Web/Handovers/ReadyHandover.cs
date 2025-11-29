using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.SetReady;

namespace Relevo.Web.Handovers;

public class ReadyHandover(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<ReadyHandoverRequest>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/ready");
  }

  public override async Task HandleAsync(ReadyHandoverRequest req, CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }

    var result = await _mediator.Send(new ReadyHandoverCommand(req.HandoverId, userId), ct);

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

public class ReadyHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
}

