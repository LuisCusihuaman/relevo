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
    Task<HandoverClinicalDataRecord?> GetClinicalDataAsync(string handoverId);
    Task<bool> UpdateClinicalDataAsync(string handoverId, string illnessSeverity, string summaryText, string userId);
}
