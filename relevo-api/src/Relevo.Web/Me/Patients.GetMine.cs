using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Patients;
using DomainPatientRecord = Relevo.Core.Interfaces.PatientRecord;
using Microsoft.Extensions.Logging;

namespace Relevo.Web.Me;

public class GetMyPatients(
    ISetupService _setupService,
    IUserContext _userContext,
    ILogger<GetMyPatients> _logger)
  : Endpoint<GetMyPatientsRequest, GetMyPatientsResponse>
{
  public override void Configure()
  {
    Get("/me/patients");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(GetMyPatientsRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    // Debug logging
    _logger.LogInformation("GetMyPatients - User ID: {UserId}, Email: {Email}", user.Id, user.Email);

    var (patients, total) = await _setupService.GetMyPatientsAsync(user.Id, req.Page <= 0 ? 1 : req.Page, req.PageSize <= 0 ? 25 : req.PageSize);

    // Debug logging
    _logger.LogInformation("GetMyPatients - Found {PatientCount} patients, Total: {Total}", patients.Count, total);

    Response = new GetMyPatientsResponse
    {
      Patients = patients.ToList(),
      TotalCount = total,
      Page = req.Page,
      PageSize = req.PageSize
    };

    // Add debug info to response headers
    HttpContext.Response.Headers["X-Debug-UserId"] = user.Id;
    HttpContext.Response.Headers["X-Debug-PatientCount"] = patients.Count.ToString();
    HttpContext.Response.Headers["X-Debug-TotalCount"] = total.ToString();

    await SendAsync(Response, cancellation: ct);
  }
}

public class GetMyPatientsRequest
{
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 25;
}

public class GetMyPatientsResponse
{
  public List<DomainPatientRecord> Patients { get; set; } = [];
  public int TotalCount { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
}


