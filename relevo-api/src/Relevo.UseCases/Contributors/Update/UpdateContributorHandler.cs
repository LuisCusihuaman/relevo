using Ardalis.Result;
using MediatR;
using Relevo.Core.ContributorAggregate;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Contributors.Update;

public class UpdateContributorHandler(IContributorService _service)
  : IRequestHandler<UpdateContributorCommand, Result<ContributorDTO>>
{
  public async Task<Result<ContributorDTO>> Handle(UpdateContributorCommand request, CancellationToken cancellationToken)
  {
    var existingContributor = await _service.GetByIdAsync(request.ContributorId);
    if (existingContributor == null)
    {
      return Result.NotFound();
    }

    existingContributor.UpdateName(request.NewName!);

    await _service.UpdateAsync(existingContributor);

    return new ContributorDTO(existingContributor.Id,
      existingContributor.Name, existingContributor.PhoneNumber?.Number ?? "");
  }
}
