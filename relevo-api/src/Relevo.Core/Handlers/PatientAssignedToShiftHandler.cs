using MediatR;
using Microsoft.Extensions.Logging;
using Relevo.Core.Events;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Core.Handlers;

/// <summary>
/// Handler for PatientAssignedToShiftEvent.
/// Automatically creates handovers when patients are assigned to shifts.
/// V3_PLAN.md Regla #14: Handovers are created as side effects of domain commands.
/// 
/// This handler reuses the existing CreateHandoverAsync logic from HandoverRepository,
/// which already handles all the complexity of shift instances, windows, coverage validation, etc.
/// </summary>
public class PatientAssignedToShiftHandler(
    IHandoverRepository _handoverRepository,
    IShiftTransitionService _shiftTransitionService,
    ILogger<PatientAssignedToShiftHandler> _logger) : INotificationHandler<PatientAssignedToShiftEvent>
{
    public async Task Handle(PatientAssignedToShiftEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling PatientAssignedToShiftEvent: PatientId={PatientId}, UserId={UserId}, ShiftId={ShiftId}, IsPrimary={IsPrimary}",
            domainEvent.PatientId, domainEvent.UserId, domainEvent.ShiftId, domainEvent.IsPrimary);

        try
        {
            // Only create handover if this is the primary assignment
            // V3_PLAN.md: Sender is the primary of FROM shift
            if (!domainEvent.IsPrimary)
            {
                _logger.LogDebug(
                    "Skipping handover creation: assignment is not primary. PatientId={PatientId}, ShiftId={ShiftId}",
                    domainEvent.PatientId, domainEvent.ShiftId);
                return;
            }

            // Get next shift ID (e.g., Day -> Night, Night -> Day)
            var nextShiftId = await _shiftTransitionService.GetNextShiftIdAsync(domainEvent.ShiftId);
            if (string.IsNullOrEmpty(nextShiftId))
            {
                _logger.LogWarning(
                    "Cannot create handover: next shift not found. CurrentShiftId={ShiftId}, PatientId={PatientId}",
                    domainEvent.ShiftId, domainEvent.PatientId);
                return;
            }

            // Reuse existing CreateHandoverAsync logic - it handles:
            // - Shift instance creation/getting
            // - Shift window creation/getting
            // - Coverage validation (V3_PLAN.md Regla #10)
            // - Sender selection from SHIFT_COVERAGE
            // - Previous handover linking
            // - Idempotency via DB constraint UQ_HO_PAT_WINDOW
            var createRequest = new CreateHandoverRequest(
                domainEvent.PatientId,
                domainEvent.UserId, // Will be overridden by CreateHandoverAsync from SHIFT_COVERAGE
                null, // Receiver not known yet, will be determined when completing
                domainEvent.ShiftId,
                nextShiftId,
                domainEvent.UserId,
                $"Auto-created handover when patient assigned to {domainEvent.ShiftId} shift"
            );

            // CreateHandoverAsync will throw InvalidOperationException if:
            // - Patient has no coverage in FROM shift (V3_PLAN.md Regla #10)
            // - Shift templates not found
            // The DB constraint UQ_HO_PAT_WINDOW will prevent duplicates (idempotency)
            try
            {
                var handover = await _handoverRepository.CreateHandoverAsync(createRequest);
                _logger.LogInformation(
                    "Auto-created handover. HandoverId={HandoverId}, PatientId={PatientId}, FromShiftId={FromShiftId}, ToShiftId={ToShiftId}",
                    handover.Id, domainEvent.PatientId, domainEvent.ShiftId, nextShiftId);
            }
            catch (InvalidOperationException ex)
            {
                // Handle expected exceptions gracefully:
                // - No coverage: This shouldn't happen if event is published correctly, but log it
                // - Duplicate: DB constraint prevents this, but if it happens, it's fine (idempotent)
                if (ex.Message.Contains("coverage") || ex.Message.Contains("shift templates"))
                {
                    _logger.LogWarning(ex,
                        "Cannot create handover automatically. PatientId={PatientId}, ShiftId={ShiftId}, Reason={Reason}",
                        domainEvent.PatientId, domainEvent.ShiftId, ex.Message);
                }
                else
                {
                    // Other InvalidOperationException (e.g., duplicate) - log as debug (expected in race conditions)
                    _logger.LogDebug(ex,
                        "Handover creation skipped (likely duplicate). PatientId={PatientId}, ShiftId={ShiftId}",
                        domainEvent.PatientId, domainEvent.ShiftId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating handover automatically. PatientId={PatientId}, ShiftId={ShiftId}",
                domainEvent.PatientId, domainEvent.ShiftId);
            // Don't throw - we don't want assignment to fail if handover creation fails
            // The handover can be created manually later if needed
        }
    }
}

