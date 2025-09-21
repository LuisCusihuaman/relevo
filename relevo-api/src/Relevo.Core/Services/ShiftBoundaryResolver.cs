using Relevo.Core.Interfaces;

namespace Relevo.Core.Services;

public class ShiftBoundaryResolver : IShiftBoundaryResolver
{
    private readonly ISetupRepository _setupRepository;

    public ShiftBoundaryResolver(ISetupRepository setupRepository)
    {
        _setupRepository = setupRepository;
    }

    public (DateTime windowDate, string toShiftId) Resolve(DateTime now, string fromShiftId)
    {
        // This is a placeholder implementation.
        var shifts = _setupRepository.GetShifts();
        var currentShift = shifts.FirstOrDefault(s => s.Id == fromShiftId);
        
        if (currentShift is null)
        {
            throw new InvalidOperationException($"Shift with id {fromShiftId} not found.");
        }

        // Simplistic logic: just pick the other shift.
        var nextShift = shifts.FirstOrDefault(s => s.Id != fromShiftId);

        if (nextShift is null)
        {
            throw new InvalidOperationException("Next shift could not be determined.");
        }

        // This simplistic logic does not correctly handle the window date for overnight shifts.
        return (now.Date, nextShift.Id);
    }
}
