using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.GetHandovers;

public record GetPatientHandoversQuery(string PatientId, int Page, int PageSize) : IQuery<Result<GetPatientHandoversResult>>;

public record GetPatientHandoversResult(IReadOnlyList<HandoverRecord> Items, int TotalCount, int Page, int PageSize);

