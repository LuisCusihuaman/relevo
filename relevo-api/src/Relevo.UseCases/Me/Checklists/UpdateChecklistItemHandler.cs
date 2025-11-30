using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Me.Checklists;

public class UpdateChecklistItemHandler(IHandoverRepository _repository)
    : IRequestHandler<UpdateChecklistItemCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UpdateChecklistItemCommand request,
        CancellationToken cancellationToken)
    {
        var success = await _repository.UpdateChecklistItemAsync(
            request.HandoverId,
            request.ItemId,
            request.IsChecked,
            request.UserId);

        return success ? Result.Success(true) : Result.NotFound("Checklist item not found");
    }
}

