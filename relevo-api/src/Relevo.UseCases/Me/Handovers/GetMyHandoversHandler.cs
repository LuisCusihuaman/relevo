using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Handovers;

public class GetMyHandoversHandler(IHandoverRepository _repository)
    : IRequestHandler<GetMyHandoversQuery, Result<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)>>
{
    public async Task<Result<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)>> Handle(
        GetMyHandoversQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _repository.GetMyHandoversAsync(request.UserId, request.Page, request.PageSize);
        return Result.Success(result);
    }
}

