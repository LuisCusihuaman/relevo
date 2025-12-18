using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

public interface IPatientRepository
{
    Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(string unitId, int page, int pageSize, string? userId = null);
        Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetAllPatientsAsync(int page, int pageSize);
        Task<PatientDetailRecord?> GetPatientByIdAsync(string patientId);
        Task<PatientSummaryRecord?> GetPatientSummaryFromHandoverAsync(string handoverId);
        Task<PatientSummaryRecord> CreatePatientSummaryAsync(string handoverId, string summaryText, string createdBy);
        Task<bool> UpdatePatientSummaryAsync(string handoverId, string summaryText, string lastEditedBy);
        Task<IReadOnlyList<PatientActionItemRecord>> GetPatientActionItemsAsync(string patientId);
    }
