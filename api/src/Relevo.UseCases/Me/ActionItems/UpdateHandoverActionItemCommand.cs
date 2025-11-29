using Ardalis.Result;
using MediatR;

namespace Relevo.UseCases.Me.ActionItems;

public record UpdateHandoverActionItemCommand(
    string HandoverId,
    string ItemId,
    bool IsCompleted
) : IRequest<Result<bool>>;

