using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Handovers;

public class ReadyHandoverEndpoint(IShiftCheckInService shiftCheckInService) : Endpoint<ReadyHandoverRequest>
{
    public override void Configure()
    {
        Post("/handovers/{id}/ready");
        AllowAnonymous(); // Let our custom middleware handle authentication
    }

    public override async Task HandleAsync(ReadyHandoverRequest req, CancellationToken ct)
    {
        try
        {
            var userId = "user_demo12345678901234567890123456"; // Dummy user
            
            bool success;
            if (req.Version.HasValue)
            {
                success = await shiftCheckInService.ReadyHandoverAsync(req.Id, userId, req.Version.Value);
            }
            else
            {
                success = await shiftCheckInService.ReadyHandoverAsync(req.Id, userId);
            }

            if (!success)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(ct);
        }
        catch (Relevo.Core.Exceptions.OptimisticLockException ex)
        {
            await SendAsync(new { success = false, id = req.Id, message = ex.Message }, 409, ct);
        }
    }
}

public class ReadyHandoverRequest
{
    public string Id { get; set; } = null!;
    public int? Version { get; set; }
}
