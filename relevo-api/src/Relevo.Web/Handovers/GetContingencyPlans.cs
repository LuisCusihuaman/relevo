using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.GetContingencyPlans;
using Relevo.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Relevo.Web.Handovers;

public class GetContingencyPlans(IMediator _mediator)
  : Endpoint<GetContingencyPlansRequest, GetContingencyPlansResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}/contingency-plans");
  }

  public override async Task HandleAsync(GetContingencyPlansRequest req, CancellationToken ct)
  {
    var result = await _mediator.Send(new GetContingencyPlansQuery(req.HandoverId), ct);

    if (result.IsSuccess)
    {
      Response = new GetContingencyPlansResponse
      {
        Plans = result.Value.Select(p => new ContingencyPlanDto
        {
            Id = p.Id,
            HandoverId = p.HandoverId,
            ConditionText = p.ConditionText,
            ActionText = p.ActionText,
            Priority = p.Priority,
            Status = p.Status,
            CreatedBy = p.CreatedBy,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList()
      };
      await SendAsync(Response, cancellation: ct);
    }
  }
}

public class GetContingencyPlansRequest
{
  public string HandoverId { get; set; } = string.Empty;
}

public class GetContingencyPlansResponse
{
  public List<ContingencyPlanDto> Plans { get; set; } = [];
}

// ...

public class ContingencyPlanDto
{
    [Required]
    public required string Id { get; set; }
    [Required]
    public required string HandoverId { get; set; }
    [Required]
    public required string ConditionText { get; set; }
    [Required]
    public required string ActionText { get; set; }
    [Required]
    public required string Priority { get; set; }
    [Required]
    public required string Status { get; set; }
    [Required]
    public required string CreatedBy { get; set; }
    [Required]
    public required DateTime CreatedAt { get; set; }
    [Required]
    public required DateTime UpdatedAt { get; set; }
}

