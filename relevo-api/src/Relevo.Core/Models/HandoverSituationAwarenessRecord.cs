namespace Relevo.Core.Models;

public record HandoverSituationAwarenessRecord(
    string HandoverId,
    string? Content,
    string Status,
    string LastEditedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

