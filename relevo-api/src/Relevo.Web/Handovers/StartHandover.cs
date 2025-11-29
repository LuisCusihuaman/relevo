using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Handovers;

public class StartHandoverEndpoint(IShiftCheckInService shiftCheckInService)
    : Endpoint<StartHandoverRequest, StartHandoverResponse>
{
    public override void Configure()
    {
        Post("/handovers/{HandoverId}/start");
        AllowAnonymous(); // For now, let middleware handle auth
    }

    public override async Task HandleAsync(StartHandoverRequest req, CancellationToken ct)
    {
        try
        {
            var userId = "user_demo12345678901234567890123456"; // Dummy user
            
            bool success;
            if (req.Version.HasValue)
            {
                success = await shiftCheckInService.StartHandoverAsync(req.HandoverId, userId, req.Version.Value);
            }
            else
            {
                success = await shiftCheckInService.StartHandoverAsync(req.HandoverId, userId);
            }

            if (!success)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            Response = new StartHandoverResponse { Success = true, HandoverId = req.HandoverId };
            await SendAsync(Response, cancellation: ct);
        }
        catch (Relevo.Core.Exceptions.OptimisticLockException ex)
        {
            await SendAsync(new StartHandoverResponse
            {
                Success = false,
                HandoverId = req.HandoverId,
                Message = ex.Message
            }, 409, ct);
        }
    }
}

public class StartHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public int? Version { get; set; }
}

public class StartHandoverResponse
{
    public bool Success { get; set; }
    public string HandoverId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
