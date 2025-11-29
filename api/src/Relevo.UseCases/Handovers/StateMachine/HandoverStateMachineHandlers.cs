using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Handovers.StateMachine;

public class HandoverStateMachineHandlers(IHandoverRepository _repository) :
    ICommandHandler<StartHandoverCommand, Result>,
    ICommandHandler<AcceptHandoverCommand, Result>,
    ICommandHandler<RejectHandoverCommand, Result>,
    ICommandHandler<CancelHandoverCommand, Result>,
    ICommandHandler<CompleteHandoverCommand, Result>
{
    public async Task<Result> Handle(StartHandoverCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteStateChange(request.HandoverId, request.UserId, (id, uid) => _repository.StartHandoverAsync(id, uid));
    }

    public async Task<Result> Handle(AcceptHandoverCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteStateChange(request.HandoverId, request.UserId, (id, uid) => _repository.AcceptHandoverAsync(id, uid));
    }

    public async Task<Result> Handle(RejectHandoverCommand request, CancellationToken cancellationToken)
    {
        var success = await _repository.RejectHandoverAsync(request.HandoverId, request.Reason, request.UserId);
        return success ? Result.Success() : Result.Error("Failed to reject handover.");
    }

    public async Task<Result> Handle(CancelHandoverCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteStateChange(request.HandoverId, request.UserId, (id, uid) => _repository.CancelHandoverAsync(id, uid));
    }

    public async Task<Result> Handle(CompleteHandoverCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteStateChange(request.HandoverId, request.UserId, (id, uid) => _repository.CompleteHandoverAsync(id, uid));
    }

    private async Task<Result> ExecuteStateChange(string handoverId, string userId, Func<string, string, Task<bool>> action)
    {
        var success = await action(handoverId, userId);
        return success ? Result.Success() : Result.Error("Failed to update handover state.");
    }
}

