namespace Relevo.Core.Interfaces;

public interface IShiftBoundaryResolver
{
    (DateTime windowDate, string toShiftId) Resolve(DateTime now, string fromShiftId);
}
