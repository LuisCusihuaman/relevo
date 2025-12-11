using FastEndpoints;
using MediatR;
using Relevo.UseCases.Patients.GetHandovers;
using Relevo.Core.Models;

namespace Relevo.Web.Patients;

public class GetPatientHandovers(IMediator _mediator)
  : Endpoint<GetPatientHandoversRequest, GetPatientHandoversResponse>
{
  public override void Configure()
  {
    Get("/patients/{patientId}/handovers");
  }

  public override async Task HandleAsync(GetPatientHandoversRequest req, CancellationToken ct)
  {
    Console.WriteLine($"[GetPatientHandovers] Request for PatientId: {req.PatientId}, Page: {req.Page}, PageSize: {req.PageSize}");
    
    var query = new GetPatientHandoversQuery(req.PatientId, req.Page <= 0 ? 1 : req.Page, req.PageSize <= 0 ? 25 : req.PageSize);
    var result = await _mediator.Send(query, ct);

    Console.WriteLine($"[GetPatientHandovers] Result IsSuccess: {result.IsSuccess}, Count: {(result.IsSuccess ? result.Value.Items.Count : 0)}");

    if (result.IsSuccess)
    {
      Response = new GetPatientHandoversResponse
      {
        Items = result.Value.Items.ToList(),
        Pagination = new PaginationInfo
        {
          TotalItems = result.Value.TotalCount,
          Page = result.Value.Page,
          PageSize = result.Value.PageSize,
          TotalPages = (int)Math.Ceiling((double)result.Value.TotalCount / result.Value.PageSize)
        }
      };
      
      Console.WriteLine($"[GetPatientHandovers] Returning {Response.Items.Count} handovers for patient {req.PatientId}");
      
      await SendAsync(Response, cancellation: ct);
    }
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
  public List<HandoverRecord> Items { get; set; } = [];
  public PaginationInfo Pagination { get; set; } = new();
}

