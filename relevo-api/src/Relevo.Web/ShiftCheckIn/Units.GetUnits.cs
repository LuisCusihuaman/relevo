using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.ShiftCheckIn;

public class GetUnits(IShiftCheckInService _shiftCheckInService) : EndpointWithoutRequest<UnitListResponse>
{
  public override void Configure()
  {
    Get("/shift-check-in/units");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var units = await _shiftCheckInService.GetUnitsAsync();
    Response = new UnitListResponse
    {
      Units = units.Select(u => new UnitItem { Id = u.Id, Name = u.Name }).ToList()
    };
    await SendAsync(Response, cancellation: ct);
  }
}

public class UnitListResponse
{
  public List<UnitItem> Units { get; set; } = [];
}

public class UnitItem
{
  public string Id { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
}
