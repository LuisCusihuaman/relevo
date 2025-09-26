using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Handovers;

public class PutSituationAwareness(
    ISetupService _setupService,
    IUserContext _userContext,
    ILogger<PutSituationAwareness> _logger)
  : Endpoint<PutSituationAwarenessRequest, ApiResponse>
{
  public override void Configure()
  {
    Put("/handovers/{handoverId}/situation-awareness");
    AllowAnonymous(); // Middleware handles auth
  }

  public override async Task HandleAsync(PutSituationAwarenessRequest req, CancellationToken ct)
  {
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("PutSituationAwareness - Handover ID: {HandoverId}, User ID: {UserId}", req.HandoverId, user.Id);

    var success = await _setupService.UpdateSituationAwarenessAsync(req.HandoverId, req.Content, req.Status, user.Id);

    if (!success)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    await SendAsync(new ApiResponse { Success = true, Message = "Situation awareness updated successfully." }, cancellation: ct);
  }
}

public class PutSituationAwarenessRequest
{
    public required string HandoverId { get; set; }
    public string? Content { get; set; }
    public string Status { get; set; } = "draft";
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
