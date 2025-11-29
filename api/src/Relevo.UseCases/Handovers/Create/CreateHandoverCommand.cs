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
    string? Notes
) : ICommand<Result<HandoverRecord>>;

