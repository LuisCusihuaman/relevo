using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Me.Assignments;

public class UnassignMyPatientHandler(IAssignmentRepository _repository)
  : ICommandHandler<UnassignMyPatientCommand, Result>
{
  public async Task<Result> Handle(UnassignMyPatientCommand request, CancellationToken cancellationToken)
  {
    var success = await _repository.UnassignMyPatientAsync(
      request.UserId, 
      request.PatientId);

    if (!success)
    {
      return Result.Error("Failed to unassign patient. No active assignment found for this patient.");
    }

    return Result.Success();
  }
}

