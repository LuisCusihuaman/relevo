using FastEndpoints;
using MediatR;
using Relevo.UseCases.ShiftCheckIn.GetShifts;
using Relevo.Core.Models;

namespace Relevo.Web.ShiftCheckIn;

public class GetShifts(IMediator _mediator)
  : EndpointWithoutRequest<ShiftListResponse>
{
  public override void Configure()
  {
    Get("/shift-check-in/shifts");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var result = await _mediator.Send(new GetShiftsQuery(), ct);

    if (result.IsSuccess)
    {
      Response = new ShiftListResponse
      {
        Shifts = result.Value.Shifts.ToList()
      };
      await SendAsync(Response, cancellation: ct);
    }
  }
}

public class ShiftListResponse
{
  public List<ShiftRecord> Shifts { get; set; } = [];
}

