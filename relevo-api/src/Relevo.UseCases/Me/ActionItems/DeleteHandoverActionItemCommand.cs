using Ardalis.Result;
using MediatR;

namespace Relevo.UseCases.Me.ActionItems;

public record DeleteHandoverActionItemCommand(
    string HandoverId,
    string ItemId
) : IRequest<Result<bool>>;

