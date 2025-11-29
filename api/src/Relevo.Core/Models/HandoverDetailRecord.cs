namespace Relevo.Core.Models;

public record ActionItemRecord(string Id, string Description, bool IsCompleted);

public record HandoverDetailRecord(
    HandoverRecord Handover,
    IReadOnlyList<ActionItemRecord> ActionItems
);

