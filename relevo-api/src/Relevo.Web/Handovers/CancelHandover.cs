using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Handovers;

public class CancelHandoverEndpoint(ISetupService setupService)
    : Endpoint<CancelHandoverRequest, bool>
{
    public override void Configure()
    {
        Post("/handovers/{HandoverId}/cancel");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancelHandoverRequest req, CancellationToken ct)
    {
        var userId = "user_demo12345678901234567890123456"; // Dummy user
        var result = await setupService.CancelHandoverAsync(req.HandoverId, userId);
        await SendAsync(result, cancellation: ct);
    }
}

public class CancelHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
}
