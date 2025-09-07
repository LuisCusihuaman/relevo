using Relevo.Core.ContributorAggregate;

namespace Relevo.Core.Interfaces;

public interface IContributorService
{
    Task<int> CreateAsync(Contributor contributor);
    Task<Contributor?> GetByIdAsync(int id);
    Task UpdateAsync(Contributor contributor);
    Task DeleteAsync(int id);
    Task<List<Contributor>> GetAllAsync();
}
