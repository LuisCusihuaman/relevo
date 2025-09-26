using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Handovers;

public class GetSynthesis(
    ISetupService _setupService,
    IUserContext _userContext,
    ILogger<GetSynthesis> _logger)
  : Endpoint<GetSynthesisRequest, GetSynthesisResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}/synthesis");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetSynthesisRequest req, CancellationToken ct)
  {
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("GetSynthesis - Handover ID: {HandoverId}, User ID: {UserId}", req.HandoverId, user.Id);

    var synthesis = await _setupService.GetSynthesisAsync(req.HandoverId);

    if (synthesis == null)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    Response = new GetSynthesisResponse
    {
        Synthesis = new SynthesisDto
        {
            HandoverId = synthesis.HandoverId,
            Content = synthesis.Content,
            Status = synthesis.Status,
            LastEditedBy = synthesis.LastEditedBy,
            UpdatedAt = synthesis.UpdatedAt
        }
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class GetSynthesisRequest
{
    public required string HandoverId { get; set; }
}

public class GetSynthesisResponse
{
    public SynthesisDto? Synthesis { get; set; }
}

public class SynthesisDto
{
    public string HandoverId { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LastEditedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
