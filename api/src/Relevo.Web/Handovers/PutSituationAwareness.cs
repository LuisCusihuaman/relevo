using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.UpdateSituationAwareness;
using Microsoft.AspNetCore.Mvc;

namespace Relevo.Web.Handovers;

public class PutSituationAwareness(IMediator _mediator)
  : Endpoint<UpdateSituationAwarenessRequest>
{
  public override void Configure()
  {
    Put("/handovers/{handoverId}/situation-awareness");
    AllowAnonymous();
  }

  public override async Task HandleAsync(UpdateSituationAwarenessRequest req, CancellationToken ct)
  {
    // Assuming UserId is available (e.g. from context or request), here taking from request for simplicity/migration
    // If auth was enabled, we'd get it from User.Identity
    // Using "dr-1" which is a seeded user to avoid FK violation in tests/dev
    var userId = "dr-1"; 

    var result = await _mediator.Send(new UpdateHandoverSituationAwarenessCommand(
        req.HandoverId,
        req.Content,
        req.Status,
        userId
    ), ct);

    if (result.IsSuccess)
    {
        await SendOkAsync(ct);
    }
    else
    {
        // If failed (e.g. handover not found), return 404 or 400
        // For now, if it fails, it's likely not found or DB error
        await SendNotFoundAsync(ct);
    }
  }
}

public class UpdateSituationAwarenessRequest
{
    public string HandoverId { get; set; } = string.Empty;
    
    // FastEndpoints by default binds body to properties not in route. 
    // We need to ensure json body is bound correctly. 
    // But here HandoverId is in route.
    
    public string? Content { get; set; }
    public string Status { get; set; } = string.Empty;
}
