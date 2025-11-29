using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Me.Messages;

namespace Relevo.Web.Me;

public record CreateHandoverMessageRequest
{
    [FromRoute]
    public string HandoverId { get; init; } = string.Empty;

    public string MessageText { get; init; } = string.Empty;
    public string? MessageType { get; init; }
}

public record CreateHandoverMessageResponse
{
    public bool Success { get; init; }
    public HandoverMessageRecord? Message { get; init; }
}

public class CreateHandoverMessageEndpoint(IMediator _mediator, ICurrentUser _currentUser)
    : Endpoint<CreateHandoverMessageRequest, CreateHandoverMessageResponse>
{
    public override void Configure()
    {
        Post("/me/handovers/{handoverId}/messages");
    }

    public override async Task HandleAsync(CreateHandoverMessageRequest req, CancellationToken ct)
    {
        var userId = _currentUser.Id;
        var userName = _currentUser.Email ?? "Unknown"; // Fallback as ICurrentUser doesn't expose Name yet

        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await _mediator.Send(
            new CreateHandoverMessageCommand(
                req.HandoverId,
                userId,
                userName,
                req.MessageText,
                req.MessageType ?? "message"),
            ct);

        if (!result.IsSuccess)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        Response = new CreateHandoverMessageResponse
        {
            Success = true,
            Message = result.Value
        };
        await SendAsync(Response, cancellation: ct);
    }
}

