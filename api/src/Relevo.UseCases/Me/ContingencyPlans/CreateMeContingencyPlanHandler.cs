using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.ContingencyPlans;

public class CreateMeContingencyPlanHandler(IHandoverRepository _repository)
    : IRequestHandler<CreateMeContingencyPlanCommand, Result<ContingencyPlanRecord>>
{
    public async Task<Result<ContingencyPlanRecord>> Handle(
        CreateMeContingencyPlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.CreateContingencyPlanAsync(
            request.HandoverId,
            request.ConditionText,
            request.ActionText,
            request.Priority,
            request.UserId);

        return Result.Success(plan);
    }
}

