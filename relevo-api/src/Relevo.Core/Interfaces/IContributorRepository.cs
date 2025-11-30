using Relevo.Core.ContributorAggregate;

namespace Relevo.Core.Interfaces;

public interface IContributorRepository
{
  Task<Contributor?> GetByIdAsync(int id, CancellationToken ct = default);
  Task<Contributor> AddAsync(Contributor entity, CancellationToken ct = default);
  Task UpdateAsync(Contributor entity, CancellationToken ct = default);
  Task DeleteAsync(Contributor entity, CancellationToken ct = default);
}

