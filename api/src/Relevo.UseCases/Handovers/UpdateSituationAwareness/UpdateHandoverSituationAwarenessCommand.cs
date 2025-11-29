using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Handovers.UpdateSituationAwareness;

public record UpdateHandoverSituationAwarenessCommand(string HandoverId, string? Content, string Status, string UserId) : ICommand<Result>;

