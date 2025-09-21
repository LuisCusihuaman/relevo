using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Handovers;

public class UpdateSituationAwareness(
    ISetupService _setupService,
    IUserContext _userContext,
    ILogger<UpdateSituationAwareness> _logger)
  : Endpoint<UpdateSituationAwarenessRequest, UpdateSituationAwarenessResponse>
{
  public override void Configure()
  {
    Put("/handovers/{handoverId}/situation-awareness");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(UpdateSituationAwarenessRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("UpdateSituationAwareness - Handover ID: {HandoverId}, User ID: {UserId}", req.HandoverId, user.Id);

    // First get the situation awareness section
    var sections = await _setupService.GetHandoverSectionsAsync(req.HandoverId);
    var situationAwarenessSection = sections.FirstOrDefault(s => s.SectionType == "situation_awareness");

    if (situationAwarenessSection == null)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    var success = await _setupService.UpdateHandoverSectionAsync(
        req.HandoverId,
        situationAwarenessSection.Id,
        req.Content ?? "",
        "completed", // Mark as completed when updated
        user.Id
    );

    if (!success)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    Response = new UpdateSituationAwarenessResponse
    {
        Success = true,
        Message = "Situation awareness updated successfully"
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class UpdateSituationAwarenessRequest
{
    public required string HandoverId { get; set; }
    public required string Content { get; set; }
}

public class UpdateSituationAwarenessResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
