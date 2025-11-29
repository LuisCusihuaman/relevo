using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.GetSynthesis;
using Relevo.Core.Models;

namespace Relevo.Web.Handovers;

public class GetSynthesis(IMediator _mediator)
  : Endpoint<GetSynthesisRequest, GetSynthesisResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}/synthesis");
  }

  public override async Task HandleAsync(GetSynthesisRequest req, CancellationToken ct)
  {
    var result = await _mediator.Send(new GetHandoverSynthesisQuery(req.HandoverId), ct);

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    if (result.IsSuccess)
    {
        var synthesis = result.Value;
        Response = new GetSynthesisResponse
        {
            Synthesis = new SynthesisDto
            {
                HandoverId = synthesis.HandoverId,
                Content = synthesis.Content,
                Status = synthesis.Status,
                LastEditedBy = synthesis.LastEditedBy,
                UpdatedAt = synthesis.UpdatedAt
            }
        };
        await SendAsync(Response, cancellation: ct);
    }
  }
}

public class GetSynthesisRequest
{
    public string HandoverId { get; set; } = string.Empty;
}

public class GetSynthesisResponse
{
    public SynthesisDto? Synthesis { get; set; }
}

public class SynthesisDto
{
    public string HandoverId { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = string.Empty;
    public string LastEditedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

