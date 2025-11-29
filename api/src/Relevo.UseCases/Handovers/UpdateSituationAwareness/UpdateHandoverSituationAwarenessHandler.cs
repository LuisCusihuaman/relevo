using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Handovers.UpdateSituationAwareness;

public class UpdateHandoverSituationAwarenessHandler(IHandoverRepository _repository)
  : ICommandHandler<UpdateHandoverSituationAwarenessCommand, Result>
{
  public async Task<Result> Handle(UpdateHandoverSituationAwarenessCommand request, CancellationToken cancellationToken)
  {
    var success = await _repository.UpdateSituationAwarenessAsync(
        request.HandoverId,
        request.Content,
        request.Status,
        request.UserId
    );

    if (!success)
    {
        return Result.Error("Failed to update situation awareness. Handover may not exist.");
    }

    return Result.Success();
  }
}

