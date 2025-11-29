using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.CreateSummary;

public record CreatePatientSummaryCommand(string PatientId, string SummaryText, string UserId) : ICommand<Result<PatientSummaryRecord>>;

