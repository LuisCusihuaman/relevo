using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Me.Assignments;

public class UnassignPatientHandler(IAssignmentRepository _repository)
  : ICommandHandler<UnassignPatientCommand, Result>
{
  public async Task<Result> Handle(UnassignPatientCommand request, CancellationToken cancellationToken)
  {
    var success = await _repository.UnassignPatientAsync(
      request.UserId, 
      request.ShiftInstanceId, 
      request.PatientId);

    if (!success)
    {
        return Result.Error("Failed to unassign patient. The assignment may not exist.");
    }

    return Result.Success();
  }
}

