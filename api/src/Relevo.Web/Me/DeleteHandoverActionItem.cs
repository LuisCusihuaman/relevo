using FastEndpoints;
using MediatR;
using Relevo.UseCases.Me.ActionItems;

namespace Relevo.Web.Me;

public class DeleteHandoverActionItem(IMediator _mediator)
    : Endpoint<DeleteHandoverActionItemRequest, DeleteHandoverActionItemResponse>
{
    public override void Configure()
    {
        Delete("/me/handovers/{handoverId}/action-items/{itemId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteHandoverActionItemRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new DeleteHandoverActionItemCommand(req.HandoverId, req.ItemId),
            ct);

        if (result.IsSuccess)
        {
            Response = new DeleteHandoverActionItemResponse { Success = true };
            await SendAsync(Response, cancellation: ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}

public class DeleteHandoverActionItemRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
}

public class DeleteHandoverActionItemResponse
{
    public bool Success { get; set; }
}

