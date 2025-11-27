namespace Relevo.Core.Interfaces;

public interface IPatientSummaryRepository
{
    PatientSummaryRecord? GetPatientSummary(string patientId);
    PatientSummaryRecord CreatePatientSummary(string patientId, string physicianId, string summaryText, string createdBy);
    bool UpdatePatientSummary(string summaryId, string summaryText, string lastEditedBy);
}
