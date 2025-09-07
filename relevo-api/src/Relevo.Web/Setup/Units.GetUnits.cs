using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Setup;

public class GetUnits(ISetupService _setupService) : EndpointWithoutRequest<UnitListResponse>
{
  public override void Configure()
  {
    Get("/setup/units");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var units = await _setupService.GetUnitsAsync();
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


