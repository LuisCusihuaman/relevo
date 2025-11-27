using System.Collections.Generic;

namespace Relevo.Core.Interfaces;

public interface IUnitRepository
{
    IReadOnlyList<UnitRecord> GetUnits();
}
