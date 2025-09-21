using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Handovers;

public class DeleteContingencyPlan(
    ISetupService _setupService,
    IUserContext _userContext,
    ILogger<DeleteContingencyPlan> _logger)
  : Endpoint<DeleteContingencyPlanRequest, DeleteContingencyPlanResponse>
{
  public override void Configure()
  {
    Delete("/handovers/{handoverId}/contingency-plans/{contingencyId}");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(DeleteContingencyPlanRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("DeleteContingencyPlan - Handover ID: {HandoverId}, Contingency ID: {ContingencyId}, User ID: {UserId}",
        req.HandoverId, req.ContingencyId, user.Id);

    var success = await _setupService.DeleteContingencyPlanAsync(req.HandoverId, req.ContingencyId);

    if (!success)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    Response = new DeleteContingencyPlanResponse
    {
        Success = true,
        Message = "Contingency plan deleted successfully"
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class DeleteContingencyPlanRequest
{
    public required string HandoverId { get; set; }
    public required string ContingencyId { get; set; }
}

public class DeleteContingencyPlanResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
