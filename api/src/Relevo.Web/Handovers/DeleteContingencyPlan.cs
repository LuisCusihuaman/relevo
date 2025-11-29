using FastEndpoints;
using MediatR;
using Relevo.UseCases.Handovers.DeleteContingencyPlan;

namespace Relevo.Web.Handovers;

public class DeleteContingencyPlan(IMediator _mediator)
  : Endpoint<DeleteContingencyPlanRequest, DeleteContingencyPlanResponse>
{
  public override void Configure()
  {
    Delete("/handovers/{handoverId}/contingency-plans/{contingencyId}");
    AllowAnonymous();
  }

  public override async Task HandleAsync(DeleteContingencyPlanRequest req, CancellationToken ct)
  {
    var result = await _mediator.Send(new DeleteContingencyPlanCommand(req.HandoverId, req.ContingencyId), ct);

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
        await SendNotFoundAsync(ct);
        return;
    }

    if (result.IsSuccess)
    {
        Response = new DeleteContingencyPlanResponse
        {
            Success = true,
            Message = "Contingency plan deleted successfully"
        };
        await SendAsync(Response, cancellation: ct);
    }
  }
}

public class DeleteContingencyPlanRequest
{
    public string HandoverId { get; set; } = string.Empty;
    public string ContingencyId { get; set; } = string.Empty;
}

public class DeleteContingencyPlanResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

