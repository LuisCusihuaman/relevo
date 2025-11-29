using Ardalis.Result;
using MediatR;

namespace Relevo.UseCases.Me.Assignments;

public record PostAssignmentsCommand(
    string UserId,
    string ShiftId,
    IEnumerable<string> PatientIds,
    string? UserEmail = null
) : IRequest<Result<IReadOnlyList<string>>>;

