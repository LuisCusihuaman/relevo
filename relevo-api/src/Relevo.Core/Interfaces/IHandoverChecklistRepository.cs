using System.Collections.Generic;

namespace Relevo.Core.Interfaces;

public interface IHandoverChecklistRepository
{
    IReadOnlyList<HandoverChecklistItemRecord> GetHandoverChecklists(string handoverId);
    bool UpdateChecklistItem(string handoverId, string itemId, bool isChecked, string userId);
}
