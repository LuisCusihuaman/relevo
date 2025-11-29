using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Handovers.UpdateSynthesis;

public class UpdateHandoverSynthesisHandler(IHandoverRepository _repository)
  : ICommandHandler<UpdateHandoverSynthesisCommand, Result>
{
  public async Task<Result> Handle(UpdateHandoverSynthesisCommand request, CancellationToken cancellationToken)
  {
    var success = await _repository.UpdateSynthesisAsync(
        request.HandoverId,
        request.Content,
        request.Status,
        request.UserId
    );

    if (!success)
    {
        return Result.NotFound();
    }

    return Result.Success();
  }
}

