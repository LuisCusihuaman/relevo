using System.ComponentModel.DataAnnotations;

namespace Relevo.Core.Models;

public record PatientRecord(
    [property: Required] string Id,
    [property: Required] string Name,
    [property: Required] string HandoverStatus,
    string? HandoverId,
    decimal? Age,
    [property: Required] string Room,
    [property: Required] string Diagnosis,
    string? Status,
    string? Severity
)
{
    public PatientRecord() : this("", "", "not-started", null, null, "", "", null, null) { }
}

