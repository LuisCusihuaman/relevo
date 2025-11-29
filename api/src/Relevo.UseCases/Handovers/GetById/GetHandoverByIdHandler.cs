using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetById;

public class GetHandoverByIdHandler(IHandoverRepository _repository)
  : IQueryHandler<GetHandoverByIdQuery, Result<HandoverDetailRecord>>
{
  public async Task<Result<HandoverDetailRecord>> Handle(GetHandoverByIdQuery request, CancellationToken cancellationToken)
  {
    var handover = await _repository.GetHandoverByIdAsync(request.HandoverId);

    if (handover == null)
    {
      return Result.NotFound();
    }

    return handover;
  }
}

