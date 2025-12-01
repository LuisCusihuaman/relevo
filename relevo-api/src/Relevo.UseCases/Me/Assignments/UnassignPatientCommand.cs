using Ardalis.SharedKernel;

namespace Relevo.UseCases.Me.Assignments;

public record UnassignPatientCommand(string UserId, string ShiftInstanceId, string PatientId) 
  : ICommand<Ardalis.Result.Result>;

