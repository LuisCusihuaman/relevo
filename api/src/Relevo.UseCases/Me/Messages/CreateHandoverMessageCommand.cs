using Ardalis.Result;
using MediatR;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Messages;

public record CreateHandoverMessageCommand(
    string HandoverId,
    string UserId,
    string UserName,
    string MessageText,
    string MessageType
) : IRequest<Result<HandoverMessageRecord>>;

