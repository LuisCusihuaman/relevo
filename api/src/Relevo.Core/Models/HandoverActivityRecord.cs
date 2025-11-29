namespace Relevo.Core.Models;

public record HandoverActivityRecord(
    string Id,
    string HandoverId,
    string UserId,
    string UserName,
    string ActivityType,
    string? ActivityDescription,
    string? SectionAffected,
    string? Metadata,
    DateTime CreatedAt
)
{
    public HandoverActivityRecord() : this("", "", "", "", "", null, null, null, DateTime.MinValue) { }
}

