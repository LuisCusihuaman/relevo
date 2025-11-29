namespace Relevo.Core.Models;

public record PatientDetailRecord(
    string Id,
    string Name,
    string Mrn,
    string Dob,
    string Gender,
    string AdmissionDate,
    string CurrentUnit,
    string RoomNumber,
    string Diagnosis,
    IReadOnlyList<string> Allergies,
    IReadOnlyList<string> Medications,
    string Notes
);

