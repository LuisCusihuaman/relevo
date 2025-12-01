using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.StateMachine;

namespace Relevo.Web.Handovers;

public class StartHandover(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<StartHandoverRequest>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/start");
  }

  public override async Task HandleAsync(StartHandoverRequest req, CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }
    
    var result = await _mediator.Send(new StartHandoverCommand(req.HandoverId, userId), ct);

    if (result.IsSuccess) 
      await SendOkAsync(ct);
    else
    {
      AddError(result.Errors.FirstOrDefault() ?? "Cannot start handover: state machine constraint violated.");
      await SendErrorsAsync(statusCode: 400, ct);
    }
  }
}

public class StartHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
}

