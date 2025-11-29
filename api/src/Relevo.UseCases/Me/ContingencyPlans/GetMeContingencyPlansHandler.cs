using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.ContingencyPlans;

public class GetMeContingencyPlansHandler(IHandoverRepository _repository)
    : IRequestHandler<GetMeContingencyPlansQuery, Result<IReadOnlyList<ContingencyPlanRecord>>>
{
    public async Task<Result<IReadOnlyList<ContingencyPlanRecord>>> Handle(
        GetMeContingencyPlansQuery request,
        CancellationToken cancellationToken)
    {
        var plans = await _repository.GetContingencyPlansAsync(request.HandoverId);
        return Result.Success(plans);
    }
}

