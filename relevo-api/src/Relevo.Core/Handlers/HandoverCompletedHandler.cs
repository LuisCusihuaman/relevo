using MediatR;
using Microsoft.Extensions.Logging;
using Relevo.Core.Events;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Core.Handlers;

/// <summary>
/// Handler for HandoverCompletedEvent.
/// Automatically creates the next handover when one is completed.
/// 
/// V3_PLAN.md Regla #15: "al completar, el receptor 'toma' el pase y el próximo handover lo tendrá como emisor"
/// V3_PLAN.md Regla #52: "El patient summary se copia/arrastra del handover previo al nuevo"
/// 
/// Flow:
/// 1. Handover Day→Night completed by Dr. B
/// 2. Dr. B becomes the sender of the next handover Night→Day
/// 3. Patient summary is copied from the completed handover
/// </summary>
public class HandoverCompletedHandler(
    IHandoverRepository _handoverRepository,
    IShiftTransitionService _shiftTransitionService,
    ILogger<HandoverCompletedHandler> _logger) : INotificationHandler<HandoverCompletedEvent>
{
    public async Task Handle(HandoverCompletedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling HandoverCompletedEvent: HandoverId={HandoverId}, PatientId={PatientId}, CompletedByUserId={CompletedByUserId}, ToShiftId={ToShiftId}",
            domainEvent.HandoverId, domainEvent.PatientId, domainEvent.CompletedByUserId, domainEvent.ToShiftId);

        try
        {
            // The TO shift of the completed handover becomes the FROM shift of the next handover
            var fromShiftId = domainEvent.ToShiftId;
            
            // Get the next shift (e.g., Night -> Day)
            var toShiftId = await _shiftTransitionService.GetNextShiftIdAsync(fromShiftId);
            if (string.IsNullOrEmpty(toShiftId))
            {
                _logger.LogWarning(
                    "Cannot create next handover: next shift not found. CurrentShiftId={ShiftId}, PatientId={PatientId}",
                    fromShiftId, domainEvent.PatientId);
                return;
            }

            // Check if there's already an active handover for this patient in this direction
            // This prevents creating duplicates if the event is processed multiple times
            var existingHandoverId = await _handoverRepository.GetActiveHandoverForPatientAndFromShiftAsync(
                domainEvent.PatientId, fromShiftId);
            
            if (!string.IsNullOrEmpty(existingHandoverId))
            {
                _logger.LogInformation(
                    "Skipping next handover creation: active handover already exists. PatientId={PatientId}, FromShiftId={FromShiftId}, ExistingHandoverId={ExistingHandoverId}",
                    domainEvent.PatientId, fromShiftId, existingHandoverId);
                return;
            }

            // Create the next handover
            // The completedByUserId becomes the sender of the next handover
            // V3_PLAN.md Regla #15: "el receptor 'toma' el pase y el próximo handover lo tendrá como emisor"
            var createRequest = new CreateHandoverRequest(
                domainEvent.PatientId,
                domainEvent.CompletedByUserId, // Receiver becomes sender
                null, // Receiver not known yet
                fromShiftId,
                toShiftId,
                domainEvent.CompletedByUserId,
                $"Auto-created after completing handover {domainEvent.HandoverId}"
            );

            try
            {
                // CreateHandoverAsync will:
                // - Create the handover with PREVIOUS_HANDOVER_ID set to the completed handover
                // - Copy PATIENT_SUMMARY from the previous handover (Regla #52)
                var handover = await _handoverRepository.CreateHandoverAsync(createRequest);
                
                _logger.LogInformation(
                    "Auto-created next handover after completion. NewHandoverId={HandoverId}, PreviousHandoverId={PreviousHandoverId}, PatientId={PatientId}, FromShiftId={FromShiftId}, ToShiftId={ToShiftId}",
                    handover.Id, domainEvent.HandoverId, domainEvent.PatientId, fromShiftId, toShiftId);
            }
            catch (InvalidOperationException ex)
            {
                // Handle expected exceptions:
                // - No coverage: The receiver needs to be assigned to the next shift
                // - Duplicate: DB constraint prevents this (idempotent)
                if (ex.Message.Contains("coverage"))
                {
                    _logger.LogInformation(
                        "Next handover not created: receiver needs coverage in new FROM shift. PatientId={PatientId}, FromShiftId={FromShiftId}. " +
                        "Handover will be created when user is assigned to the shift.",
                        domainEvent.PatientId, fromShiftId);
                }
                else
                {
                    _logger.LogDebug(ex,
                        "Next handover creation skipped (likely duplicate). PatientId={PatientId}, FromShiftId={FromShiftId}",
                        domainEvent.PatientId, fromShiftId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating next handover after completion. HandoverId={HandoverId}, PatientId={PatientId}",
                domainEvent.HandoverId, domainEvent.PatientId);
            // Don't throw - we don't want the completion to fail
        }
    }
}
