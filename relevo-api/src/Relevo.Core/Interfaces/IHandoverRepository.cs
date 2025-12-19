using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

public interface IHandoverRepository
{
    Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetPatientHandoversAsync(string patientId, int page, int pageSize);
    Task<HandoverDetailRecord?> GetHandoverByIdAsync(string handoverId);
    Task<PatientHandoverDataRecord?> GetPatientHandoverDataAsync(string handoverId);
    Task<HandoverRecord> CreateHandoverAsync(CreateHandoverRequest request);
    Task<IReadOnlyList<ContingencyPlanRecord>> GetContingencyPlansAsync(string handoverId);
    Task<ContingencyPlanRecord> CreateContingencyPlanAsync(string handoverId, string condition, string action, string priority, string createdBy);
    Task<bool> DeleteContingencyPlanAsync(string handoverId, string contingencyId);
    Task<HandoverSynthesisRecord?> GetSynthesisAsync(string handoverId);
    Task<bool> UpdateSynthesisAsync(string handoverId, string? content, string status, string userId);
    Task<HandoverSituationAwarenessRecord?> GetSituationAwarenessAsync(string handoverId);
    Task<bool> UpdateSituationAwarenessAsync(string handoverId, string? content, string status, string userId);
    Task<bool> MarkAsReadyAsync(string handoverId, string userId);
    Task<bool> ReturnForChangesAsync(string handoverId, string userId);
    Task<HandoverClinicalDataRecord?> GetClinicalDataAsync(string handoverId);
    Task<bool> UpdateClinicalDataAsync(string handoverId, string illnessSeverity, string summaryText, string userId);
    Task<bool> StartHandoverAsync(string handoverId, string userId, string? receiverUserId = null);
    Task<bool> RejectHandoverAsync(string handoverId, string cancelReason, string userId); // Uses Cancel with CANCEL_REASON='ReceiverRefused'
    Task<bool> CancelHandoverAsync(string handoverId, string cancelReason, string userId); // V3 requires cancelReason
    Task<bool> CompleteHandoverAsync(string handoverId, string userId);
    Task<bool> UpdateReceiverAsync(string handoverId, string receiverUserId);
    Task<IReadOnlyList<HandoverRecord>> GetPendingHandoversAsync(string userId);

    // Action Items
    Task<IReadOnlyList<HandoverActionItemFullRecord>> GetActionItemsAsync(string handoverId);
    Task<HandoverActionItemFullRecord> CreateActionItemAsync(string handoverId, string description, string priority, string? dueTime, string createdBy);
    Task<bool> UpdateActionItemAsync(string handoverId, string itemId, bool isCompleted);
    Task<bool> DeleteActionItemAsync(string handoverId, string itemId);

    // Messages
    Task<IReadOnlyList<HandoverMessageRecord>> GetMessagesAsync(string handoverId);
    Task<HandoverMessageRecord> CreateMessageAsync(string handoverId, string userId, string userName, string messageText, string messageType);

    // My Handovers
    Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetMyHandoversAsync(string userId, int page, int pageSize);

    // Patient Summary (uses current handover)
    Task<string?> GetCurrentHandoverIdAsync(string patientId);

    // Coverage validation (V3 app-enforced rules)
    Task<bool> HasCoverageInToShiftAsync(string handoverId, string userId);

    /// <summary>
    /// Gets the ID of an active handover where the given shift is the TO shift.
    /// Used to detect if a patient assignment is for receiving (TO shift) rather than sending (FROM shift).
    /// Regla #27: Receiver assignment should NOT create a new handover.
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="toShiftId">The shift template ID (e.g., "shift-day", "shift-night")</param>
    /// <returns>Handover ID if an active handover exists with this TO shift, null otherwise</returns>
    Task<string?> GetActiveHandoverForPatientAndToShiftAsync(string patientId, string toShiftId);

    /// <summary>
    /// Gets the ID of an active handover where the given shift is the FROM shift.
    /// Used to prevent duplicate handover creation when completing a handover.
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="fromShiftId">The shift template ID (e.g., "shift-day", "shift-night")</param>
    /// <returns>Handover ID if an active handover exists with this FROM shift, null otherwise</returns>
    Task<string?> GetActiveHandoverForPatientAndFromShiftAsync(string patientId, string fromShiftId);

    /// <summary>
    /// Gets the information needed to create the next handover after completing one.
    /// Used by HandoverCompletedHandler to create the chained handover.
    /// </summary>
    /// <param name="handoverId">The completed handover ID</param>
    /// <returns>Tuple with PatientId, ToShiftId (shift template ID), and UnitId</returns>
    Task<(string PatientId, string ToShiftId, string UnitId)?> GetHandoverCompletionInfoAsync(string handoverId);
}
