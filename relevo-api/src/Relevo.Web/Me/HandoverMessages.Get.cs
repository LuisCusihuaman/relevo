using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

namespace Relevo.Web.Me;

public class GetHandoverMessagesEndpoint(
    ISetupService _setupService,
    IUserContext _userContext)
    : Endpoint<GetHandoverMessagesRequest, GetHandoverMessagesResponse>
{
    public override void Configure()
    {
        Get("/me/handovers/{handoverId}/messages");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(GetHandoverMessagesRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var messages = await _setupService.GetHandoverMessagesAsync(req.HandoverId);
        Response = new GetHandoverMessagesResponse { Messages = messages };
        await SendAsync(Response, cancellation: ct);
    }
}
