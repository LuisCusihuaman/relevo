using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.UpdateSituationAwareness;

namespace Relevo.Web.Handovers;

public class PutSituationAwareness(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<UpdateSituationAwarenessRequest>
{
  public override void Configure()
  {
    Put("/handovers/{handoverId}/situation-awareness");
  }

  public override async Task HandleAsync(UpdateSituationAwarenessRequest req, CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }

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
