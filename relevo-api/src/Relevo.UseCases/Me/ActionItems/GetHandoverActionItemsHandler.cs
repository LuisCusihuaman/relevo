using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.ActionItems;

public class GetHandoverActionItemsHandler(IHandoverRepository _repository)
    : IRequestHandler<GetHandoverActionItemsQuery, Result<IReadOnlyList<HandoverActionItemFullRecord>>>
{
    public async Task<Result<IReadOnlyList<HandoverActionItemFullRecord>>> Handle(
        GetHandoverActionItemsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetActionItemsAsync(request.HandoverId);
        return Result.Success(items);
    }
}

