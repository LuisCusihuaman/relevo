using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.GetSummary;

public record GetPatientSummaryQuery(string PatientId, string? UserId = null) : IQuery<Result<PatientSummaryRecord>>;

