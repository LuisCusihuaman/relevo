using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Handovers.ReturnForChanges;

public class ReturnForChangesHandler(IHandoverRepository _repository)
  : ICommandHandler<ReturnForChangesCommand, Result>
{
  public async Task<Result> Handle(ReturnForChangesCommand request, CancellationToken cancellationToken)
  {
    var success = await _repository.ReturnForChangesAsync(request.HandoverId, request.UserId);

    if (!success)
    {
        return Result.Error("Failed to return handover for changes. It may not exist, is not in Ready state, or is already completed/cancelled.");
    }

    return Result.Success();
  }
}

