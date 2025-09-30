namespace Relevo.Core.Interfaces;

/// <summary>
/// Repository interface for physician shift data access
/// </summary>
public interface IPhysicianShiftRepository
{
    /// <summary>
    /// Gets the shift ID assigned to a physician
    /// </summary>
    /// <param name="userId">The physician's user ID</param>
    /// <returns>The shift ID, or null if not found</returns>
    Task<string?> GetPhysicianShiftIdAsync(string userId);

    /// <summary>
    /// Gets the shift times (start and end) for a shift ID
    /// </summary>
    /// <param name="shiftId">The shift ID</param>
    /// <returns>A tuple containing start time and end time, or nulls if not found</returns>
    Task<(string? startTime, string? endTime)> GetShiftTimesAsync(string shiftId);
}
