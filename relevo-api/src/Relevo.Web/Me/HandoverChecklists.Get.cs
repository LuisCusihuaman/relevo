using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

namespace Relevo.Web.Me;

public class GetHandoverChecklistsEndpoint(
    IShiftCheckInService _shiftCheckInService,
    IUserContext _userContext)
    : Endpoint<GetHandoverChecklistsRequest, GetHandoverChecklistsResponse>
{
    public override void Configure()
    {
        Get("/me/handovers/{handoverId}/checklists");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(GetHandoverChecklistsRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var checklists = await _shiftCheckInService.GetHandoverChecklistsAsync(req.HandoverId);
        Response = new GetHandoverChecklistsResponse { Checklists = checklists };
        await SendAsync(Response, cancellation: ct);
    }
}
