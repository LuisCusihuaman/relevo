using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Handovers;

public class ReadyHandoverEndpoint(ISetupService setupService) : Endpoint<ReadyHandoverRequest>
{
    public override void Configure()
    {
        Post("/handovers/{id}/ready");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(ReadyHandoverRequest req, CancellationToken ct)
    {
        var userId = "user_demo12345678901234567890123456"; // Dummy user
        var success = await setupService.ReadyHandoverAsync(req.Id, userId);

        if (success)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(new {}, 400, ct);
        }
    }
}

public class ReadyHandoverRequest
{
    public string Id { get; set; } = null!;
}
