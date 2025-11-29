using Ardalis.Result;
using MediatR;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.ActionItems;

public record CreateHandoverActionItemCommand(
    string HandoverId,
    string Description,
    string Priority
) : IRequest<Result<HandoverActionItemFullRecord>>;

