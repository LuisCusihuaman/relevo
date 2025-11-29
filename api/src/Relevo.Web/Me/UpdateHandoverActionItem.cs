using FastEndpoints;
using MediatR;
using Relevo.UseCases.Me.ActionItems;

namespace Relevo.Web.Me;

public class UpdateHandoverActionItem(IMediator _mediator)
    : Endpoint<UpdateHandoverActionItemRequest, UpdateHandoverActionItemResponse>
{
    public override void Configure()
    {
        Put("/me/handovers/{handoverId}/action-items/{itemId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateHandoverActionItemRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateHandoverActionItemCommand(req.HandoverId, req.ItemId, req.IsCompleted),
            ct);

        if (result.IsSuccess)
        {
            Response = new UpdateHandoverActionItemResponse { Success = true };
            await SendAsync(Response, cancellation: ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}

public class UpdateHandoverActionItemRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class UpdateHandoverActionItemResponse
{
    public bool Success { get; set; }
}

