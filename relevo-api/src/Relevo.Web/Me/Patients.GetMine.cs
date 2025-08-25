using FastEndpoints;
using Relevo.Web.Patients;

namespace Relevo.Web.Me;

public class GetMyPatients(Setup.SetupDataStore _dataStore)
  : Endpoint<GetMyPatientsRequest, GetMyPatientsResponse>
{
  public override void Configure()
  {
    Get("/me/patients");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetMyPatientsRequest req, CancellationToken ct)
  {
    string userId = "demo-user"; // placeholder until auth is wired
    var (patients, total) = _dataStore.GetMyPatients(userId, req.Page <= 0 ? 1 : req.Page, req.PageSize <= 0 ? 25 : req.PageSize);
    Response = new GetMyPatientsResponse
    {
      Patients = patients.ToList(),
      TotalCount = total,
      Page = req.Page,
      PageSize = req.PageSize
    };
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
  public List<PatientRecord> Patients { get; set; } = [];
  public int TotalCount { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
}


