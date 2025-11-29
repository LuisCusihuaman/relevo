using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.GetSituationAwareness;
using Relevo.Core.Models;

namespace Relevo.Web.Handovers;

public class GetSituationAwareness(IMediator _mediator)
  : Endpoint<GetSituationAwarenessRequest, GetSituationAwarenessResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}/situation-awareness");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetSituationAwarenessRequest req, CancellationToken ct)
  {
    var result = await _mediator.Send(new GetHandoverSituationAwarenessQuery(req.HandoverId), ct);

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    if (result.IsSuccess)
    {
        var sa = result.Value;
        Response = new GetSituationAwarenessResponse
        {
            SituationAwareness = new SituationAwarenessDto
            {
                HandoverId = sa.HandoverId,
                Content = sa.Content,
                Status = sa.Status,
                LastEditedBy = sa.LastEditedBy,
                UpdatedAt = sa.UpdatedAt
            }
        };
        await SendAsync(Response, cancellation: ct);
    }
  }
}

public class GetSituationAwarenessRequest
{
    public string HandoverId { get; set; } = string.Empty;
}

public class GetSituationAwarenessResponse
{
    public SituationAwarenessDto? SituationAwareness { get; set; }
}

public class SituationAwarenessDto
{
    public string HandoverId { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = string.Empty;
    public string LastEditedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

