namespace Relevo.Core.Models;

public record PatientHandoverDataRecord(
    string Id,
    string Name,
    string Dob,
    string Mrn,
    string AdmissionDate,
    string PrimaryTeam,
    string PrimaryDiagnosis,
    string Room,
    string Unit,
    PhysicianRecord? AssignedPhysician,
    PhysicianRecord? ReceivingPhysician,
    string? IllnessSeverity,
    string? SummaryText,
    string? LastEditedBy,
    string? UpdatedAt,
    string? Weight,
    string? Height
);

public record PhysicianRecord(
    string Name,
    string Role,
    string Color,
    string? ShiftStart,
    string? ShiftEnd,
    string Status,
    string PatientAssignment
);

