using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.GetClinicalData;
using Relevo.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Relevo.Web.Handovers;

public class GetClinicalData(IMediator _mediator)
  : Endpoint<GetClinicalDataRequest, GetClinicalDataResponse>
{
  public override void Configure()
  {
    Get("/handovers/{handoverId}/patient-data");
  }

  public override async Task HandleAsync(GetClinicalDataRequest req, CancellationToken ct)
  {
    var result = await _mediator.Send(new GetHandoverClinicalDataQuery(req.HandoverId), ct);

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    if (result.IsSuccess)
    {
        var data = result.Value;
        Response = new GetClinicalDataResponse
        {
            HandoverId = data.HandoverId,
            IllnessSeverity = data.IllnessSeverity,
            SummaryText = data.SummaryText,
            LastEditedBy = data.LastEditedBy,
            Status = data.Status,
            UpdatedAt = data.UpdatedAt
        };
        await SendAsync(Response, cancellation: ct);
    }
  }
}

public class GetClinicalDataRequest
{
    public string HandoverId { get; set; } = string.Empty;
}

public class GetClinicalDataResponse
{
    [Required]
    public required string HandoverId { get; set; }
    [Required]
    public required string IllnessSeverity { get; set; }
    [Required]
    public required string SummaryText { get; set; }
    [Required]
    public required string LastEditedBy { get; set; }
    [Required]
    public required string Status { get; set; }
    [Required]
    public required DateTime UpdatedAt { get; set; }
}

