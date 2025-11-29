using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Units.GetPatientsByUnit;

public record GetPatientsByUnitQuery(string UnitId, int Page, int PageSize) : IQuery<Result<GetPatientsByUnitResult>>;

public record GetPatientsByUnitResult(IReadOnlyList<PatientRecord> Patients, int TotalCount, int Page, int PageSize);

