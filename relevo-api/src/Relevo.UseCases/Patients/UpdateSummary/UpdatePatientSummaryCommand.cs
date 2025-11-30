using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Relevo.UseCases.Patients.UpdateSummary;

public record UpdatePatientSummaryCommand(string PatientId, string SummaryText, string UserId) : ICommand<Result>;

