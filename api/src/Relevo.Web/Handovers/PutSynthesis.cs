using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.UpdateSynthesis;
using System.ComponentModel.DataAnnotations;

namespace Relevo.Web.Handovers;

public class PutSynthesis(IMediator _mediator)
  : Endpoint<PutSynthesisRequest, PutSynthesisResponse>
{
  public override void Configure()
  {
    Put("/handovers/{handoverId}/synthesis");
    AllowAnonymous();
  }

  public override async Task HandleAsync(PutSynthesisRequest req, CancellationToken ct)
  {
    // Mock user
    var userId = "dr-1";

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

