namespace Relevo.Core.Interfaces;

public interface IHandoverStateService
{
    Task<bool> TryMarkAsReadyAsync(string handoverId, string? triggeringUserId = null);
}
