namespace Relevo.Core.Models;

public record HandoverSynthesisRecord(
    string HandoverId,
    string? Content,
    string Status,
    string LastEditedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

