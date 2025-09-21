using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Handovers;

public class ReadyHandoverEndpoint(ISetupService setupService)
    : Endpoint<ReadyHandoverRequest, bool>
{
    public override void Configure()
    {
        Post("/handovers/{HandoverId}/ready");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ReadyHandoverRequest req, CancellationToken ct)
    {
        var userId = "user_demo1_2_3_4_5_6_7_8_9_0_1_2_3_4_5_6"; // Dummy user
        var result = await setupService.ReadyHandoverAsync(req.HandoverId, userId);
        await SendAsync(result, cancellation: ct);
    }
}

public class ReadyHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
}
