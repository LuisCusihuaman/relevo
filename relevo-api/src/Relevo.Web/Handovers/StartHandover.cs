using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Handovers;

public class StartHandoverEndpoint(ISetupService setupService)
    : Endpoint<StartHandoverRequest, bool>
{
    public override void Configure()
    {
        Post("/handovers/{HandoverId}/start");
        AllowAnonymous(); // For now, let middleware handle auth
    }

    public override async Task HandleAsync(StartHandoverRequest req, CancellationToken ct)
    {
        // For now, we'll pass a "dummy" userId until auth is fully implemented
        var userId = "user_demo12345678901234567890123456"; // Dummy user
        var result = await setupService.StartHandoverAsync(req.HandoverId, userId);
        await SendAsync(result, cancellation: ct);
    }
}

public class StartHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
}
