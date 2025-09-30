namespace Relevo.Core.Interfaces;

/// <summary>
/// Domain service for physician shift management following hexagonal architecture
/// </summary>
public interface IPhysicianShiftService
{
    /// <summary>
    /// Gets the shift times (start and end) for a physician based on their user ID
    /// </summary>
    /// <param name="userId">The physician's user ID</param>
    /// <returns>A tuple containing start time and end time, or empty strings if not found</returns>
    Task<(string startTime, string endTime)> GetPhysicianShiftTimesAsync(string userId);
}
