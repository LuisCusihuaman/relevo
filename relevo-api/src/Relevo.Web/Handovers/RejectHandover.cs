using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Handovers;

public class RejectHandoverEndpoint(IShiftCheckInService shiftCheckInService)
    : Endpoint<RejectHandoverRequest, RejectHandoverResponse>
{
    public override void Configure()
    {
        Post("/handovers/{HandoverId}/reject");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RejectHandoverRequest req, CancellationToken ct)
    {
        try
        {
            var userId = "user_demo12345678901234567890123456";
            
            bool success;
            if (req.Version.HasValue)
            {
                success = await shiftCheckInService.RejectHandoverAsync(req.HandoverId, userId, req.Reason, req.Version.Value);
            }
            else
            {
                success = await shiftCheckInService.RejectHandoverAsync(req.HandoverId, userId, req.Reason);
            }

            if (!success)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            Response = new RejectHandoverResponse
            {
                Success = true,
                HandoverId = req.HandoverId,
                Message = "Handover rejected successfully"
            };

            await SendAsync(Response, cancellation: ct);
        }
        catch (Relevo.Core.Exceptions.OptimisticLockException ex)
        {
            await SendAsync(new RejectHandoverResponse
            {
                Success = false,
                HandoverId = req.HandoverId,
                Message = ex.Message
            }, 409, ct);
        }
    }
}

public class RejectHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int? Version { get; set; }
}

public class RejectHandoverResponse
{
    public bool Success { get; set; }
    public string HandoverId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
