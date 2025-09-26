using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Handovers;

public class PutSynthesis(
    ISetupService _setupService,
    IUserContext _userContext,
    ILogger<PutSynthesis> _logger)
  : Endpoint<PutSynthesisRequest, ApiResponse>
{
  public override void Configure()
  {
    Put("/handovers/{handoverId}/synthesis");
    AllowAnonymous(); // Middleware handles auth
  }

  public override async Task HandleAsync(PutSynthesisRequest req, CancellationToken ct)
  {
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("PutSynthesis - Handover ID: {HandoverId}, User ID: {UserId}", req.HandoverId, user.Id);

    var success = await _setupService.UpdateSynthesisAsync(req.HandoverId, req.Content, req.Status, user.Id);

    if (!success)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    await SendAsync(new ApiResponse { Success = true, Message = "Synthesis updated successfully." }, cancellation: ct);
  }
}

public class PutSynthesisRequest
{
    public required string HandoverId { get; set; }
    public string? Content { get; set; }
    public string Status { get; set; } = "draft";
}
