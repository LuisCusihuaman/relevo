using System;
using System.Threading.Tasks;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class CreateHandoverForAssignmentUseCase
{
    private readonly IHandoverRepository _handoverRepository;
    private readonly IUserRepository _userRepository;

    public CreateHandoverForAssignmentUseCase(IHandoverRepository handoverRepository, IUserRepository userRepository)
    {
        _handoverRepository = handoverRepository;
        _userRepository = userRepository;
    }

    public async Task ExecuteAsync(string assignmentId, string userId, string userName, DateTime windowDate, string fromShiftId, string toShiftId)
    {
        // Orchestrate user provisioning before creating handover
        _userRepository.EnsureUserExists(userId, null, null, null, null);

        // Proceed with handover creation
        await _handoverRepository.CreateHandoverForAssignmentAsync(assignmentId, userId, userName, windowDate, fromShiftId, toShiftId);
    }
}
