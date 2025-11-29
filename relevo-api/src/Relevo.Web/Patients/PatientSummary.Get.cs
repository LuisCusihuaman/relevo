using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Patients;

public class GetPatientSummary(
    IShiftCheckInService _shiftCheckInService,
    IUserContext _userContext,
    ILogger<GetPatientSummary> _logger)
  : Endpoint<GetPatientSummaryRequest, GetPatientSummaryResponse>
{
  public override void Configure()
  {
    Get("/patients/{patientId}/summary");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(GetPatientSummaryRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("GetPatientSummary - Patient ID: {PatientId}, User ID: {UserId}", req.PatientId, user.Id);

    var summary = await _shiftCheckInService.GetPatientSummaryAsync(req.PatientId);

    Response = new GetPatientSummaryResponse
    {
        Summary = summary != null ? new PatientSummaryDto
        {
            Id = summary.Id,
            PatientId = summary.PatientId,
            PhysicianId = summary.PhysicianId,
            SummaryText = summary.SummaryText,
            CreatedAt = summary.CreatedAt,
            UpdatedAt = summary.UpdatedAt,
            LastEditedBy = summary.LastEditedBy
        } : null
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class GetPatientSummaryRequest
{
    public required string PatientId { get; set; }
}

public class GetPatientSummaryResponse
{
    public PatientSummaryDto? Summary { get; set; }
}

public class PatientSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string PhysicianId { get; set; } = string.Empty;
    public string SummaryText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string LastEditedBy { get; set; } = string.Empty;
}
