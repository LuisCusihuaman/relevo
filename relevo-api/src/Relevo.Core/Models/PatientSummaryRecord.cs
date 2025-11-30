namespace Relevo.Core.Models;

public record PatientSummaryRecord(
    string Id,
    string PatientId,
    string PhysicianId,
    string SummaryText,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string LastEditedBy
);

