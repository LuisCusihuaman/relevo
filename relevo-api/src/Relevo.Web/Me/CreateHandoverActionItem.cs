using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Me.ActionItems;

namespace Relevo.Web.Me;

public class CreateHandoverActionItem(IMediator _mediator, ICurrentUser _currentUser)
    : Endpoint<CreateHandoverActionItemRequest, CreateHandoverActionItemResponse>
{
    public override void Configure()
    {
        Post("/me/handovers/{handoverId}/action-items");
    }

    public override async Task HandleAsync(CreateHandoverActionItemRequest req, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
        {
             await SendUnauthorizedAsync(ct);
             return;
        }

        var result = await _mediator.Send(
            new CreateHandoverActionItemCommand(
                req.HandoverId, 
                req.Description, 
                req.Priority,
                req.DueTime,
                _currentUser.FullName ?? _currentUser.Id ?? "Unknown"),
            ct);

        if (result.IsSuccess)
        {
            Response = new CreateHandoverActionItemResponse
            {
                Success = true,
                ActionItemId = result.Value.Id
            };
            await SendAsync(Response, cancellation: ct);
        }
        else
        {
            await SendErrorsAsync(cancellation: ct);
        }
    }
}

public class CreateHandoverActionItemRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "medium";
    public string? DueTime { get; set; }
}

public class CreateHandoverActionItemResponse
{
    public bool Success { get; set; }
    public string ActionItemId { get; set; } = string.Empty;
}

