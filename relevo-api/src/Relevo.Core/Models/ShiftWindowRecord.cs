namespace Relevo.Core.Models;

public record ShiftWindowRecord(
    string Id,
    string UnitId,
    string FromShiftInstanceId,
    string ToShiftInstanceId,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public ShiftWindowRecord() : this("", "", "", "", DateTime.MinValue, DateTime.MinValue) { }
}





