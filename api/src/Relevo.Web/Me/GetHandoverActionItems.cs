using FastEndpoints;
using MediatR;
using Relevo.Core.Models;
using Relevo.UseCases.Me.ActionItems;

namespace Relevo.Web.Me;

public class GetHandoverActionItems(IMediator _mediator)
    : Endpoint<GetHandoverActionItemsRequest, GetHandoverActionItemsResponse>
{
    public override void Configure()
    {
        Get("/me/handovers/{handoverId}/action-items");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetHandoverActionItemsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetHandoverActionItemsQuery(req.HandoverId), ct);

        if (result.IsSuccess)
        {
            Response = new GetHandoverActionItemsResponse { ActionItems = result.Value };
            await SendAsync(Response, cancellation: ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}

public class GetHandoverActionItemsRequest
{
    public string HandoverId { get; set; } = string.Empty;
}

public class GetHandoverActionItemsResponse
{
    public IReadOnlyList<HandoverActionItemFullRecord> ActionItems { get; set; } = [];
}

