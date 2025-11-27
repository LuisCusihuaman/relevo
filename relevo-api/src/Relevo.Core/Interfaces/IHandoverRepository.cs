using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relevo.Core.Interfaces;

public interface IHandoverRepository
{
    (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetMyHandovers(string userId, int page, int pageSize);
    (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetPatientHandovers(string patientId, int page, int pageSize);
    HandoverRecord? GetHandoverById(string handoverId);
    Task CreateHandoverForAssignmentAsync(string assignmentId, string userId, string userName, DateTime windowDate, string fromShiftId, string toShiftId);
    Task<bool> StartHandover(string handoverId, string userId);
    Task<bool> ReadyHandover(string handoverId, string userId);
    Task<bool> AcceptHandover(string handoverId, string userId);
    Task<bool> CompleteHandover(string handoverId, string userId);
    Task<bool> CancelHandover(string handoverId, string userId);
    Task<bool> RejectHandover(string handoverId, string userId, string reason);
}
