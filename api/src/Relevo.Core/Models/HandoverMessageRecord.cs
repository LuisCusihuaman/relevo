namespace Relevo.Core.Models;

public record HandoverMessageRecord(
    string Id,
    string HandoverId,
    string UserId,
    string UserName,
    string MessageText,
    string MessageType,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public HandoverMessageRecord() : this("", "", "", "", "", "message", DateTime.MinValue, DateTime.MinValue) { }
}

