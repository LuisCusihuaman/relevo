using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.GetContingencyPlans;
using Relevo.Core.Models;

namespace Relevo.Web.Handovers;

public class GetContingencyPlans(IMediator _mediator)
  : Endpoint<GetContingencyPlansRequest, GetContingencyPlansResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}/contingency-plans");
    AllowAnonymous();
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

public class ContingencyPlanDto
{
    public string Id { get; set; } = string.Empty;
    public string HandoverId { get; set; } = string.Empty;
    public string ConditionText { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

