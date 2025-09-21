using FastEndpoints;
using Relevo.Web.Setup;

namespace Relevo.Web.Handovers;

public class CompleteHandoverEndpoint(ISetupDataProvider _dataProvider)
  : Endpoint<CompleteHandoverRequest, CompleteHandoverResponse>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/complete");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(CompleteHandoverRequest req, CancellationToken ct)
  {
    var success = await _dataProvider.CompleteHandoverAsync(req.HandoverId, req.UserId);

    if (!success)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    Response = new CompleteHandoverResponse
    {
      Success = true,
      HandoverId = req.HandoverId,
      Message = "Handover completed successfully"
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class CompleteHandoverRequest
{
  public string HandoverId { get; set; } = string.Empty;
  public string UserId { get; set; } = string.Empty;
}

public class CompleteHandoverResponse
{
  public bool Success { get; set; }
  public string HandoverId { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
}
