using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.UpdateSituationAwareness;
using Microsoft.AspNetCore.Mvc;

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

    // FastEndpoints automatically binds route parameters to matching request properties
    // The route parameter {handoverId} is bound to req.HandoverId
    var handoverId = req.HandoverId;
    
    if (string.IsNullOrEmpty(handoverId))
    {
        AddError("HandoverId is required");
        await SendErrorsAsync(statusCode: 400, ct);
        return;
    }

    var result = await _mediator.Send(new UpdateHandoverSituationAwarenessCommand(
        handoverId,
        req.Content,
        req.Status,
        userId
    ), ct);

    if (result.IsSuccess)
    {
        await SendOkAsync(ct);
    }
    else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
        await SendNotFoundAsync(ct);
    }
    else
    {
        // Check if error message indicates "not found" and return 404, otherwise 400
        var errorMessage = result.Errors.FirstOrDefault() ?? "Error updating situation awareness";
        if (errorMessage.Contains("may not exist") || errorMessage.Contains("not exist"))
        {
            await SendNotFoundAsync(ct);
        }
        else
        {
            AddError(errorMessage);
            await SendErrorsAsync(statusCode: 400, ct);
        }
    }
  }
}

public class UpdateSituationAwarenessRequest
{
    [FromRoute]
    public string HandoverId { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = string.Empty;
}
