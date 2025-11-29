using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Handovers;

public class GetPatientData(
    IShiftCheckInService _shiftCheckInService,
    IUserContext _userContext,
    ILogger<GetPatientData> _logger)
  : Endpoint<GetPatientDataRequest, GetPatientDataResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}/patient-data");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(GetPatientDataRequest req, CancellationToken ct)
  {
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("GetPatientData - Handover ID: {HandoverId}, User ID: {UserId}", req.HandoverId, user.Id);

    var patientData = await _shiftCheckInService.GetPatientDataAsync(req.HandoverId);

    if (patientData == null)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    Response = new GetPatientDataResponse
    {
        PatientData = new PatientDataDto
        {
            HandoverId = patientData.HandoverId,
            IllnessSeverity = patientData.IllnessSeverity,
            SummaryText = patientData.SummaryText,
            LastEditedBy = patientData.LastEditedBy,
            UpdatedAt = patientData.UpdatedAt
        }
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class GetPatientDataRequest
{
    public required string HandoverId { get; set; }
}

public class GetPatientDataResponse
{
    public PatientDataDto? PatientData { get; set; }
}

public class PatientDataDto
{
    public string HandoverId { get; set; } = string.Empty;
    public string? IllnessSeverity { get; set; }
    public string? SummaryText { get; set; }
    public string? LastEditedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
