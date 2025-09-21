using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

namespace Relevo.Web.Me;

public class GetHandoverContingencyEndpoint(
    ISetupService _setupService,
    IUserContext _userContext)
    : Endpoint<GetHandoverContingencyRequest, GetHandoverContingencyResponse>
{
    public override void Configure()
    {
        Get("/me/handovers/{handoverId}/contingency-plans");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(GetHandoverContingencyRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var contingencyPlans = await _setupService.GetHandoverContingencyPlansAsync(req.HandoverId);
        Response = new GetHandoverContingencyResponse { ContingencyPlans = contingencyPlans };
        await SendAsync(Response, cancellation: ct);
    }
}
