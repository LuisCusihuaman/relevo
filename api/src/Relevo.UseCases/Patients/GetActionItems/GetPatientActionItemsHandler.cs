using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.GetActionItems;

public class GetPatientActionItemsHandler(IPatientRepository _repository)
  : IQueryHandler<GetPatientActionItemsQuery, Result<IReadOnlyList<PatientActionItemRecord>>>
{
  public async Task<Result<IReadOnlyList<PatientActionItemRecord>>> Handle(GetPatientActionItemsQuery request, CancellationToken cancellationToken)
  {
    var actionItems = await _repository.GetPatientActionItemsAsync(request.PatientId);
    return Result.Success(actionItems);
  }
}

