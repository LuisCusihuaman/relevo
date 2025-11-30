using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Checklists;

public class GetHandoverChecklistsHandler(IHandoverRepository _repository)
    : IRequestHandler<GetHandoverChecklistsQuery, Result<IReadOnlyList<HandoverChecklistRecord>>>
{
    public async Task<Result<IReadOnlyList<HandoverChecklistRecord>>> Handle(
        GetHandoverChecklistsQuery request,
        CancellationToken cancellationToken)
    {
        var checklists = await _repository.GetChecklistsAsync(request.HandoverId);
        return Result.Success(checklists);
    }
}

