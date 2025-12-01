using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Relevo.UseCases.Handovers.StateMachine;

public record StartHandoverCommand(string HandoverId, string UserId) : ICommand<Result>;
public record RejectHandoverCommand(string HandoverId, string Reason, string UserId) : ICommand<Result>;
public record CancelHandoverCommand(string HandoverId, string UserId, string? CancelReason = null) : ICommand<Result>; // V3: CancelReason required
public record CompleteHandoverCommand(string HandoverId, string UserId) : ICommand<Result>;

