using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetSynthesis;

public class GetHandoverSynthesisHandler(IHandoverRepository _repository)
  : IQueryHandler<GetHandoverSynthesisQuery, Result<HandoverSynthesisRecord>>
{
  public async Task<Result<HandoverSynthesisRecord>> Handle(GetHandoverSynthesisQuery request, CancellationToken cancellationToken)
  {
    var synthesis = await _repository.GetSynthesisAsync(request.HandoverId);

    if (synthesis == null)
    {
        return Result.NotFound();
    }

    return synthesis;
  }
}

