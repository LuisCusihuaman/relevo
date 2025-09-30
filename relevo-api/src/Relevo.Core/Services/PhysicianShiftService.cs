using Relevo.Core.Interfaces;

namespace Relevo.Core.Services;

/// <summary>
/// Domain service implementation for physician shift management
/// </summary>
public class PhysicianShiftService(IPhysicianShiftRepository physicianShiftRepository) : IPhysicianShiftService
{
    private readonly IPhysicianShiftRepository _physicianShiftRepository = physicianShiftRepository;

    public async Task<(string startTime, string endTime)> GetPhysicianShiftTimesAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return (string.Empty, string.Empty);

        try
        {
            // Get the shift ID for this physician
            var shiftId = await _physicianShiftRepository.GetPhysicianShiftIdAsync(userId);

            if (string.IsNullOrEmpty(shiftId))
                return (string.Empty, string.Empty);

            // Get the shift times
            var (startTime, endTime) = await _physicianShiftRepository.GetShiftTimesAsync(shiftId);

            return (
                startTime ?? string.Empty,
                endTime ?? string.Empty
            );
        }
        catch (Exception)
        {
            // Domain service handles exceptions gracefully
            return (string.Empty, string.Empty);
        }
    }
}
