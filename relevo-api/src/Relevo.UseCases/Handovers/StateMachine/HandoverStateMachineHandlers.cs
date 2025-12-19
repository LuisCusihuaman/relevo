using Ardalis.Result;
using Ardalis.SharedKernel;
using MediatR;
using Relevo.Core.Events;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Handovers.StateMachine;

public class HandoverStateMachineHandlers(
    IHandoverRepository _repository,
    IMediator _mediator) :
    ICommandHandler<StartHandoverCommand, Result>,
    ICommandHandler<RejectHandoverCommand, Result>,
    ICommandHandler<CancelHandoverCommand, Result>,
    ICommandHandler<CompleteHandoverCommand, Result>
{
    public async Task<Result> Handle(StartHandoverCommand request, CancellationToken cancellationToken)
    {
        // V3: Start handover - sender can now start their own handover
        return await ExecuteStateChange(request.HandoverId, request.UserId, (id, uid) => _repository.StartHandoverAsync(id, uid, request.ReceiverUserId));
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

        // Execute the completion
        var success = await _repository.CompleteHandoverAsync(request.HandoverId, request.UserId);
        if (!success)
        {
            return Result.Error("Failed to complete handover.");
        }

        // V3_PLAN.md Regla #15: "al completar, el receptor 'toma' el pase y el próximo handover lo tendrá como emisor"
        // Publish HandoverCompletedEvent to trigger creation of the next handover
        var completionInfo = await _repository.GetHandoverCompletionInfoAsync(request.HandoverId);
        if (completionInfo.HasValue)
        {
            var (patientId, toShiftId, unitId) = completionInfo.Value;
            var completedEvent = new HandoverCompletedEvent(
                request.HandoverId,
                patientId,
                request.UserId,
                toShiftId,
                unitId
            );
            
            // Fire-and-forget: don't await, don't block the completion
            // The next handover creation is a side effect, not critical path
            _ = Task.Run(async () =>
            {
                try
                {
                    await _mediator.Publish(completedEvent, CancellationToken.None);
                }
                catch
                {
                    // Log but don't fail - next handover can be created manually or on next assignment
                }
            }, CancellationToken.None);
        }

        return Result.Success();
    }

    private async Task<Result> ExecuteStateChange(string handoverId, string userId, Func<string, string, Task<bool>> action)
    {
        var success = await action(handoverId, userId);
        return success ? Result.Success() : Result.Error("Failed to update handover state.");
    }
}
