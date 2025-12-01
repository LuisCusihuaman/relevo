namespace Relevo.Core.Models;

public record ShiftInstanceRecord(
    string Id,
    string UnitId,
    string ShiftId,
    DateTime StartAt,
    DateTime EndAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public ShiftInstanceRecord() : this("", "", "", DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue) { }
}





