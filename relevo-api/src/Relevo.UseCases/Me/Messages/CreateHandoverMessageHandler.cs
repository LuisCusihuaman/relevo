using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Messages;

public class CreateHandoverMessageHandler(IHandoverRepository _repository)
    : IRequestHandler<CreateHandoverMessageCommand, Result<HandoverMessageRecord>>
{
    public async Task<Result<HandoverMessageRecord>> Handle(
        CreateHandoverMessageCommand request,
        CancellationToken cancellationToken)
    {
        var message = await _repository.CreateMessageAsync(
            request.HandoverId,
            request.UserId,
            request.UserName,
            request.MessageText,
            request.MessageType);

        return Result.Success(message);
    }
}

