using FastEndpoints;
using MediatR;
using Relevo.UseCases.Patients.GetAllPatients;
using Relevo.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Relevo.Web.Patients;

public class GetAllPatients(IMediator _mediator)
  : Endpoint<GetAllPatientsRequest, GetAllPatientsResponse>
{
  public override void Configure()
  {
    Get("/patients");
  }

  public override async Task HandleAsync(GetAllPatientsRequest req, CancellationToken ct)
  {
    var query = new GetAllPatientsQuery(req.Page <= 0 ? 1 : req.Page, req.PageSize <= 0 ? 25 : req.PageSize);
    var result = await _mediator.Send(query, ct);

    if (result.IsSuccess)
    {
      Response = new GetAllPatientsResponse
      {
        Items = result.Value.Patients.Select(p => new PatientSummaryCard
        {
            Id = p.Id,
            Name = p.Name,
            HandoverStatus = p.HandoverStatus,
            HandoverId = p.HandoverId,
            Unit = p.Unit,
            AssignedToName = p.AssignedToName,
            Severity = p.Severity
        }).ToList(),
        Pagination = new PaginationInfo
        {
          TotalItems = result.Value.TotalCount,
          Page = result.Value.Page,
          PageSize = result.Value.PageSize,
          TotalPages = (int)Math.Ceiling((double)result.Value.TotalCount / result.Value.PageSize)
        }
      };
      await SendAsync(Response, cancellation: ct);
    }
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
    [Required]
    public required string Id { get; set; }
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string HandoverStatus { get; set; }
    public string? HandoverId { get; set; }
    public string? Unit { get; set; }
    public string? AssignedToName { get; set; }
    public string? Severity { get; set; }
}

public class PaginationInfo
{
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

