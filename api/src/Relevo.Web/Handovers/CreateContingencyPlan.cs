using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.CreateContingencyPlan;
using Relevo.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Relevo.Web.Handovers;

public class CreateContingencyPlan(IMediator _mediator)
  : Endpoint<CreateContingencyPlanRequest, CreateContingencyPlanResponse>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/contingency-plans");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CreateContingencyPlanRequest req, CancellationToken ct)
  {
    // Mock user
    var userId = "dr-1";

    var command = new CreateContingencyPlanCommand(
        req.HandoverId,
        req.ConditionText,
        req.ActionText,
        req.Priority,
        userId
    );

    var result = await _mediator.Send(command, ct);

    if (result.IsSuccess)
    {
        var plan = result.Value;
        Response = new CreateContingencyPlanResponse
        {
            Plan = new ContingencyPlanDto
            {
                Id = plan.Id,
                HandoverId = plan.HandoverId,
                ConditionText = plan.ConditionText,
                ActionText = plan.ActionText,
                Priority = plan.Priority,
                Status = plan.Status,
                CreatedBy = plan.CreatedBy,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt
            }
        };
        await SendAsync(Response, cancellation: ct);
    }
    else
    {
        AddError(result.Errors.FirstOrDefault() ?? "Error creating contingency plan");
        await SendErrorsAsync(cancellation: ct);
    }
  }
}

public class CreateContingencyPlanRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public string ConditionText { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

public class CreateContingencyPlanResponse
{
    public ContingencyPlanDto Plan { get; set; } = new();
}

