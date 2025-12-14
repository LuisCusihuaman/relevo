using Ardalis.SharedKernel;

namespace Relevo.Core.Events;

/// <summary>
/// Domain event dispatched when a handover is completed.
/// This triggers automatic creation of the next handover for the following shift transition.
/// V3_PLAN.md Regla #15: "al completar, el receptor 'toma' el pase y el próximo handover lo tendrá como emisor"
/// V3_PLAN.md Regla #52: "El patient summary se copia/arrastra del handover previo al nuevo"
/// </summary>
public sealed class HandoverCompletedEvent : DomainEventBase
{
    /// <summary>
    /// The ID of the completed handover
    /// </summary>
    public string HandoverId { get; init; }
    
    /// <summary>
    /// The patient ID from the completed handover
    /// </summary>
    public string PatientId { get; init; }
    
    /// <summary>
    /// The user who completed the handover (receiver-of-record, becomes sender of next handover)
    /// </summary>
    public string CompletedByUserId { get; init; }
    
    /// <summary>
    /// The TO shift ID of the completed handover (becomes FROM shift of next handover)
    /// </summary>
    public string ToShiftId { get; init; }
    
    /// <summary>
    /// The unit ID for the handover
    /// </summary>
    public string UnitId { get; init; }

    public HandoverCompletedEvent(
        string handoverId,
        string patientId,
        string completedByUserId,
        string toShiftId,
        string unitId)
    {
        HandoverId = handoverId;
        PatientId = patientId;
        CompletedByUserId = completedByUserId;
        ToShiftId = toShiftId;
        UnitId = unitId;
    }
}
