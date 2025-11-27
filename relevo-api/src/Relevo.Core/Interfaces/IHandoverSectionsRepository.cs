using System;
using System.Threading.Tasks;

namespace Relevo.Core.Interfaces;

public interface IHandoverSectionsRepository
{
    Task<HandoverPatientDataRecord?> GetPatientDataAsync(string handoverId);
    Task<HandoverSituationAwarenessRecord?> GetSituationAwarenessAsync(string handoverId);
    Task<HandoverSynthesisRecord?> GetSynthesisAsync(string handoverId);

    Task<bool> UpdatePatientDataAsync(string handoverId, string illnessSeverity, string? summaryText, string status, string userId);
    Task<bool> UpdateSituationAwarenessAsync(string handoverId, string? content, string status, string userId);
    Task<bool> UpdateSynthesisAsync(string handoverId, string? content, string status, string userId);
}
