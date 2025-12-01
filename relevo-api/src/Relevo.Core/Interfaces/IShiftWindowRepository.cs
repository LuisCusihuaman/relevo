using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

public interface IShiftWindowRepository
{
    /// <summary>
    /// Gets or creates a SHIFT_WINDOW for the given FROM and TO shift instances.
    /// Returns the ID of the existing or newly created window.
    /// </summary>
    Task<string> GetOrCreateShiftWindowAsync(string fromShiftInstanceId, string toShiftInstanceId, string unitId);
    
    /// <summary>
    /// Gets a shift window by ID.
    /// </summary>
    Task<ShiftWindowRecord?> GetShiftWindowByIdAsync(string shiftWindowId);
    
    /// <summary>
    /// Gets shift windows for a unit within a date range.
    /// </summary>
    Task<IReadOnlyList<ShiftWindowRecord>> GetShiftWindowsAsync(string unitId, DateTime? startDate, DateTime? endDate);
}

