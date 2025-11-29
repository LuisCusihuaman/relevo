using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

namespace Relevo.Web.Me;

public class UpdateHandoverActionItemEndpoint(
    IShiftCheckInService _shiftCheckInService,
    IUserContext _userContext)
    : Endpoint<UpdateHandoverActionItemRequest, UpdateHandoverActionItemResponse>
{
    public override void Configure()
    {
        Put("/me/handovers/{handoverId}/action-items/{itemId}");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(UpdateHandoverActionItemRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var success = await _shiftCheckInService.UpdateHandoverActionItemAsync(
            req.HandoverId,
            req.ItemId,
            req.IsCompleted
        );

        Response = new UpdateHandoverActionItemResponse { Success = success };
        await SendAsync(Response, cancellation: ct);
    }
}
