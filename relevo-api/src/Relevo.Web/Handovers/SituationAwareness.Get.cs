using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Handovers;

public class GetSituationAwareness(
    ISetupService _setupService,
    IUserContext _userContext,
    ILogger<GetSituationAwareness> _logger)
  : Endpoint<GetSituationAwarenessRequest, GetSituationAwarenessResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}/situation-awareness");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(GetSituationAwarenessRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("GetSituationAwareness - Handover ID: {HandoverId}, User ID: {UserId}", req.HandoverId, user.Id);

    var situationAwarenessSection = await _setupService.GetSituationAwarenessAsync(req.HandoverId);

    if (situationAwarenessSection == null)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    Response = new GetSituationAwarenessResponse
    {
        Section = new SituationAwarenessSectionDto
        {
            HandoverId = situationAwarenessSection.HandoverId,
            Content = situationAwarenessSection.Content,
            Status = situationAwarenessSection.Status,
            LastEditedBy = situationAwarenessSection.LastEditedBy,
            UpdatedAt = situationAwarenessSection.UpdatedAt
        }
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class GetSituationAwarenessRequest
{
    public required string HandoverId { get; set; }
}

public class GetSituationAwarenessResponse
{
    public SituationAwarenessSectionDto? Section { get; set; }
}

public class SituationAwarenessSectionDto
{
    public string HandoverId { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LastEditedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
