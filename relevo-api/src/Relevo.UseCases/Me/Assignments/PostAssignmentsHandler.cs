using Ardalis.Result;
using MediatR;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Me.Assignments;

public class PostAssignmentsHandler(
    IAssignmentRepository _repository,
    IUserRepository _userRepository)
    : IRequestHandler<PostAssignmentsCommand, Result<IReadOnlyList<string>>>
{
    public async Task<Result<IReadOnlyList<string>>> Handle(
        PostAssignmentsCommand request,
        CancellationToken cancellationToken)
    {
        // Ensure user exists in local DB before assignment (Lazy Provisioning)
        // This prevents FK violations when a new Clerk user makes their first assignment
        await _userRepository.EnsureUserExistsAsync(
            request.UserId, 
            request.UserEmail,
            request.FirstName,
            request.LastName,
            request.FullName,
            request.AvatarUrl,
            request.OrgRole
        );

        var assignedPatientIds = await _repository.AssignPatientsAsync(
            request.UserId,
            request.ShiftId,
            request.PatientIds);

        return Result.Success(assignedPatientIds);
    }
}

