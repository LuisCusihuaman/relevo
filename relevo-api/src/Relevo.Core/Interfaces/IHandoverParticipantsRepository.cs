using System.Collections.Generic;

namespace Relevo.Core.Interfaces;

public interface IHandoverParticipantsRepository
{
    IReadOnlyList<HandoverParticipantRecord> GetHandoverParticipants(string handoverId);
}
