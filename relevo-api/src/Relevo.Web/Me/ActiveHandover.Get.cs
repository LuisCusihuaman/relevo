using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

namespace Relevo.Web.Me;

public class GetActiveHandoverEndpoint(
    ISetupService _setupService,
    IUserContext _userContext)
  : EndpointWithoutRequest<GetActiveHandoverResponse>
{
  public override void Configure()
  {
    Get("/me/handovers/active");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    var activeHandover = await _setupService.GetActiveHandoverAsync(user.Id);

    if (activeHandover == null)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    Response = new GetActiveHandoverResponse
    {
        Handover = activeHandover,
        Participants = await _setupService.GetHandoverParticipantsAsync(activeHandover.Id),
        Sections = await _setupService.GetHandoverSectionsAsync(activeHandover.Id),
        SyncStatus = await _setupService.GetHandoverSyncStatusAsync(activeHandover.Id, user.Id)
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class GetActiveHandoverResponse
{
    public required DomainHandoverRecord Handover { get; set; }
    public required List<HandoverParticipantRecord> Participants { get; set; }
    public required List<HandoverSectionRecord> Sections { get; set; }
    public required HandoverSyncStatusRecord? SyncStatus { get; set; }
}

public record HandoverParticipantRecord(
    string Id,
    string UserId,
    string UserName,
    string? UserRole,
    string Status,
    DateTime JoinedAt,
    DateTime LastActivity
);

public record HandoverSectionRecord(
    string Id,
    string SectionType,
    string? Content,
    string Status,
    string? LastEditedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record HandoverSyncStatusRecord(
    string Id,
    string SyncStatus,
    DateTime LastSync,
    int Version
);
