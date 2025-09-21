using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Handovers;

public class RejectHandoverEndpoint(ISetupService setupService)
    : Endpoint<RejectHandoverRequest, bool>
{
    public override void Configure()
    {
        Post("/handovers/{HandoverId}/reject");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RejectHandoverRequest req, CancellationToken ct)
    {
        var userId = "user_demo12345678901234567890123456"; // Dummy user
        var result = await setupService.RejectHandoverAsync(req.HandoverId, userId, req.Reason);
        await SendAsync(result, cancellation: ct);
    }
}

public class RejectHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
