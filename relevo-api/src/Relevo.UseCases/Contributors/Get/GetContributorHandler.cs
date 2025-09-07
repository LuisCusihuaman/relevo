using Ardalis.Result;
using MediatR;
using Relevo.Core.ContributorAggregate;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Contributors.Get;

/// <summary>
/// Queries use ContributorService directly with Dapper
/// </summary>
public class GetContributorHandler(IContributorService _service)
  : IRequestHandler<GetContributorQuery, Result<ContributorDTO>>
{
  public async Task<Result<ContributorDTO>> Handle(GetContributorQuery request, CancellationToken cancellationToken)
  {
    var entity = await _service.GetByIdAsync(request.ContributorId);
    if (entity == null) return Result.NotFound();

    string? phoneNumber = null;
    if (entity.PhoneNumber is not null)
    {
      phoneNumber = $"{entity.PhoneNumber.CountryCode}{entity.PhoneNumber.Number}";
      if (!string.IsNullOrEmpty(entity.PhoneNumber.Extension))
      {
        phoneNumber += $" ext {entity.PhoneNumber.Extension}";
      }
    }

    return new ContributorDTO(entity.Id, entity.Name, phoneNumber);
  }
}
