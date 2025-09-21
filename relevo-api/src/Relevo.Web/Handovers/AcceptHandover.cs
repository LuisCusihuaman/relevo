using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Handovers;

public class AcceptHandoverEndpoint(ISetupService setupService)
  : Endpoint<AcceptHandoverRequest, AcceptHandoverResponse>
{
  public override void Configure()
  {
    Post("/handovers/{HandoverId}/accept");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(AcceptHandoverRequest req, CancellationToken ct)
  {
    var userId = "user_demo12345678901234567890123456"; // Dummy user
    var success = await setupService.AcceptHandoverAsync(req.HandoverId, userId);

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
}

public class AcceptHandoverResponse
{
  public bool Success { get; set; }
  public string HandoverId { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
}
