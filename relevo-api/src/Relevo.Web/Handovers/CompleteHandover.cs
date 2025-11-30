using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.StateMachine;

namespace Relevo.Web.Handovers;

public class CompleteHandover(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<CompleteHandoverRequest>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/complete");
  }

  public override async Task HandleAsync(CompleteHandoverRequest req, CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }
    
    var result = await _mediator.Send(new CompleteHandoverCommand(req.HandoverId, userId), ct);

    if (result.IsSuccess) 
      await SendOkAsync(ct);
    else
    {
      AddError("Cannot complete handover: state machine constraint violated. Handover must be accepted before completing.");
      await SendErrorsAsync(statusCode: 400, ct);
    }
  }
}

public class CompleteHandoverRequest
{
    public string HandoverId { get; set; } = string.Empty;
}

