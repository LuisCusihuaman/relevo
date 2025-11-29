using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Me.ContingencyPlans;

namespace Relevo.Web.Me;

public record GetMeContingencyPlansRequest
{
    [FromRoute]
    public string HandoverId { get; init; } = string.Empty;
}

public record GetMeContingencyPlansResponse
{
    public IReadOnlyList<ContingencyPlanRecord> ContingencyPlans { get; init; } = [];
}

public class GetMeContingencyPlansEndpoint(IMediator _mediator, ICurrentUser _currentUser)
    : Endpoint<GetMeContingencyPlansRequest, GetMeContingencyPlansResponse>
{
    public override void Configure()
    {
        Get("/me/handovers/{handoverId}/contingency-plans");
    }

    public override async Task HandleAsync(GetMeContingencyPlansRequest req, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
        {
             await SendUnauthorizedAsync(ct);
             return;
        }

        var result = await _mediator.Send(new GetMeContingencyPlansQuery(req.HandoverId), ct);

        if (!result.IsSuccess)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        Response = new GetMeContingencyPlansResponse { ContingencyPlans = result.Value };
        await SendAsync(Response, cancellation: ct);
    }
}

