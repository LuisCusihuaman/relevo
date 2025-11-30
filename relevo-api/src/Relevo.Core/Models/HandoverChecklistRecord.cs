namespace Relevo.Core.Models;

public record HandoverChecklistRecord(
    string Id,
    string HandoverId,
    string UserId,
    string ItemId,
    string ItemCategory,
    string ItemLabel,
    string? ItemDescription,
    bool IsRequired,
    bool IsChecked,
    DateTime? CheckedAt,
    DateTime CreatedAt
)
{
    public HandoverChecklistRecord() : this("", "", "", "", "", "", null, false, false, null, DateTime.MinValue) { }
}

