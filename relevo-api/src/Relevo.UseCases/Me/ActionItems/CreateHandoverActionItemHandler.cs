using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.ActionItems;

public class CreateHandoverActionItemHandler(IHandoverRepository _repository)
    : IRequestHandler<CreateHandoverActionItemCommand, Result<HandoverActionItemFullRecord>>
{
    public async Task<Result<HandoverActionItemFullRecord>> Handle(
        CreateHandoverActionItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await _repository.CreateActionItemAsync(
            request.HandoverId,
            request.Description,
            request.Priority,
            request.DueTime,
            request.CreatedBy);

        return Result.Success(item);
    }
}

