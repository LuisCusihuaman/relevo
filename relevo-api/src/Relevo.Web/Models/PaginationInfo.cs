using Relevo.Core.Interfaces;

namespace Relevo.Web.Models;

public class PaginationInfo
{
    public int TotalCount { get; set; }
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    // Computed properties for backward compatibility
    public int TotalPagesComputed => PageSize > 0 ? (int)Math.Ceiling((double)(TotalCount > 0 ? TotalCount : TotalItems) / PageSize) : 0;
    public int PageOrCurrentPage => Page > 0 ? Page : CurrentPage;
}

// Handover Messages
public record GetHandoverMessagesRequest(string HandoverId);

public record GetHandoverMessagesResponse
{
    public required IReadOnlyList<HandoverMessageRecord> Messages { get; init; }
}

public record CreateHandoverMessageRequest(string HandoverId, string MessageText, string? MessageType);

public record CreateHandoverMessageResponse
{
    public required bool Success { get; init; }
    public required HandoverMessageRecord Message { get; init; }
}

// Handover Activity Log
public record GetHandoverActivityRequest(string HandoverId);

public record GetHandoverActivityResponse
{
    public required IReadOnlyList<HandoverActivityItemRecord> Activities { get; init; }
}

// Handover Checklists
public record GetHandoverChecklistsRequest(string HandoverId);

public record GetHandoverChecklistsResponse
{
    public required IReadOnlyList<HandoverChecklistItemRecord> Checklists { get; init; }
}

public record UpdateChecklistItemRequest(string HandoverId, string ItemId, bool IsChecked);

public record UpdateChecklistItemResponse
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
}

// Handover Contingency Plans
public record GetHandoverContingencyRequest(string HandoverId);

public record GetHandoverContingencyResponse
{
    public required IReadOnlyList<HandoverContingencyPlanRecord> ContingencyPlans { get; init; }
}

public record CreateContingencyPlanRequest(string HandoverId, string ConditionText, string ActionText, string Priority);

public record CreateContingencyPlanResponse
{
    public required bool Success { get; init; }
    public required HandoverContingencyPlanRecord ContingencyPlan { get; init; }
}

// Active Handover Response
public record GetActiveHandoverResponse
{
    public required HandoverRecord Handover { get; init; }
    public required IReadOnlyList<HandoverParticipantRecord> Participants { get; init; }
    public required IReadOnlyList<HandoverSectionRecord> Sections { get; init; }
    public required HandoverSyncStatusRecord? SyncStatus { get; init; }
}
