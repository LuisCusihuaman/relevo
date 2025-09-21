using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Handovers;

public class CompleteHandoverEndpoint(ISetupService setupService)
  : Endpoint<CompleteHandoverRequest, CompleteHandoverResponse>
{
  public override void Configure()
  {
    Post("/handovers/{HandoverId}/complete");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(CompleteHandoverRequest req, CancellationToken ct)
  {
    var userId = "user_demo12345678901234567890123456"; // Dummy user
    var success = await setupService.CompleteHandoverAsync(req.HandoverId, userId);

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
}

public class CompleteHandoverResponse
{
  public bool Success { get; set; }
  public string HandoverId { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
}
