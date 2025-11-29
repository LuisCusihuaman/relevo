namespace Relevo.Core.Models;

public record HandoverClinicalDataRecord(
    string HandoverId,
    string IllnessSeverity,
    string SummaryText,
    string LastEditedBy,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

