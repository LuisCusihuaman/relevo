using Ardalis.Result;
using MediatR;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Messages;

public record GetHandoverMessagesQuery(string HandoverId) : IRequest<Result<IReadOnlyList<HandoverMessageRecord>>>;

