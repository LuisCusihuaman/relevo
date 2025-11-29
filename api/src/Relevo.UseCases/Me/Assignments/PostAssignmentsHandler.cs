using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Me.Assignments;

public class PostAssignmentsHandler(IAssignmentRepository _repository)
    : IRequestHandler<PostAssignmentsCommand, Result<IReadOnlyList<string>>>
{
    public async Task<Result<IReadOnlyList<string>>> Handle(
        PostAssignmentsCommand request,
        CancellationToken cancellationToken)
    {
        var assignedPatientIds = await _repository.AssignPatientsAsync(
            request.UserId,
            request.ShiftId,
            request.PatientIds);

        return Result.Success(assignedPatientIds);
    }
}

