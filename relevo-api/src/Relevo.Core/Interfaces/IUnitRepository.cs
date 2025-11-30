using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

public interface IUnitRepository
{
    Task<IReadOnlyList<UnitRecord>> GetUnitsAsync();
}

