using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Setup;
using Relevo.Web.Models;
using DomainHandoverRecord = Relevo.Core.Interfaces.HandoverRecord;

namespace Relevo.Web.Patients;

public class GetPatientHandoversEndpoint(
    ISetupService _setupService)
  : Endpoint<GetPatientHandoversRequest, GetPatientHandoversResponse>
{
  public override void Configure()
  {
    Get("/patients/{patientId}/handovers");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(GetPatientHandoversRequest req, CancellationToken ct)
  {
    var (handovers, total) = await _setupService.GetPatientHandoversAsync(req.PatientId, req.Page <= 0 ? 1 : req.Page, req.PageSize <= 0 ? 25 : req.PageSize);

    Response = new GetPatientHandoversResponse
    {
      Items = handovers.ToList(),
      Pagination = new PaginationInfo
      {
        TotalItems = total,
        CurrentPage = req.Page <= 0 ? 1 : req.Page,
        PageSize = req.PageSize <= 0 ? 25 : req.PageSize,
        TotalPages = (int)Math.Ceiling((double)total / (req.PageSize <= 0 ? 25 : req.PageSize))
      }
    };
    await SendAsync(Response, cancellation: ct);
  }
}

public class GetPatientHandoversRequest
{
  public string PatientId { get; set; } = string.Empty;
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 25;
}

public class GetPatientHandoversResponse
{
  public List<DomainHandoverRecord> Items { get; set; } = [];
  public PaginationInfo Pagination { get; set; } = new();
}
