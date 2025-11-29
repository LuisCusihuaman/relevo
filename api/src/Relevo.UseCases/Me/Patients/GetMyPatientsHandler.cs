using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Patients;

public class GetMyPatientsHandler(IAssignmentRepository _repository)
    : IRequestHandler<GetMyPatientsQuery, Result<(IReadOnlyList<PatientRecord> Patients, int TotalCount)>>
{
    public async Task<Result<(IReadOnlyList<PatientRecord> Patients, int TotalCount)>> Handle(
        GetMyPatientsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _repository.GetMyPatientsAsync(request.UserId, request.Page, request.PageSize);
        return Result.Success(result);
    }
}

