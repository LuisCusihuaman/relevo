using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Messages;

public class GetHandoverMessagesHandler(IHandoverRepository _repository)
    : IRequestHandler<GetHandoverMessagesQuery, Result<IReadOnlyList<HandoverMessageRecord>>>
{
    public async Task<Result<IReadOnlyList<HandoverMessageRecord>>> Handle(
        GetHandoverMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var messages = await _repository.GetMessagesAsync(request.HandoverId);
        return Result.Success(messages);
    }
}

