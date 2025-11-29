using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Handovers;

public class CreateContingencyPlan(
    IShiftCheckInService _shiftCheckInService,
    IUserContext _userContext,
    ILogger<CreateContingencyPlan> _logger)
  : Endpoint<CreateContingencyPlanRequest, CreateContingencyPlanResponse>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/contingency-plans");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(CreateContingencyPlanRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("CreateContingencyPlan - Handover ID: {HandoverId}, User ID: {UserId}", req.HandoverId, user.Id);

    var plan = await _shiftCheckInService.CreateContingencyPlanAsync(
        req.HandoverId,
        req.ConditionText,
        req.ActionText,
        req.Priority,
        user.Id
    );

    Response = new CreateContingencyPlanResponse
    {
        Plan = new ContingencyPlanDto
        {
            Id = plan.Id,
            HandoverId = plan.HandoverId,
            ConditionText = plan.ConditionText,
            ActionText = plan.ActionText,
            Priority = plan.Priority,
            Status = plan.Status,
            CreatedBy = plan.CreatedBy,
            CreatedAt = plan.CreatedAt,
            UpdatedAt = plan.UpdatedAt
        }
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class CreateContingencyPlanRequest
{
    public required string HandoverId { get; set; }
    public required string ConditionText { get; set; }
    public required string ActionText { get; set; }
    public required string Priority { get; set; }
}

public class CreateContingencyPlanResponse
{
    public required ContingencyPlanDto Plan { get; set; }
}
