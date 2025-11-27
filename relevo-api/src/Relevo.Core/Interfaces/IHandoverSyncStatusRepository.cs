namespace Relevo.Core.Interfaces;

public interface IHandoverSyncStatusRepository
{
    HandoverSyncStatusRecord? GetHandoverSyncStatus(string handoverId, string userId);
}
