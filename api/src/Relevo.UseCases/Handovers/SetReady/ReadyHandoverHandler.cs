using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Handovers.SetReady;

public class ReadyHandoverHandler(IHandoverRepository _repository)
  : ICommandHandler<ReadyHandoverCommand, Result>
{
  public async Task<Result> Handle(ReadyHandoverCommand request, CancellationToken cancellationToken)
  {
    var success = await _repository.MarkAsReadyAsync(request.HandoverId, request.UserId);

    if (!success)
    {
        return Result.Error("Failed to mark handover as ready. It may not exist or is in an invalid state.");
    }

    return Result.Success();
  }
}

