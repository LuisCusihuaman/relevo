using System.Collections.Generic;

namespace Relevo.Core.Interfaces;

public interface IHandoverMessagingRepository
{
    IReadOnlyList<HandoverMessageRecord> GetHandoverMessages(string handoverId);
    HandoverMessageRecord CreateHandoverMessage(string handoverId, string userId, string userName, string messageText, string messageType);
}
