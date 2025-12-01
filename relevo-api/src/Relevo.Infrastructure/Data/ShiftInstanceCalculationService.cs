using System.Data;
using Dapper;

namespace Relevo.Infrastructure.Data;

/// <summary>
/// Service for calculating shift instance dates from shift templates.
/// Extracted from HandoverRepository to avoid duplication in tests and production code.
/// </summary>
public class ShiftInstanceCalculationService
{
    /// <summary>
    /// Calculates the start and end DateTime for shift instances based on shift templates and a base date.
    /// </summary>
    /// <param name="fromShiftStartTime">Start time of FROM shift (format: "HH:mm")</param>
    /// <param name="fromShiftEndTime">End time of FROM shift (format: "HH:mm")</param>
    /// <param name="toShiftStartTime">Start time of TO shift (format: "HH:mm")</param>
    /// <param name="toShiftEndTime">End time of TO shift (format: "HH:mm")</param>
    /// <param name="baseDate">Base date to calculate from (typically DateTime.Today)</param>
    /// <returns>Tuple with (fromShiftStartAt, fromShiftEndAt, toShiftStartAt, toShiftEndAt)</returns>
    public static (DateTime fromShiftStartAt, DateTime fromShiftEndAt, DateTime toShiftStartAt, DateTime toShiftEndAt)
        CalculateShiftInstanceDates(
            string fromShiftStartTime,
            string fromShiftEndTime,
            string toShiftStartTime,
            string toShiftEndTime,
            DateTime baseDate)
    {
        var fromStartTime = ParseTime(fromShiftStartTime);
        var fromEndTime = ParseTime(fromShiftEndTime);
        var toStartTime = ParseTime(toShiftStartTime);
        var toEndTime = ParseTime(toShiftEndTime);

        var fromShiftStartAt = baseDate.Add(fromStartTime);
        var fromShiftEndAt = fromEndTime < fromStartTime
            ? baseDate.AddDays(1).Add(fromEndTime)
            : baseDate.Add(fromEndTime);

        var toShiftStartAt = toStartTime < fromEndTime
            ? baseDate.AddDays(1).Add(toStartTime)
            : baseDate.Add(toStartTime);
        var toShiftEndAt = toEndTime < toStartTime
            ? toShiftStartAt.AddDays(1).Add(toEndTime - TimeSpan.FromDays(1))
            : toShiftStartAt.Add(toEndTime - toStartTime);

        return (fromShiftStartAt, fromShiftEndAt, toShiftStartAt, toShiftEndAt);
    }

    /// <summary>
    /// Gets shift templates from database and calculates shift instance dates.
    /// </summary>
    /// <param name="connection">Database connection</param>
    /// <param name="fromShiftId">FROM shift template ID</param>
    /// <param name="toShiftId">TO shift template ID</param>
    /// <param name="baseDate">Base date to calculate from (typically DateTime.Today)</param>
    /// <returns>Tuple with shift instance dates, or null if shift templates not found</returns>
    public static async Task<(DateTime fromShiftStartAt, DateTime fromShiftEndAt, DateTime toShiftStartAt, DateTime toShiftEndAt)?>
        CalculateShiftInstanceDatesFromDbAsync(
            IDbConnection connection,
            string fromShiftId,
            string toShiftId,
            DateTime baseDate)
    {
        var fromShift = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT START_TIME, END_TIME FROM SHIFTS WHERE ID = :shiftId",
            new { shiftId = fromShiftId });

        var toShift = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT START_TIME, END_TIME FROM SHIFTS WHERE ID = :shiftId",
            new { shiftId = toShiftId });

        if (fromShift == null || toShift == null)
        {
            return null;
        }

        var (fromStart, fromEnd, toStart, toEnd) = CalculateShiftInstanceDates(
            (string)fromShift!.START_TIME,
            (string)fromShift.END_TIME,
            (string)toShift!.START_TIME,
            (string)toShift.END_TIME,
            baseDate);

        return (fromStart, fromEnd, toStart, toEnd);
    }

    private static TimeSpan ParseTime(string timeStr)
    {
        var parts = timeStr.Split(':');
        return new TimeSpan(int.Parse(parts[0]), int.Parse(parts[1]), 0);
    }
}

