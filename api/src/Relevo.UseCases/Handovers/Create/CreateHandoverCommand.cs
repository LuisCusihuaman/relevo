using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.Create;

public record CreateHandoverCommand(
    string PatientId,
    string FromDoctorId,
    string ToDoctorId,
    string FromShiftId,
    string ToShiftId,
    string InitiatedBy,
    string? Notes,
    // User details for lazy provisioning (FromDoctor)
    string? UserEmail = null,
    string? FirstName = null,
    string? LastName = null,
    string? FullName = null,
    string? AvatarUrl = null,
    string? OrgRole = null
) : ICommand<Result<HandoverRecord>>;

