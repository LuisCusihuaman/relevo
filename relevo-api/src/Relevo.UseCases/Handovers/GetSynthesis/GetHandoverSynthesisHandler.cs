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
        // In legacy, it created a default if missing. 
        // Repository implementation should handle this or we handle it here.
        // Legacy repo implementation creates it. 
        // If our new repo implementation mimics legacy, it will return a record.
        // If not found (e.g. invalid handover ID), we return NotFound.
        return Result.NotFound();
    }

    return synthesis;
  }
}

