using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

public interface IShiftInstanceRepository
{
    /// <summary>
    /// Gets or creates a SHIFT_INSTANCE for the given shift template, unit, and start time.
    /// Returns the ID of the existing or newly created instance.
    /// </summary>
    Task<string> GetOrCreateShiftInstanceAsync(string shiftId, string unitId, DateTime startAt, DateTime endAt);
    
    /// <summary>
    /// Gets a shift instance by ID.
    /// </summary>
    Task<ShiftInstanceRecord?> GetShiftInstanceByIdAsync(string shiftInstanceId);
    
    /// <summary>
    /// Gets shift instances for a unit within a date range.
    /// </summary>
    Task<IReadOnlyList<ShiftInstanceRecord>> GetShiftInstancesAsync(string unitId, DateTime? startDate, DateTime? endDate);
}

