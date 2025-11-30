using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

public interface IShiftRepository
{
    Task<IReadOnlyList<ShiftRecord>> GetShiftsAsync();
}

