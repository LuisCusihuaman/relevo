using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

namespace Relevo.Web.Me;

public class GetHandoverActionItemsEndpoint(
    IShiftCheckInService _shiftCheckInService,
    IUserContext _userContext)
    : Endpoint<GetHandoverActionItemsRequest, GetHandoverActionItemsResponse>
{
    public override void Configure()
    {
        Get("/me/handovers/{handoverId}/action-items");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(GetHandoverActionItemsRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var actionItems = await _shiftCheckInService.GetHandoverActionItemsAsync(req.HandoverId);
        Response = new GetHandoverActionItemsResponse { ActionItems = actionItems };
        await SendAsync(Response, cancellation: ct);
    }
}
