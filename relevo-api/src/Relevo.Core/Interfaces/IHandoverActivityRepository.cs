using System.Collections.Generic;

namespace Relevo.Core.Interfaces;

public interface IHandoverActivityRepository
{
    IReadOnlyList<HandoverActivityItemRecord> GetHandoverActivityLog(string handoverId);
}
