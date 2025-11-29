using FastEndpoints;
using MediatR;
using Relevo.UseCases.ShiftCheckIn.GetUnits;
using Relevo.Core.Models;

namespace Relevo.Web.ShiftCheckIn;

public class GetUnits(IMediator _mediator)
  : EndpointWithoutRequest<UnitListResponse>
{
  public override void Configure()
  {
    Get("/shift-check-in/units");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var result = await _mediator.Send(new GetUnitsQuery(), ct);

    if (result.IsSuccess)
    {
      Response = new UnitListResponse
      {
        Units = result.Value.Units.ToList()
      };
      await SendAsync(Response, cancellation: ct);
    }
  }
}

public class UnitListResponse
{
  public List<UnitRecord> Units { get; set; } = [];
}

