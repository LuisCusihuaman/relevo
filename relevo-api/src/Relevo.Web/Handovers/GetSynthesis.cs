using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.GetSynthesis;
using Relevo.Core.Models;
using System.ComponentModel.DataAnnotations;

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

// ...

public class SynthesisDto
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

