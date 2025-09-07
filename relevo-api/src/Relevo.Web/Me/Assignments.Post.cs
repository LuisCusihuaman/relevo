using FastEndpoints;
using Relevo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Me;

public class PostAssignments(
    ISetupService _setupService,
    IUserContext _userContext,
    ILogger<PostAssignments> _logger)
  : Endpoint<PostAssignmentsRequest>
{
  public override void Configure()
  {
    Post("/me/assignments");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(PostAssignmentsRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    // Debug logging - Enhanced to show more user details
    _logger.LogInformation("🚀 AssignPatients - User: {UserId} ({UserEmail}), Shift: {ShiftId}, Patients: {PatientIds}",
        user.Id, user.Email ?? "no-email", req.ShiftId, string.Join(",", req.PatientIds ?? []));
    _logger.LogInformation("👤 User Context Details - ID: {UserId}, Email: {Email}, Name: {FirstName} {LastName}",
        user.Id, user.Email ?? "no-email", user.FirstName ?? "no-first", user.LastName ?? "no-last");

    await _setupService.AssignPatientsAsync(user.Id, req.ShiftId, req.PatientIds ?? []);

    // Add debug info to response headers
    HttpContext.Response.Headers["X-Debug-UserId"] = user.Id;
    HttpContext.Response.Headers["X-Debug-ShiftId"] = req.ShiftId;
    HttpContext.Response.Headers["X-Debug-PatientIds"] = string.Join(",", req.PatientIds ?? []);

    await SendNoContentAsync(ct);
  }
}

public class PostAssignmentsRequest
{
  public string ShiftId { get; set; } = string.Empty;
  public List<string>? PatientIds { get; set; }
}


