using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

// Use specific types from Core layer to avoid conflicts
using HandoverRecord = Relevo.Core.Interfaces.HandoverRecord;
using HandoverParticipantRecord = Relevo.Core.Interfaces.HandoverParticipantRecord;
using HandoverSectionRecord = Relevo.Core.Interfaces.HandoverSectionRecord;
using HandoverSyncStatusRecord = Relevo.Core.Interfaces.HandoverSyncStatusRecord;

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

    // For now, return empty arrays to test if the main handover works
    Response = new GetActiveHandoverResponse
    {
        Handover = activeHandover,
        Participants = Array.Empty<HandoverParticipantRecord>(),
        Sections = Array.Empty<HandoverSectionRecord>(),
        SyncStatus = null
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class GetActiveHandoverResponse
{
    public required HandoverRecord Handover { get; set; }
    public required IReadOnlyList<HandoverParticipantRecord> Participants { get; set; }
    public required IReadOnlyList<HandoverSectionRecord> Sections { get; set; }
    public required HandoverSyncStatusRecord? SyncStatus { get; set; }
}
