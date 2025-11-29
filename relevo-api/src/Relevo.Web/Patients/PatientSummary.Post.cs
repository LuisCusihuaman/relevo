using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Patients;

public class CreatePatientSummary(
    IShiftCheckInService _shiftCheckInService,
    IUserContext _userContext,
    ILogger<CreatePatientSummary> _logger)
  : Endpoint<CreatePatientSummaryRequest, CreatePatientSummaryResponse>
{
  public override void Configure()
  {
    Post("/patients/{patientId}/summary");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(CreatePatientSummaryRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    _logger.LogInformation("CreatePatientSummary - Patient ID: {PatientId}, User ID: {UserId}", req.PatientId, user.Id);

    var summary = await _shiftCheckInService.CreatePatientSummaryAsync(
        req.PatientId,
        user.Id, // Use current user as the physician
        req.SummaryText,
        user.Id // Use current user as the creator
    );

    Response = new CreatePatientSummaryResponse
    {
        Summary = new PatientSummaryDto
        {
            Id = summary.Id,
            PatientId = summary.PatientId,
            PhysicianId = summary.PhysicianId,
            SummaryText = summary.SummaryText,
            CreatedAt = summary.CreatedAt,
            UpdatedAt = summary.UpdatedAt,
            LastEditedBy = summary.LastEditedBy
        }
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class CreatePatientSummaryRequest
{
    public required string PatientId { get; set; }
    public required string SummaryText { get; set; }
}

public class CreatePatientSummaryResponse
{
    public required PatientSummaryDto Summary { get; set; }
}
