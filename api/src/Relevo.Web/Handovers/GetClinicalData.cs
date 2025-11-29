using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.GetClinicalData;
using Relevo.Core.Models;

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
    public string HandoverId { get; set; } = string.Empty;
    public string IllnessSeverity { get; set; } = string.Empty;
    public string SummaryText { get; set; } = string.Empty;
    public string LastEditedBy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

