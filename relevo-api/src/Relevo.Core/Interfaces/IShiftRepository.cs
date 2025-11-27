using System.Collections.Generic;

namespace Relevo.Core.Interfaces;

public interface IShiftRepository
{
    IReadOnlyList<ShiftRecord> GetShifts();
}
