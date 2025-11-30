using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Handovers.DeleteContingencyPlan;

public class DeleteContingencyPlanHandler(IHandoverRepository _repository)
  : ICommandHandler<DeleteContingencyPlanCommand, Result>
{
  public async Task<Result> Handle(DeleteContingencyPlanCommand request, CancellationToken cancellationToken)
  {
    var success = await _repository.DeleteContingencyPlanAsync(request.HandoverId, request.ContingencyId);

    if (!success)
    {
      return Result.NotFound();
    }

    return Result.Success();
  }
}

