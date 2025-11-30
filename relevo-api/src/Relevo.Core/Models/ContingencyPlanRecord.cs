namespace Relevo.Core.Models;

public record ContingencyPlanRecord(
    string Id,
    string HandoverId,
    string ConditionText,
    string ActionText,
    string Priority,
    string Status,
    string CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

