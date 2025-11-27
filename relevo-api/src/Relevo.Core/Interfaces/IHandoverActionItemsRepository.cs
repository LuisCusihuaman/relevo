using System.Collections.Generic;

namespace Relevo.Core.Interfaces;

public interface IHandoverActionItemsRepository
{
    IReadOnlyList<HandoverActionItemRecord> GetHandoverActionItems(string handoverId);
    string CreateHandoverActionItem(string handoverId, string description, string priority);
    bool UpdateHandoverActionItem(string handoverId, string itemId, bool isCompleted);
    bool DeleteHandoverActionItem(string handoverId, string itemId);
}
