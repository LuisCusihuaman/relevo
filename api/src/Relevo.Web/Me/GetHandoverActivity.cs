using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Relevo.Core.Models;
using Relevo.UseCases.Me.Activity;

namespace Relevo.Web.Me;

public record GetHandoverActivityRequest
{
    [FromRoute]
    public string HandoverId { get; init; } = string.Empty;
}

public record GetHandoverActivityResponse
{
    public IReadOnlyList<HandoverActivityRecord> Activities { get; init; } = [];
}

public class GetHandoverActivityEndpoint(IMediator _mediator)
    : Endpoint<GetHandoverActivityRequest, GetHandoverActivityResponse>
{
    public override void Configure()
    {
        Get("/me/handovers/{handoverId}/activity");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetHandoverActivityRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetHandoverActivityQuery(req.HandoverId), ct);

        if (!result.IsSuccess)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        Response = new GetHandoverActivityResponse { Activities = result.Value };
        await SendAsync(Response, cancellation: ct);
    }
}

