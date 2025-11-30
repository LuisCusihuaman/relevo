using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Activity;

public class GetHandoverActivityHandler(IHandoverRepository _repository)
    : IRequestHandler<GetHandoverActivityQuery, Result<IReadOnlyList<HandoverActivityRecord>>>
{
    public async Task<Result<IReadOnlyList<HandoverActivityRecord>>> Handle(
        GetHandoverActivityQuery request,
        CancellationToken cancellationToken)
    {
        var activities = await _repository.GetActivityLogAsync(request.HandoverId);
        return Result.Success(activities);
    }
}

