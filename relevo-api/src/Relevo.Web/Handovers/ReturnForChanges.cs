using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.ReturnForChanges;

namespace Relevo.Web.Handovers;

public class ReturnForChanges(IMediator _mediator, ICurrentUser _currentUser)
  : Endpoint<ReturnForChangesRequest>
{
  public override void Configure()
  {
    Post("/handovers/{handoverId}/return-for-changes");
  }

  public override async Task HandleAsync(ReturnForChangesRequest req, CancellationToken ct)
  {
    var userId = _currentUser.Id;
    if (string.IsNullOrEmpty(userId)) { await SendUnauthorizedAsync(ct); return; }

    var result = await _mediator.Send(new ReturnForChangesCommand(req.HandoverId, userId), ct);

    if (result.IsSuccess)
    {
      await SendOkAsync(ct);
    }
    else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
    }
    else
    {
      var errorMessage = result.Errors.FirstOrDefault() ?? "Failed to return handover for changes";
      AddError(errorMessage);
      await SendErrorsAsync(statusCode: 400, ct);
    }
  }
}

public class ReturnForChangesRequest
{
  public string HandoverId { get; set; } = string.Empty;
}

