using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetContingencyPlans;

public class GetContingencyPlansHandler(IHandoverRepository _repository)
  : IQueryHandler<GetContingencyPlansQuery, Result<IReadOnlyList<ContingencyPlanRecord>>>
{
  public async Task<Result<IReadOnlyList<ContingencyPlanRecord>>> Handle(GetContingencyPlansQuery request, CancellationToken cancellationToken)
  {
    var plans = await _repository.GetContingencyPlansAsync(request.HandoverId);
    return Result.Success(plans);
  }
}

