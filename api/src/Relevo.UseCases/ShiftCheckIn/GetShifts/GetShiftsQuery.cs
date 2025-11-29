using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.ShiftCheckIn.GetShifts;

public record GetShiftsQuery() : IQuery<Result<GetShiftsResult>>;

public record GetShiftsResult(IReadOnlyList<ShiftRecord> Shifts);

