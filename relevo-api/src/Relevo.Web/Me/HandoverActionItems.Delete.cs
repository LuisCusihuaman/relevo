using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

namespace Relevo.Web.Me;

public class DeleteHandoverActionItemEndpoint(
    ISetupService _setupService,
    IUserContext _userContext)
    : Endpoint<DeleteHandoverActionItemRequest, DeleteHandoverActionItemResponse>
{
    public override void Configure()
    {
        Delete("/me/handovers/{handoverId}/action-items/{itemId}");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(DeleteHandoverActionItemRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var success = await _setupService.DeleteHandoverActionItemAsync(
            req.HandoverId,
            req.ItemId
        );

        Response = new DeleteHandoverActionItemResponse { Success = success };
        await SendAsync(Response, cancellation: ct);
    }
}
