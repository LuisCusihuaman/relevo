using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.Create;

public class CreateHandoverHandler(
    IHandoverRepository _repository,
    IUserRepository _userRepository)
  : ICommandHandler<CreateHandoverCommand, Result<HandoverRecord>>
{
  public async Task<Result<HandoverRecord>> Handle(CreateHandoverCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Ensure the creator (FromDoctor) exists
      await _userRepository.EnsureUserExistsAsync(
          request.FromDoctorId, 
          request.UserEmail,
          request.FirstName,
          request.LastName,
          request.FullName,
          request.AvatarUrl,
          request.OrgRole
      );

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
    catch (InvalidOperationException ex)
    {
      // V3_PLAN.md regla #10: Cannot create handover without coverage
      return Result.Error(ex.Message);
    }
  }
}

