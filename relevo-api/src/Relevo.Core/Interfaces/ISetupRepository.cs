namespace Relevo.Core.Interfaces;

public interface ISetupRepository
{
    IReadOnlyList<UnitRecord> GetUnits();
    IReadOnlyList<ShiftRecord> GetShifts();
    (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(string unitId, int page, int pageSize);
    (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetAllPatients(int page, int pageSize);
    Task<IReadOnlyList<string>> AssignAsync(string userId, string shiftId, IEnumerable<string> patientIds);
    Task CreateHandoverForAssignmentAsync(string assignmentId, string userId);
    (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(string userId, int page, int pageSize);
    (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetMyHandovers(string userId, int page, int pageSize);
    (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetPatientHandovers(string patientId, int page, int pageSize);
}

// Domain Records
public record UnitRecord(string Id, string Name);
public record ShiftRecord(string Id, string Name, string StartTime, string EndTime);
public record PatientRecord(
    string Id, 
    string Name, 
    string HandoverStatus, 
    string? HandoverId
);
public record HandoverRecord(
    string Id,
    string AssignmentId,
    string PatientId,
    string? PatientName,
    string Status,
    HandoverIllnessSeverity IllnessSeverity,
    HandoverPatientSummary PatientSummary,
    IReadOnlyList<HandoverActionItem> ActionItems,
    string? SituationAwarenessDocId,
    HandoverSynthesis? Synthesis,
    string ShiftName,
    string CreatedBy,
    string AssignedTo
);
public record HandoverIllnessSeverity(string Value);
public record HandoverPatientSummary(string Value);
public record HandoverSynthesis(string Value);
public record HandoverActionItem(string Id, string Description, bool IsCompleted);
