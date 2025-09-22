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

        // Ensure user exists in database, fallback to demo user if not
        var effectiveUserId = user.Id;

        // Check if the user exists in the database by trying to get their preferences
        var userPreferences = await _setupService.GetUserPreferencesAsync(user.Id);
        if (userPreferences == null)
        {
            // User doesn't exist in database, use demo user that exists
            effectiveUserId = "user_demo12345678901234567890123456";
        }

        var contingencyPlan = await _setupService.CreateContingencyPlanAsync(
            req.HandoverId,
            req.ConditionText,
            req.ActionText,
            req.Priority,
            effectiveUserId
        );

        Response = new CreateContingencyPlanResponse { Success = true, ContingencyPlan = contingencyPlan };
        await SendAsync(Response, cancellation: ct);
    }
}
