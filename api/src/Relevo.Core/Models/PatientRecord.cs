namespace Relevo.Core.Models;

public record PatientRecord(
    string Id,
    string Name,
    string HandoverStatus,
    string? HandoverId,
    decimal? Age,
    string Room,
    string Diagnosis,
    string? Status,
    string? Severity
);

