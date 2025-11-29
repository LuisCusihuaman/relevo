using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Handovers;

public class CancelHandoverEndpoint(IShiftCheckInService shiftCheckInService)
    : Endpoint<CancelHandoverRequest, CancelHandoverResponse>
{
    public override void Configure()
    {
        Post("/handovers/{HandoverId}/cancel");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancelHandoverRequest req, CancellationToken ct)
    {
        try
        {
            var userId = "user_demo12345678901234567890123456";
            
            bool success;
            if (req.Version.HasValue)
            {
                success = await shiftCheckInService.CancelHandoverAsync(req.HandoverId, userId, req.Version.Value);
            }
            else
            {
                success = await shiftCheckInService.CancelHandoverAsync(req.HandoverId, userId);
            }

            if (!success)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            Response = new CancelHandoverResponse
            {
                Success = true,
                HandoverId = req.HandoverId,
                Message = "Handover cancelled successfully"
            };

            await SendAsync(Response, cancellation: ct);
        }
        catch (Relevo.Core.Exceptions.OptimisticLockException ex)
        {
            await SendAsync(new CancelHandoverResponse
            {
                Success = false,
                HandoverId = req.HandoverId,
                Message = ex.Message
            }, 409, ct);
        }
    }
}

public class CancelHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public int? Version { get; set; }
}

public class CancelHandoverResponse
{
    public bool Success { get; set; }
    public string HandoverId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
