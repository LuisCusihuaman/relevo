using FastEndpoints;
using Relevo.Web.Setup;

namespace Relevo.Web.Handovers;

public class GetPendingHandoversEndpoint(ISetupDataProvider _dataProvider)
  : Endpoint<GetPendingHandoversRequest, GetPendingHandoversResponse>
{
  public override void Configure()
  {
    Get("/handovers/pending");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(GetPendingHandoversRequest req, CancellationToken ct)
  {
    var handovers = await _dataProvider.GetPendingHandoversForUserAsync(req.UserId);

    Response = new GetPendingHandoversResponse
    {
      Handovers = handovers.Select(h => new HandoverSummaryDto
      {
        Id = h.Id,
        PatientId = h.PatientId,
        PatientName = h.PatientName,
        Status = h.Status,
        IllnessSeverity = h.IllnessSeverity.Severity,
        ShiftName = h.ShiftName,
        CreatedAt = h.CreatedAt,
        CreatedBy = h.CreatedBy
      }).ToList()
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class GetPendingHandoversRequest
{
  public string UserId { get; set; } = string.Empty;
}

public class GetPendingHandoversResponse
{
  public List<HandoverSummaryDto> Handovers { get; set; } = [];
}

public class HandoverSummaryDto
{
  public string Id { get; set; } = string.Empty;
  public string PatientId { get; set; } = string.Empty;
  public string? PatientName { get; set; }
  public string Status { get; set; } = string.Empty;
  public string IllnessSeverity { get; set; } = string.Empty;
  public string ShiftName { get; set; } = string.Empty;
  public string? CreatedAt { get; set; }
  public string CreatedBy { get; set; } = string.Empty;
}
