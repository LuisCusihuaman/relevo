using System.Collections.Generic;
using System.Threading.Tasks;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.ShiftCheckIn;

public class AssignPatientsUseCase
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IShiftBoundaryResolver _shiftBoundaryResolver;
    private readonly IUserContext _userContext;

    public AssignPatientsUseCase(IAssignmentRepository assignmentRepository, IUserRepository userRepository, IShiftBoundaryResolver shiftBoundaryResolver, IUserContext userContext)
    {
        _assignmentRepository = assignmentRepository;
        _userRepository = userRepository;
        _shiftBoundaryResolver = shiftBoundaryResolver;
        _userContext = userContext;
    }

    public async Task ExecuteAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        // Orchestrate user provisioning before assignment
        _userRepository.EnsureUserExists(userId, null, null, null, null);

        // Proceed with assignment
        await _assignmentRepository.AssignAsync(userId, shiftId, patientIds);
    }
}
