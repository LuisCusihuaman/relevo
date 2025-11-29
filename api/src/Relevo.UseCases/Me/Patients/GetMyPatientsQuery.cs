using Ardalis.Result;
using MediatR;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Patients;

public record GetMyPatientsQuery(
    string UserId,
    int Page,
    int PageSize
) : IRequest<Result<(IReadOnlyList<PatientRecord> Patients, int TotalCount)>>;

