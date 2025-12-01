namespace Relevo.Core.Models;

public record CreateHandoverRequest(
    string PatientId,
    string FromDoctorId,
    string? ToDoctorId, // V3: Can be NULL initially, receiver-of-record is determined when completing
    string FromShiftId,
    string ToShiftId,
    string InitiatedBy,
    string? Notes
);

