using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.ShiftCheckIn.GetUnits;

public record GetUnitsQuery() : IQuery<Result<GetUnitsResult>>;

public record GetUnitsResult(IReadOnlyList<UnitRecord> Units);

