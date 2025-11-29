using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Handovers.UpdateClinicalData;

public class UpdateHandoverClinicalDataHandler(IHandoverRepository _repository)
  : ICommandHandler<UpdateHandoverClinicalDataCommand, Result>
{
  public async Task<Result> Handle(UpdateHandoverClinicalDataCommand request, CancellationToken cancellationToken)
  {
    var success = await _repository.UpdateClinicalDataAsync(
        request.HandoverId,
        request.IllnessSeverity,
        request.SummaryText,
        request.UserId
    );

    if (!success)
    {
        return Result.Error("Failed to update clinical data. Handover may not exist.");
    }

    return Result.Success();
  }
}

