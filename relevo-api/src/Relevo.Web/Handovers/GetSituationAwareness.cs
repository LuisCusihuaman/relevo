using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.GetSituationAwareness;
using Relevo.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Relevo.Web.Handovers;

public class GetSituationAwareness(IMediator _mediator)
  : Endpoint<GetSituationAwarenessRequest, GetSituationAwarenessResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}/situation-awareness");
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

// ...

public class SituationAwarenessDto
{
    [Required]
    public required string HandoverId { get; set; }
    public string? Content { get; set; }
    [Required]
    public required string Status { get; set; }
    [Required]
    public required string LastEditedBy { get; set; }
    [Required]
    public required DateTime UpdatedAt { get; set; }
}

