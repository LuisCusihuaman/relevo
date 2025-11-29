using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Me.ActionItems;

public class DeleteHandoverActionItemHandler(IHandoverRepository _repository)
    : IRequestHandler<DeleteHandoverActionItemCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteHandoverActionItemCommand request,
        CancellationToken cancellationToken)
    {
        var success = await _repository.DeleteActionItemAsync(
            request.HandoverId,
            request.ItemId);

        return success
            ? Result.Success(true)
            : Result.NotFound("Action item not found");
    }
}

