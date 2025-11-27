using System.Collections.Generic;

namespace Relevo.Core.Interfaces;

public interface IPatientRepository
{
    (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(string unitId, int page, int pageSize);
    (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetAllPatients(int page, int pageSize);
    PatientDetailRecord? GetPatientById(string patientId);
}
