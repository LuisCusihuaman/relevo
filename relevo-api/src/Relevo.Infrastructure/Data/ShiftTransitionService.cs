using System.Data;
using Dapper;
using Relevo.Core.Interfaces;

namespace Relevo.Infrastructure.Data;

/// <summary>
/// Implementation of shift transition logic.
/// V3_PLAN.md Regla #6: For MVP there are only 2 shift templates: Day and Night.
/// </summary>
public class ShiftTransitionService(DapperConnectionFactory _connectionFactory) : IShiftTransitionService
{
    public async Task<string?> GetNextShiftIdAsync(string currentShiftId)
    {
        using var conn = _connectionFactory.CreateConnection();

        // Get all shifts to determine transition
        const string getShiftsSql = @"
            SELECT ID, NAME, START_TIME, END_TIME
            FROM SHIFTS
            ORDER BY START_TIME";

        var shifts = await conn.QueryAsync<dynamic>(getShiftsSql);

        var shiftList = shifts.ToList();
        if (shiftList.Count < 2)
        {
            // Need at least 2 shifts for transitions
            return null;
        }

        // Find current shift
        var currentShift = shiftList.FirstOrDefault(s => (string)s.ID == currentShiftId);
        if (currentShift == null)
        {
            return null;
        }

        // For MVP with 2 shifts: if Day -> Night, if Night -> Day
        // More robust: find shift that starts after current shift ends
        var currentStartTime = ParseTime((string)currentShift.START_TIME);
        var currentEndTime = ParseTime((string)currentShift.END_TIME);
        
        // Handle overnight shifts (end < start means it goes to next day)
        var currentEnd = currentEndTime < currentStartTime 
            ? TimeSpan.FromDays(1).Add(currentEndTime)
            : currentEndTime;

        // Find next shift that starts after current shift ends
        var nextShift = shiftList
            .Select(s => new
            {
                Id = (string)s.ID,
                StartTime = ParseTime((string)s.START_TIME),
                EndTime = ParseTime((string)s.END_TIME)
            })
            .OrderBy(s => s.StartTime)
            .FirstOrDefault(s => 
            {
                // Normalize start time (if overnight, add day)
                var normalizedStart = s.StartTime < currentStartTime 
                    ? TimeSpan.FromDays(1).Add(s.StartTime)
                    : s.StartTime;
                return normalizedStart >= currentEnd;
            });

        if (nextShift != null)
        {
            return nextShift.Id;
        }

        // Fallback: if no shift found after, wrap around to first shift
        // This handles the case where Night -> Day (next day)
        return shiftList
            .OrderBy(s => ParseTime((string)s.START_TIME))
            .First()
            .ID;
    }

    public async Task<string?> GetPreviousShiftIdAsync(string currentShiftId)
    {
        using var conn = _connectionFactory.CreateConnection();

        // Get all shifts to determine transition
        const string getShiftsSql = @"
            SELECT ID, NAME, START_TIME, END_TIME
            FROM SHIFTS
            ORDER BY START_TIME";

        var shifts = await conn.QueryAsync<dynamic>(getShiftsSql);

        var shiftList = shifts.ToList();
        if (shiftList.Count < 2)
        {
            // Need at least 2 shifts for transitions
            return null;
        }

        // For MVP with 2 shifts: Previous of Day is Night, Previous of Night is Day
        // This is the inverse of GetNextShiftIdAsync
        var currentShift = shiftList.FirstOrDefault(s => (string)s.ID == currentShiftId);
        if (currentShift == null)
        {
            return null;
        }

        // Find the other shift (for MVP with 2 shifts)
        var otherShift = shiftList.FirstOrDefault(s => (string)s.ID != currentShiftId);
        return otherShift?.ID;
    }

    private static TimeSpan ParseTime(string timeString)
    {
        var parts = timeString.Split(':');
        if (parts.Length != 2)
            throw new FormatException($"Invalid time format: {timeString}");

        return new TimeSpan(int.Parse(parts[0]), int.Parse(parts[1]), 0);
    }
}

