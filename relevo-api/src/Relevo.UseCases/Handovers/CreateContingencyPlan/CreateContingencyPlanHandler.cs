using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.CreateContingencyPlan;

public class CreateContingencyPlanHandler(IHandoverRepository _repository)
  : ICommandHandler<CreateContingencyPlanCommand, Result<ContingencyPlanRecord>>
{
  public async Task<Result<ContingencyPlanRecord>> Handle(CreateContingencyPlanCommand request, CancellationToken cancellationToken)
  {
    var plan = await _repository.CreateContingencyPlanAsync(
        request.HandoverId,
        request.Condition,
        request.Action,
        request.Priority,
        request.UserId
    );

    return Result.Success(plan);
  }
}

