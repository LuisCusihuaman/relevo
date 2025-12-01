using Ardalis.SharedKernel;

namespace Relevo.Core.Events;

/// <summary>
/// Domain event dispatched when a patient is assigned to a shift.
/// This triggers automatic handover creation for shift transitions.
/// V3_PLAN.md Regla #14: Handovers are created as side effects of domain commands.
/// </summary>
public sealed class PatientAssignedToShiftEvent : DomainEventBase
{
    public string PatientId { get; init; }
    public string UserId { get; init; }
    public string ShiftId { get; init; }
    public string ShiftInstanceId { get; init; }
    public string UnitId { get; init; }
    public DateTime ShiftStartAt { get; init; }
    public DateTime ShiftEndAt { get; init; }
    public bool IsPrimary { get; init; }

    public PatientAssignedToShiftEvent(
        string patientId,
        string userId,
        string shiftId,
        string shiftInstanceId,
        string unitId,
        DateTime shiftStartAt,
        DateTime shiftEndAt,
        bool isPrimary)
    {
        PatientId = patientId;
        UserId = userId;
        ShiftId = shiftId;
        ShiftInstanceId = shiftInstanceId;
        UnitId = unitId;
        ShiftStartAt = shiftStartAt;
        ShiftEndAt = shiftEndAt;
        IsPrimary = isPrimary;
    }
}

