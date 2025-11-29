using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetSituationAwareness;

public class GetHandoverSituationAwarenessHandler(IHandoverRepository _repository)
  : IQueryHandler<GetHandoverSituationAwarenessQuery, Result<HandoverSituationAwarenessRecord>>
{
  public async Task<Result<HandoverSituationAwarenessRecord>> Handle(GetHandoverSituationAwarenessQuery request, CancellationToken cancellationToken)
  {
    var sa = await _repository.GetSituationAwarenessAsync(request.HandoverId);

    if (sa == null)
    {
        return Result.NotFound();
    }

    return sa;
  }
}

