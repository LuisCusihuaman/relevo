using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Relevo.Web.Handovers;

public class UpdateReceiver(ICurrentUser _currentUser, IHandoverRepository _repository)
    : Endpoint<UpdateReceiverRequest>
{
    public override void Configure()
    {
        Put("/handovers/{handoverId}/receiver");
    }

    public override async Task HandleAsync(UpdateReceiverRequest req, CancellationToken ct)
    {
        var userId = _currentUser.Id;
        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var success = await _repository.UpdateReceiverAsync(req.HandoverId, req.ReceiverUserId);

        if (success)
        {
            await SendOkAsync(ct);
        }
        else
        {
            AddError("Failed to update receiver");
            await SendErrorsAsync(statusCode: 400, cancellation: ct);
        }
    }
}

public class UpdateReceiverRequest
{
    [FromRoute]
    public string HandoverId { get; set; } = string.Empty;
    
    public string ReceiverUserId { get; set; } = string.Empty;
}

