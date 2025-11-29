using Relevo.Core.Models;

namespace Relevo.Core.Interfaces;

    public interface IHandoverRepository
    {
        Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> GetPatientHandoversAsync(string patientId, int page, int pageSize);
        Task<HandoverDetailRecord?> GetHandoverByIdAsync(string handoverId);
    }

