using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Relevo.UseCases.Handovers.StateMachine;

public record StartHandoverCommand(string HandoverId, string UserId) : ICommand<Result>;
public record AcceptHandoverCommand(string HandoverId, string UserId) : ICommand<Result>;
public record RejectHandoverCommand(string HandoverId, string Reason, string UserId) : ICommand<Result>;
public record CancelHandoverCommand(string HandoverId, string UserId) : ICommand<Result>;
public record CompleteHandoverCommand(string HandoverId, string UserId) : ICommand<Result>;

