using Ardalis.Result;
using MediatR;

namespace Relevo.UseCases.Me.Checklists;

public record UpdateChecklistItemCommand(
    string HandoverId,
    string ItemId,
    bool IsChecked,
    string UserId
) : IRequest<Result<bool>>;

