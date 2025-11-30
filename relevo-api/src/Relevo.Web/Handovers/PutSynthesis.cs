using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.UpdateSynthesis;
using Microsoft.AspNetCore.Mvc;

namespace Relevo.Web.Handovers;

public class PutSynthesis(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<PutSynthesisRequest, PutSynthesisResponse>
{
  public override void Configure()
  {
    Put("/handovers/{handoverId}/synthesis");
  }

  public override async Task HandleAsync(PutSynthesisRequest req, CancellationToken ct)
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

    var command = new UpdateHandoverSynthesisCommand(handoverId, req.Content, req.Status, userId);
    var result = await _mediator.Send(command, ct);

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
        await SendNotFoundAsync(ct);
        return;
    }

    if (result.IsSuccess)
    {
        Response = new PutSynthesisResponse
        {
            Success = true,
            Message = "Synthesis updated successfully."
        };
        await SendAsync(Response, cancellation: ct);
    }
    else
    {
        var errorMessage = result.Errors.FirstOrDefault() ?? "Error updating synthesis";
        AddError(errorMessage);
        await SendErrorsAsync(cancellation: ct);
    }
  }
}

public class PutSynthesisRequest
{
    [FromRoute]
    public string HandoverId { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = "Draft";
}

public class PutSynthesisResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

