using Ardalis.Result;
using MediatR;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.ActionItems;

public record CreateHandoverActionItemCommand(
    string HandoverId,
    string Description,
    string Priority,
    string? DueTime,
    string CreatedBy
) : IRequest<Result<HandoverActionItemFullRecord>>;

