using Relevo.Web.Patients;

namespace Relevo.Web.Setup;

public interface ISetupDataProvider
{
  IReadOnlyList<UnitRecord> GetUnits();
  IReadOnlyList<ShiftRecord> GetShifts();
  (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(string unitId, int page, int pageSize);
  void Assign(string userId, string shiftId, IEnumerable<string> patientIds);
  (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(string userId, int page, int pageSize);
}


