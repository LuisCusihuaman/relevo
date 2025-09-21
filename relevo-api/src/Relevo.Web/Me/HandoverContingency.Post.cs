using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

namespace Relevo.Web.Me;

public class CreateContingencyPlanEndpoint(
    ISetupService _setupService,
    IUserContext _userContext)
    : Endpoint<CreateContingencyPlanRequest, CreateContingencyPlanResponse>
{
    public override void Configure()
    {
        Post("/me/handovers/{handoverId}/contingency-plans");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(CreateContingencyPlanRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var contingencyPlan = await _setupService.CreateContingencyPlanAsync(
            req.HandoverId,
            req.ConditionText,
            req.ActionText,
            req.Priority,
            user.Id
        );

        Response = new CreateContingencyPlanResponse { Success = true, ContingencyPlan = contingencyPlan };
        await SendAsync(Response, cancellation: ct);
    }
}
