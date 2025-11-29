using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Relevo.UseCases.Me.Checklists;

namespace Relevo.Web.Me;

public record UpdateChecklistItemRequest
{
    [FromRoute]
    public string HandoverId { get; init; } = string.Empty;

    [FromRoute]
    public string ItemId { get; init; } = string.Empty;

    public bool IsChecked { get; init; }
}

public record UpdateChecklistItemResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

public class UpdateChecklistItemEndpoint(IMediator _mediator)
    : Endpoint<UpdateChecklistItemRequest, UpdateChecklistItemResponse>
{
    public override void Configure()
    {
        Put("/me/handovers/{handoverId}/checklists/{itemId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateChecklistItemRequest req, CancellationToken ct)
    {
        // TODO: Get actual user ID from authentication context
        var userId = "dr-1";

        var result = await _mediator.Send(
            new UpdateChecklistItemCommand(req.HandoverId, req.ItemId, req.IsChecked, userId),
            ct);

        if (!result.IsSuccess)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        Response = new UpdateChecklistItemResponse
        {
            Success = true,
            Message = "Checklist item updated successfully"
        };
        await SendAsync(Response, cancellation: ct);
    }
}

