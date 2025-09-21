using Relevo.Web.Patients;
using Relevo.Web.Me;
using Relevo.Core.Interfaces;

// Use specific types from Core layer to avoid conflicts
using PatientRecord = Relevo.Core.Interfaces.PatientRecord;
using UnitRecord = Relevo.Core.Interfaces.UnitRecord;
using ShiftRecord = Relevo.Core.Interfaces.ShiftRecord;
using HandoverRecord = Relevo.Core.Interfaces.HandoverRecord;

namespace Relevo.Web.Setup;

public interface ISetupDataProvider
{
  IReadOnlyList<UnitRecord> GetUnits();
  IReadOnlyList<ShiftRecord> GetShifts();
  (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(string unitId, int page, int pageSize);
  void Assign(string userId, string shiftId, IEnumerable<string> patientIds);
  (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(string userId, int page, int pageSize);
  (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetMyHandovers(string userId, int page, int pageSize);

  // Handover Creation and Management
  Task<HandoverRecord> CreateHandoverAsync(CreateHandoverRequest request);
  Task<bool> AcceptHandoverAsync(string handoverId, string userId);
  Task<bool> CompleteHandoverAsync(string handoverId, string userId);
  Task<IReadOnlyList<HandoverRecord>> GetPendingHandoversForUserAsync(string userId);
  Task<IReadOnlyList<HandoverRecord>> GetHandoversByPatientAsync(string patientId);
  Task<IReadOnlyList<HandoverRecord>> GetShiftTransitionHandoversAsync(string fromDoctorId, string toDoctorId);
}


