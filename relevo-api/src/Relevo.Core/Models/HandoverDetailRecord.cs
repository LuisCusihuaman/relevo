namespace Relevo.Core.Models;

public record ActionItemRecord(string Id, string Description, bool IsCompleted)
{
    // Parameterless constructor for Dapper
    public ActionItemRecord() : this("", "", false) { }
}

public record HandoverDetailRecord(
    HandoverRecord Handover,
    IReadOnlyList<ActionItemRecord> ActionItems
);

