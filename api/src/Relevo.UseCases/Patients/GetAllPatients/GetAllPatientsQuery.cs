using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.GetAllPatients;

public record GetAllPatientsQuery(int Page, int PageSize) : IQuery<Result<GetAllPatientsResult>>;

public record GetAllPatientsResult(IReadOnlyList<PatientRecord> Patients, int TotalCount, int Page, int PageSize);

