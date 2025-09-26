using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Handovers;

public class PutPatientData(
    ISetupService _setupService,
    IUserContext _userContext,
    ILogger<PutPatientData> _logger)
  : Endpoint<PutPatientDataRequest, ApiResponse>
{
  public override void Configure()
  {
    Put("/handovers/{handoverId}/patient-data");
    AllowAnonymous(); // Middleware handles auth
  }

  public override async Task HandleAsync(PutPatientDataRequest req, CancellationToken ct)
  {
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("PutPatientData - Handover ID: {HandoverId}, User ID: {UserId}", req.HandoverId, user.Id);

    var success = await _setupService.UpdatePatientDataAsync(req.HandoverId, req.IllnessSeverity, req.SummaryText, req.Status, user.Id);

    if (!success)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    await SendAsync(new ApiResponse { Success = true, Message = "Patient data updated successfully." }, cancellation: ct);
  }
}

public class PutPatientDataRequest
{
    public required string HandoverId { get; set; }
    public required string IllnessSeverity { get; set; }
    public string? SummaryText { get; set; }
    public required string Status { get; set; }
}
