using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Handovers;

public class GetContingencyPlans(
    ISetupService _setupService,
    IUserContext _userContext,
    ILogger<GetContingencyPlans> _logger)
  : Endpoint<GetContingencyPlansRequest, GetContingencyPlansResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}/contingency-plans");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(GetContingencyPlansRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("GetContingencyPlans - Handover ID: {HandoverId}, User ID: {UserId}", req.HandoverId, user.Id);

    var plans = await _setupService.GetHandoverContingencyPlansAsync(req.HandoverId);

    Response = new GetContingencyPlansResponse
    {
        Plans = plans.Select(p => new ContingencyPlanDto
        {
            Id = p.Id,
            HandoverId = p.HandoverId,
            ConditionText = p.ConditionText,
            ActionText = p.ActionText,
            Priority = p.Priority,
            Status = p.Status,
            CreatedBy = p.CreatedBy,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList()
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class GetContingencyPlansRequest
{
    public required string HandoverId { get; set; }
}

public class GetContingencyPlansResponse
{
    public List<ContingencyPlanDto> Plans { get; set; } = [];
}

public class ContingencyPlanDto
{
    public string Id { get; set; } = string.Empty;
    public string HandoverId { get; set; } = string.Empty;
    public string ConditionText { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
