using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

namespace Relevo.Web.Me;

public class GetHandoverActivityEndpoint(
    IShiftCheckInService _shiftCheckInService,
    IUserContext _userContext)
    : Endpoint<GetHandoverActivityRequest, GetHandoverActivityResponse>
{
    public override void Configure()
    {
        Get("/me/handovers/{handoverId}/activity");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(GetHandoverActivityRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var activities = await _shiftCheckInService.GetHandoverActivityLogAsync(req.HandoverId);
        Response = new GetHandoverActivityResponse { Activities = activities };
        await SendAsync(Response, cancellation: ct);
    }
}
