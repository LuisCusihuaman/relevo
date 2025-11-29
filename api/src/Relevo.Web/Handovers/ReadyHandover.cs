using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.SetReady;
using Microsoft.AspNetCore.Mvc;

namespace Relevo.Web.Handovers;

public class ReadyHandover(IMediator _mediator)
  : Endpoint<ReadyHandoverRequest>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/ready");
    AllowAnonymous();
  }

  public override async Task HandleAsync(ReadyHandoverRequest req, CancellationToken ct)
  {
    var userId = "dr-1"; // Mock user for migration

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

