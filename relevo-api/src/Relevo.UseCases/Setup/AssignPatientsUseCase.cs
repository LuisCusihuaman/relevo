using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class AssignPatientsUseCase
{
    private readonly ISetupRepository _repository;
    private readonly IShiftBoundaryResolver _shiftBoundaryResolver;
    private readonly IUserContext _userContext;

    public AssignPatientsUseCase(ISetupRepository repository, IShiftBoundaryResolver shiftBoundaryResolver, IUserContext userContext)
    {
        _repository = repository;
        _shiftBoundaryResolver = shiftBoundaryResolver;
        _userContext = userContext;
    }

    public async Task ExecuteAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        // First, assign patients to the user for the shift
        var assignmentIds = await _repository.AssignAsync(userId, shiftId, patientIds);

        Console.WriteLine($"🚀 AssignPatients - User: {userId}, Shift: {shiftId}, Patients: {string.Join(",", patientIds)}, AssignmentIds: {string.Join(",", assignmentIds)}");

        // Resolve handover window. It's the same for all assignments in this batch.
        var (windowDate, toShiftId) = _shiftBoundaryResolver.Resolve(DateTime.Now, shiftId);

        Console.WriteLine($"🔄 Shift Boundary - WindowDate: {windowDate}, ToShiftId: {toShiftId}");

        // Get current user name for handover creation
        var currentUser = _userContext.CurrentUser;
        var userName = currentUser?.FullName ?? userId; // Fallback to userId if name not available

        // Then, create handovers for each assignment
        foreach (var assignmentId in assignmentIds)
        {
            Console.WriteLine($"📋 Creating handover for assignment: {assignmentId}, User: {userName}");
            await _repository.CreateHandoverForAssignmentAsync(assignmentId, userId, userName, windowDate, shiftId, toShiftId);
        }
    }
}
