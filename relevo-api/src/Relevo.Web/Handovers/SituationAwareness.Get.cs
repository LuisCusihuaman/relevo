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

    var sections = await _setupService.GetHandoverSectionsAsync(req.HandoverId);
    var situationAwarenessSection = sections.FirstOrDefault(s => s.SectionType == "situation_awareness");

    Response = new GetSituationAwarenessResponse
    {
        Section = situationAwarenessSection != null ? new SituationAwarenessSectionDto
        {
            Id = situationAwarenessSection.Id,
            HandoverId = situationAwarenessSection.HandoverId,
            SectionType = situationAwarenessSection.SectionType,
            Content = situationAwarenessSection.Content,
            Status = situationAwarenessSection.Status,
            LastEditedBy = situationAwarenessSection.LastEditedBy,
            CreatedAt = situationAwarenessSection.CreatedAt,
            UpdatedAt = situationAwarenessSection.UpdatedAt
        } : null
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
    public string Id { get; set; } = string.Empty;
    public string HandoverId { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LastEditedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
