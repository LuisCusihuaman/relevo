using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Me.Messages;

namespace Relevo.Web.Me;

public record GetHandoverMessagesRequest
{
    [FromRoute]
    public string HandoverId { get; init; } = string.Empty;
}

public record GetHandoverMessagesResponse
{
    public IReadOnlyList<HandoverMessageRecord> Messages { get; init; } = [];
}

public class GetHandoverMessagesEndpoint(IMediator _mediator, ICurrentUser _currentUser)
    : Endpoint<GetHandoverMessagesRequest, GetHandoverMessagesResponse>
{
    public override void Configure()
    {
        Get("/me/handovers/{handoverId}/messages");
    }

    public override async Task HandleAsync(GetHandoverMessagesRequest req, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
        {
             await SendUnauthorizedAsync(ct);
             return;
        }

        var result = await _mediator.Send(new GetHandoverMessagesQuery(req.HandoverId), ct);

        if (!result.IsSuccess)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        Response = new GetHandoverMessagesResponse { Messages = result.Value };
        await SendAsync(Response, cancellation: ct);
    }
}

