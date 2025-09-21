using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Handovers;

[HttpPost("/handovers/{id}/ready"), Authorize]
public class ReadyHandoverEndpoint : Endpoint<ReadyHandoverRequest>
{
    private readonly IHandoverStateService _handoverStateService;

    public ReadyHandoverEndpoint(IHandoverStateService handoverStateService)
    {
        _handoverStateService = handoverStateService;
    }

    public override async Task HandleAsync(ReadyHandoverRequest req, CancellationToken ct)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var success = await _handoverStateService.TryMarkAsReadyAsync(req.Id, userId);

        if (success)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(new {}, 400, ct);
        }
    }
}

public class ReadyHandoverRequest
{
    public string Id { get; set; } = null!;
}
