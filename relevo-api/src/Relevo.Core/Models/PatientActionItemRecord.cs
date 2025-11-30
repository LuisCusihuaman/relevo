namespace Relevo.Core.Models;

public record PatientActionItemRecord(
    string Id,
    string HandoverId,
    string Description,
    bool IsCompleted,
    DateTime CreatedAt,
    string CreatedBy,
    string ShiftName
);

