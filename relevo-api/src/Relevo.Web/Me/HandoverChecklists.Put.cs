using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

namespace Relevo.Web.Me;

public class UpdateChecklistItemEndpoint(
    IShiftCheckInService _shiftCheckInService,
    IUserContext _userContext)
    : Endpoint<UpdateChecklistItemRequest, UpdateChecklistItemResponse>
{
    public override void Configure()
    {
        Put("/me/handovers/{handoverId}/checklists/{itemId}");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(UpdateChecklistItemRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var success = await _shiftCheckInService.UpdateChecklistItemAsync(
            req.HandoverId,
            req.ItemId,
            req.IsChecked,
            user.Id
        );

        Response = new UpdateChecklistItemResponse
        {
            Success = success,
            Message = success ? "Checklist item updated successfully" : "Failed to update checklist item"
        };
        await SendAsync(Response, cancellation: ct);
    }
}
