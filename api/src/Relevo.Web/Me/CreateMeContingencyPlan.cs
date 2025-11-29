using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Relevo.Core.Models;
using Relevo.UseCases.Me.ContingencyPlans;

namespace Relevo.Web.Me;

public record CreateMeContingencyPlanRequest
{
    [FromRoute]
    public string HandoverId { get; init; } = string.Empty;

    public string ConditionText { get; init; } = string.Empty;
    public string ActionText { get; init; } = string.Empty;
    public string Priority { get; init; } = "Medium";
}

public record CreateMeContingencyPlanResponse
{
    public bool Success { get; init; }
    public ContingencyPlanRecord? ContingencyPlan { get; init; }
}

public class CreateMeContingencyPlanEndpoint(IMediator _mediator)
    : Endpoint<CreateMeContingencyPlanRequest, CreateMeContingencyPlanResponse>
{
    public override void Configure()
    {
        Post("/me/handovers/{handoverId}/contingency-plans");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateMeContingencyPlanRequest req, CancellationToken ct)
    {
        // TODO: Get actual user ID from authentication context
        var userId = "dr-1";

        var result = await _mediator.Send(
            new CreateMeContingencyPlanCommand(
                req.HandoverId,
                req.ConditionText,
                req.ActionText,
                req.Priority,
                userId),
            ct);

        if (!result.IsSuccess)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        Response = new CreateMeContingencyPlanResponse
        {
            Success = true,
            ContingencyPlan = result.Value
        };
        await SendAsync(Response, cancellation: ct);
    }
}

