using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Relevo.Core.Models;
using Relevo.UseCases.Me.Checklists;

namespace Relevo.Web.Me;

public record GetHandoverChecklistsRequest
{
    [FromRoute]
    public string HandoverId { get; init; } = string.Empty;
}

public record GetHandoverChecklistsResponse
{
    public IReadOnlyList<HandoverChecklistRecord> Checklists { get; init; } = [];
}

public class GetHandoverChecklistsEndpoint(IMediator _mediator)
    : Endpoint<GetHandoverChecklistsRequest, GetHandoverChecklistsResponse>
{
    public override void Configure()
    {
        Get("/me/handovers/{handoverId}/checklists");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetHandoverChecklistsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetHandoverChecklistsQuery(req.HandoverId), ct);

        if (!result.IsSuccess)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        Response = new GetHandoverChecklistsResponse { Checklists = result.Value };
        await SendAsync(Response, cancellation: ct);
    }
}

