using FastEndpoints;
using MediatR;
using Relevo.UseCases.Units.GetPatientsByUnit;
using Relevo.Core.Models;

namespace Relevo.Web.Units;

public class GetPatientsByUnit(IMediator _mediator)
  : Endpoint<GetPatientsByUnitRequest, GetPatientsByUnitResponse>
{
  public override void Configure()
  {
    Get("/units/{unitId}/patients");
  }

  public override async Task HandleAsync(GetPatientsByUnitRequest req, CancellationToken ct)
  {
    var query = new GetPatientsByUnitQuery(req.UnitId, req.Page <= 0 ? 1 : req.Page, req.PageSize <= 0 ? 25 : req.PageSize);
    var result = await _mediator.Send(query, ct);

    if (result.IsSuccess)
    {
      Response = new GetPatientsByUnitResponse
      {
        Patients = result.Value.Patients.ToList(),
        TotalCount = result.Value.TotalCount,
        Page = result.Value.Page,
        PageSize = result.Value.PageSize
      };
      await SendAsync(Response, cancellation: ct);
    }
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
