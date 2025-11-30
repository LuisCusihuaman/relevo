using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Relevo.UseCases.Handovers.UpdateSynthesis;

public record UpdateHandoverSynthesisCommand(
    string HandoverId,
    string? Content,
    string Status,
    string UserId
) : ICommand<Result>;

