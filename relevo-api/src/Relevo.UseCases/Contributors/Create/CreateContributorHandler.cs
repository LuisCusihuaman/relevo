using Ardalis.Result;
using MediatR;
using Relevo.Core.ContributorAggregate;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Contributors.Create;

public class CreateContributorHandler(IContributorService _service)
  : IRequestHandler<CreateContributorCommand, Result<int>>
{
  public async Task<Result<int>> Handle(CreateContributorCommand request,
    CancellationToken cancellationToken)
  {
    var newContributor = new Contributor(request.Name);
    if (!string.IsNullOrEmpty(request.PhoneNumber))
    {
      newContributor.SetPhoneNumber(request.PhoneNumber);
    }
    var newId = await _service.CreateAsync(newContributor);

    return newId;
  }
}
