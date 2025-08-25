using FastEndpoints;
using Relevo.Web.Patients;

namespace Relevo.Web.Units;

public class GetPatientsByUnit(Relevo.Web.Setup.ISetupDataProvider _dataProvider)
  : Endpoint<GetPatientsByUnitRequest, GetPatientsByUnitResponse>
{
  public override void Configure()
  {
    Get("/units/{unitId}/patients");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetPatientsByUnitRequest req, CancellationToken ct)
  {
    var (patients, total) = _dataProvider.GetPatientsByUnit(req.UnitId, req.Page <= 0 ? 1 : req.Page, req.PageSize <= 0 ? 25 : req.PageSize);
    Response = new GetPatientsByUnitResponse
    {
      Patients = patients.ToList(),
      TotalCount = total,
      Page = req.Page,
      PageSize = req.PageSize
    };
    await SendAsync(Response, cancellation: ct);
  }
}

public class GetPatientsByUnitRequest
{
  public string UnitId { get; set; } = string.Empty;
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 25;
}

public class GetPatientsByUnitResponse
{
  public List<PatientRecord> Patients { get; set; } = [];
  public int TotalCount { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
}


