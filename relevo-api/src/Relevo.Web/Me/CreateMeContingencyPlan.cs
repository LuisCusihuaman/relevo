using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Me.ContingencyPlans;

namespace Relevo.Web.Me;

public record CreateMeContingencyPlanRequest
{
    [FromRoute]
    public string HandoverId { get; init; } = string.Empty;

    public string ConditionText { get; init; } = string.Empty;
    public string ActionText { get; init; } = string.Empty;
    public string Priority { get; init; } = "medium"; // V3: Must be lowercase per CHK_CONT_PRIORITY constraint
}

public record CreateMeContingencyPlanResponse
{
    public bool Success { get; init; }
    public ContingencyPlanRecord? ContingencyPlan { get; init; }
}

public class CreateMeContingencyPlanEndpoint(IMediator _mediator, ICurrentUser _currentUser)
    : Endpoint<CreateMeContingencyPlanRequest, CreateMeContingencyPlanResponse>
{
    public override void Configure()
    {
        Post("/me/handovers/{handoverId}/contingency-plans");
    }

    public override async Task HandleAsync(CreateMeContingencyPlanRequest req, CancellationToken ct)
    {
        var userId = _currentUser.Id;
        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

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

