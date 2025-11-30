using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Me.ActionItems;

public class UpdateHandoverActionItemHandler(IHandoverRepository _repository)
    : IRequestHandler<UpdateHandoverActionItemCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UpdateHandoverActionItemCommand request,
        CancellationToken cancellationToken)
    {
        var success = await _repository.UpdateActionItemAsync(
            request.HandoverId,
            request.ItemId,
            request.IsCompleted);

        return success
            ? Result.Success(true)
            : Result.NotFound("Action item not found");
    }
}

