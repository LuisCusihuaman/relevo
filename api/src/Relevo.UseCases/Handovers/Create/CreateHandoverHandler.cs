using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.Create;

public class CreateHandoverHandler(IHandoverRepository _repository)
  : ICommandHandler<CreateHandoverCommand, Result<HandoverRecord>>
{
  public async Task<Result<HandoverRecord>> Handle(CreateHandoverCommand request, CancellationToken cancellationToken)
  {
    var createRequest = new CreateHandoverRequest(
        request.PatientId,
        request.FromDoctorId,
        request.ToDoctorId,
        request.FromShiftId,
        request.ToShiftId,
        request.InitiatedBy,
        request.Notes
    );

    var handover = await _repository.CreateHandoverAsync(createRequest);

    return Result.Success(handover);
  }
}

