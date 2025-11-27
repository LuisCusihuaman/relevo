using Relevo.Core.Interfaces;

namespace Relevo.Core.Services;

public class HandoverStateService : IHandoverStateService
{
    private readonly IHandoverRepository _repository;

    public HandoverStateService(IHandoverRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> TryMarkAsReadyAsync(string handoverId, string? triggeringUserId = null)
    {
        var handover = _repository.GetHandoverById(handoverId);

        if (handover is null)
        {
            return false; // Or throw an exception
        }

        // Guards
        if (handover.StateName is "Completed" or "Accepted" or "Ready")
        {
            return false; // Already in a state that cannot be marked as ready again
        }
        
        // This is a simplified check for minimum content.
        bool HasMinimumContent(HandoverRecord h) => !string.IsNullOrEmpty(h.PatientSummary.Content);

        if (HasMinimumContent(handover))
        {
            return await _repository.ReadyHandover(handoverId, triggeringUserId ?? handover.CreatedBy);
        }

        return false;
    }
}
