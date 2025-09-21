using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Models;

// Use specific types from Core layer to avoid conflicts
using HandoverRecord = Relevo.Core.Interfaces.HandoverRecord;
using HandoverParticipantRecord = Relevo.Core.Interfaces.HandoverParticipantRecord;
using HandoverSectionRecord = Relevo.Core.Interfaces.HandoverSectionRecord;
using HandoverSyncStatusRecord = Relevo.Core.Interfaces.HandoverSyncStatusRecord;

// Additional types for new endpoints

namespace Relevo.Web.Me;

public class GetHandoverMessagesEndpoint(
    ISetupService _setupService,
    IUserContext _userContext)
    : Endpoint<GetHandoverMessagesRequest, GetHandoverMessagesResponse>
{
    public override void Configure()
    {
        Get("/me/handovers/{handoverId}/messages");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(GetHandoverMessagesRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var messages = await _setupService.GetHandoverMessagesAsync(req.HandoverId);
        Response = new GetHandoverMessagesResponse { Messages = messages };
        await SendAsync(Response, cancellation: ct);
    }
}

public class CreateHandoverMessageEndpoint(
    ISetupService _setupService,
    IUserContext _userContext)
    : Endpoint<CreateHandoverMessageRequest, CreateHandoverMessageResponse>
{
    public override void Configure()
    {
        Post("/me/handovers/{handoverId}/messages");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(CreateHandoverMessageRequest req, CancellationToken ct)
    {
        var user = _userContext.CurrentUser;
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var message = await _setupService.CreateHandoverMessageAsync(
            req.HandoverId,
            user.Id,
            user.FirstName + " " + user.LastName,
            req.MessageText,
            req.MessageType ?? "message"
        );

        Response = new CreateHandoverMessageResponse { Success = true, Message = message };
        await SendAsync(Response, cancellation: ct);
    }
}

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
    public required HandoverRecord Handover { get; set; }
    public required IReadOnlyList<HandoverParticipantRecord> Participants { get; set; }
    public required IReadOnlyList<HandoverSectionRecord> Sections { get; set; }
    public required HandoverSyncStatusRecord? SyncStatus { get; set; }
}
