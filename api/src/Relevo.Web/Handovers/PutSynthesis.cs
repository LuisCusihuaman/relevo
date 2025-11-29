using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.UpdateSynthesis;

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

    var command = new UpdateHandoverSynthesisCommand(req.HandoverId, req.Content, req.Status, userId);
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
        AddError(result.Errors.FirstOrDefault() ?? "Error updating synthesis");
        await SendErrorsAsync(cancellation: ct);
    }
  }
}

public class PutSynthesisRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = "draft";
}

public class PutSynthesisResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

