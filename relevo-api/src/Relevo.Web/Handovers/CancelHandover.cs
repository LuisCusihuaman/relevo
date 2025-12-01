using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.StateMachine;

namespace Relevo.Web.Handovers;

public class CancelHandover(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<CancelHandoverRequest>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/cancel");
  }

  public override async Task HandleAsync(CancelHandoverRequest req, CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }
    
    // V3: CancelReason is required
    var cancelReason = string.IsNullOrWhiteSpace(req.CancelReason) ? "UserCancelled" : req.CancelReason;
    var result = await _mediator.Send(new CancelHandoverCommand(req.HandoverId, userId, cancelReason), ct);

    if (result.IsSuccess) 
      await SendOkAsync(ct);
    else
    {
      AddError("Cannot cancel handover: handover not found or state change not allowed.");
      await SendErrorsAsync(statusCode: 400, ct);
    }
  }
}

public class CancelHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public string? CancelReason { get; set; } // V3: CancelReason required (defaults to "UserCancelled" if not provided)
}

