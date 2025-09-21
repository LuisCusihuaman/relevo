using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

namespace Relevo.Web.Me;

public class CreateHandoverMessageEndpoint(
    ISetupService _setupService,
    IUserContext _userContext)
    : Endpoint<CreateHandoverMessageRequest, CreateHandoverMessageResponse>
{
    public override void Configure()
    {
        Post("/me/handovers/{handoverId}/messages");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(CreateHandoverMessageRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var message = await _setupService.CreateHandoverMessageAsync(
            req.HandoverId,
            user.Id,
            user.FirstName + " " + user.LastName,
            req.MessageText,
            req.MessageType ?? "message"
        );

        Response = new CreateHandoverMessageResponse { Success = true, Message = message };
        await SendAsync(Response, cancellation: ct);
    }
}
