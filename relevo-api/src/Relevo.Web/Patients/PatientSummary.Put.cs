using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Patients;

public class UpdatePatientSummary(
    IShiftCheckInService _shiftCheckInService,
    IUserContext _userContext,
    ILogger<UpdatePatientSummary> _logger)
  : Endpoint<UpdatePatientSummaryRequest, UpdatePatientSummaryResponse>
{
  public override void Configure()
  {
    Put("/patients/{patientId}/summary");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(UpdatePatientSummaryRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("UpdatePatientSummary - Patient ID: {PatientId}, User ID: {UserId}", req.PatientId, user.Id);

    // First get the existing summary to get its ID
    var existingSummary = await _shiftCheckInService.GetPatientSummaryAsync(req.PatientId);
    if (existingSummary == null)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    var success = await _shiftCheckInService.UpdatePatientSummaryAsync(
        existingSummary.Id,
        req.SummaryText,
        user.Id // Use current user as the last editor
    );

    if (!success)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    Response = new UpdatePatientSummaryResponse
    {
        Success = true,
        Message = "Patient summary updated successfully"
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class UpdatePatientSummaryRequest
{
    public required string PatientId { get; set; }
    public required string SummaryText { get; set; }
}

public class UpdatePatientSummaryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
