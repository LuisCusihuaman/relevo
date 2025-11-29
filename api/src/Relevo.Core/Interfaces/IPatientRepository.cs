using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

public interface IPatientRepository
{
    Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(string unitId, int page, int pageSize);
}

