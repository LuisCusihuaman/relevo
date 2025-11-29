using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

namespace Relevo.Web.Me;

public class CreateHandoverActionItemEndpoint(
    IShiftCheckInService _shiftCheckInService,
    IUserContext _userContext)
    : Endpoint<CreateHandoverActionItemRequest, CreateHandoverActionItemResponse>
{
    public override void Configure()
    {
        Post("/me/handovers/{handoverId}/action-items");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(CreateHandoverActionItemRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var actionItemId = await _shiftCheckInService.CreateHandoverActionItemAsync(
            req.HandoverId,
            req.Description,
            req.Priority
        );

        Response = new CreateHandoverActionItemResponse
        {
            Success = true,
            ActionItemId = actionItemId
        };
        await SendAsync(Response, cancellation: ct);
    }
}
