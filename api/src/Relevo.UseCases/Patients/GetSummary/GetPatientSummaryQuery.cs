using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.GetSummary;

public record GetPatientSummaryQuery(string PatientId) : IQuery<Result<PatientSummaryRecord>>;

