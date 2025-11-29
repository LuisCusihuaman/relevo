using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Patients;
using Relevo.Web.Models;
using DomainPatientRecord = Relevo.Core.Interfaces.PatientRecord;

namespace Relevo.Web.Patients;

public class GetAllPatients(IShiftCheckInService _shiftCheckInService)
  : Endpoint<GetAllPatientsRequest, GetAllPatientsResponse>
{
  public override void Configure()
  {
    Get("/patients");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetAllPatientsRequest req, CancellationToken ct)
  {
    var (patients, total) = await _shiftCheckInService.GetAllPatientsAsync(req.Page <= 0 ? 1 : req.Page, req.PageSize <= 0 ? 25 : req.PageSize);
    Response = new GetAllPatientsResponse
    {
      Items = patients.Select(p => new PatientSummaryCard
      {
          Id = p.Id,
          Name = p.Name,
          HandoverStatus = p.HandoverStatus,
          HandoverId = p.HandoverId
      }).ToList(),
      Pagination = new PaginationInfo
      {
        TotalItems = total,
        Page = req.Page <= 0 ? 1 : req.Page,
        PageSize = req.PageSize <= 0 ? 25 : req.PageSize,
        TotalPages = (int)Math.Ceiling((double)total / (req.PageSize <= 0 ? 25 : req.PageSize))
      }
    };
    await SendAsync(Response, cancellation: ct);
  }
}

public class GetAllPatientsRequest
{
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 25;
}

public class GetAllPatientsResponse
{
  public List<PatientSummaryCard> Items { get; set; } = [];
  public PaginationInfo Pagination { get; set; } = new();
}

public class PatientSummaryCard
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HandoverStatus { get; set; } = "NotStarted"; // Default value
    public string? HandoverId { get; set; }
}
