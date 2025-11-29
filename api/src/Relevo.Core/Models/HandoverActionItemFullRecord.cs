namespace Relevo.Core.Models;

public record HandoverActionItemFullRecord(
    string Id,
    string HandoverId,
    string Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt
)
{
    public HandoverActionItemFullRecord() : this("", "", "", false, DateTime.MinValue, DateTime.MinValue, null) { }
}

