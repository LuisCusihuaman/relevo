using FastEndpoints;
using Relevo.Web.Setup;

namespace Relevo.Web.Handovers;

public class AcceptHandoverEndpoint(OracleSetupDataProvider _dataProvider)
  : Endpoint<AcceptHandoverRequest, AcceptHandoverResponse>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/accept");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(AcceptHandoverRequest req, CancellationToken ct)
  {
    var success = await _dataProvider.AcceptHandoverAsync(req.HandoverId, req.UserId);

    if (!success)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    Response = new AcceptHandoverResponse
    {
      Success = true,
      HandoverId = req.HandoverId,
      Message = "Handover accepted successfully"
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class AcceptHandoverRequest
{
  public string HandoverId { get; set; } = string.Empty;
  public string UserId { get; set; } = string.Empty;
}

public class AcceptHandoverResponse
{
  public bool Success { get; set; }
  public string HandoverId { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
}
