using System.ComponentModel.DataAnnotations;

namespace Relevo.Core.Models;

public record HandoverActionItemFullRecord(
    [property: Required] string Id,
    [property: Required] string HandoverId,
    [property: Required] string Description,
    [property: Required] bool IsCompleted,
    [property: Required] DateTime CreatedAt,
    [property: Required] DateTime UpdatedAt,
    DateTime? CompletedAt,
    string? Priority,
    string? DueTime,
    string? CreatedBy
)
{
    public HandoverActionItemFullRecord() : this("", "", "", false, DateTime.MinValue, DateTime.MinValue, null, null, null, null) { }
}

