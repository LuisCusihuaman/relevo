using Ardalis.Result;
using MediatR;

namespace Relevo.UseCases.Me.Assignments;

public record PostAssignmentsCommand(
  string UserId, 
  string ShiftId, 
  IEnumerable<string> PatientIds,
  // User details for lazy provisioning
  string? UserEmail = null,
  string? FirstName = null,
  string? LastName = null,
  string? FullName = null,
  string? AvatarUrl = null,
  string? OrgRole = null
) : IRequest<Result<IReadOnlyList<string>>>;

