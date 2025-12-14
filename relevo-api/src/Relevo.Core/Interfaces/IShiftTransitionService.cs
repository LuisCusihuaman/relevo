namespace Relevo.Core.Interfaces;

/// <summary>
/// Service to determine shift transitions (e.g., Day -> Night, Night -> Day).
/// V3_PLAN.md: For MVP there are only 2 shift templates: Day and Night.
/// </summary>
public interface IShiftTransitionService
{
    /// <summary>
    /// Gets the next shift ID after the given shift.
    /// For MVP: Day -> Night, Night -> Day.
    /// </summary>
    /// <param name="currentShiftId">Current shift ID (e.g., "shift-day", "shift-night")</param>
    /// <returns>Next shift ID, or null if transition cannot be determined</returns>
    Task<string?> GetNextShiftIdAsync(string currentShiftId);

    /// <summary>
    /// Gets the previous shift ID before the given shift.
    /// For MVP: Night -> Day (previous), Day -> Night (previous).
    /// Used to determine if an assignment is to the TO shift of an existing handover.
    /// </summary>
    /// <param name="currentShiftId">Current shift ID (e.g., "shift-day", "shift-night")</param>
    /// <returns>Previous shift ID, or null if transition cannot be determined</returns>
    Task<string?> GetPreviousShiftIdAsync(string currentShiftId);
}

