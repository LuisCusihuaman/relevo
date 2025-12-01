using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Handovers.StateMachine;

public class HandoverStateMachineHandlers(IHandoverRepository _repository) :
    ICommandHandler<StartHandoverCommand, Result>,
    ICommandHandler<RejectHandoverCommand, Result>,
    ICommandHandler<CancelHandoverCommand, Result>,
    ICommandHandler<CompleteHandoverCommand, Result>
{
    public async Task<Result> Handle(StartHandoverCommand request, CancellationToken cancellationToken)
    {
        // V3 Regla #22: quien start debe tener coverage en TO shift, NO puede ser sender
        var hasCoverage = await _repository.HasCoverageInToShiftAsync(request.HandoverId, request.UserId);
        if (!hasCoverage)
        {
            return Result.Error("Cannot start handover: user must have coverage in the TO shift.");
        }

        // Get handover to verify user is not the sender
        var handover = await _repository.GetHandoverByIdAsync(request.HandoverId);
        if (handover?.Handover.SenderUserId == request.UserId)
        {
            return Result.Error("Cannot start handover: sender cannot start the handover.");
        }

        return await ExecuteStateChange(request.HandoverId, request.UserId, (id, uid) => _repository.StartHandoverAsync(id, uid));
    }

    public async Task<Result> Handle(RejectHandoverCommand request, CancellationToken cancellationToken)
    {
        // V3: Reject uses Cancel with CANCEL_REASON='ReceiverRefused'
        var cancelReason = string.IsNullOrEmpty(request.Reason) ? "ReceiverRefused" : request.Reason;
        var success = await _repository.CancelHandoverAsync(request.HandoverId, cancelReason, request.UserId);
        return success ? Result.Success() : Result.Error("Failed to reject handover.");
    }

    public async Task<Result> Handle(CancelHandoverCommand request, CancellationToken cancellationToken)
    {
        // V3: CancelHandoverAsync now requires cancelReason
        var cancelReason = request.CancelReason ?? "UserCancelled";
        var success = await _repository.CancelHandoverAsync(request.HandoverId, cancelReason, request.UserId);
        return success ? Result.Success() : Result.Error("Failed to cancel handover.");
    }

    public async Task<Result> Handle(CompleteHandoverCommand request, CancellationToken cancellationToken)
    {
        // V3 Regla #24: quien completa debe tener coverage en TO shift, NO puede ser sender
        var hasCoverage = await _repository.HasCoverageInToShiftAsync(request.HandoverId, request.UserId);
        if (!hasCoverage)
        {
            return Result.Error("Cannot complete handover: user must have coverage in the TO shift.");
        }

        // Get handover to verify user is not the sender
        var handover = await _repository.GetHandoverByIdAsync(request.HandoverId);
        if (handover?.Handover.SenderUserId == request.UserId)
        {
            return Result.Error("Cannot complete handover: sender cannot complete the handover.");
        }

        return await ExecuteStateChange(request.HandoverId, request.UserId, (id, uid) => _repository.CompleteHandoverAsync(id, uid));
    }

    private async Task<Result> ExecuteStateChange(string handoverId, string userId, Func<string, string, Task<bool>> action)
    {
        var success = await action(handoverId, userId);
        return success ? Result.Success() : Result.Error("Failed to update handover state.");
    }
}

