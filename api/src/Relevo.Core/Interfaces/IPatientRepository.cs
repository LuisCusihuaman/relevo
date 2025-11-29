using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

public interface IPatientRepository
{
    Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(string unitId, int page, int pageSize);
        Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetAllPatientsAsync(int page, int pageSize);
        Task<PatientDetailRecord?> GetPatientByIdAsync(string patientId);
        Task<PatientSummaryRecord?> GetPatientSummaryAsync(string patientId);
        Task<PatientSummaryRecord> CreatePatientSummaryAsync(string patientId, string physicianId, string summaryText, string createdBy);
    }
