using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Relevo.UseCases.Me.Assignments;

public record UnassignMyPatientCommand(string UserId, string PatientId) : ICommand<Result>;

