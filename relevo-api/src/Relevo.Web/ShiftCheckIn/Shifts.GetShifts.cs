using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.ShiftCheckIn;

public class GetShifts(IShiftCheckInService _shiftCheckInService) : EndpointWithoutRequest<ShiftListResponse>
{
  public override void Configure()
  {
    Get("/shift-check-in/shifts");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var shifts = await _shiftCheckInService.GetShiftsAsync();
    Response = new ShiftListResponse
    {
      Shifts = shifts.Select(s => new ShiftItem
      {
        Id = s.Id,
        Name = s.Name,
        StartTime = s.StartTime,
        EndTime = s.EndTime
      }).ToList()
    };
    await SendAsync(Response, cancellation: ct);
  }
}

public class ShiftListResponse
{
  public List<ShiftItem> Shifts { get; set; } = [];
}

public class ShiftItem
{
  public string Id { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string StartTime { get; set; } = string.Empty;
  public string EndTime { get; set; } = string.Empty;
}
