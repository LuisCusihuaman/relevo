using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetPending;

public class GetPendingHandoversHandler(IHandoverRepository _repository)
  : IQueryHandler<GetPendingHandoversQuery, Result<IReadOnlyList<HandoverRecord>>>
{
  public async Task<Result<IReadOnlyList<HandoverRecord>>> Handle(GetPendingHandoversQuery request, CancellationToken cancellationToken)
  {
    var pending = await _repository.GetPendingHandoversAsync(request.UserId);
    return Result.Success(pending);
  }
}

