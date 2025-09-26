using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class AssignPatientsUseCase
{
    private readonly ISetupRepository _repository;
    private readonly IShiftBoundaryResolver _shiftBoundaryResolver;

    public AssignPatientsUseCase(ISetupRepository repository, IShiftBoundaryResolver shiftBoundaryResolver)
    {
        _repository = repository;
        _shiftBoundaryResolver = shiftBoundaryResolver;
    }

    public async Task ExecuteAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        // First, assign patients to the user for the shift
        var assignmentIds = await _repository.AssignAsync(userId, shiftId, patientIds);

        Console.WriteLine($"🚀 AssignPatients - User: {userId}, Shift: {shiftId}, Patients: {string.Join(",", patientIds)}, AssignmentIds: {string.Join(",", assignmentIds)}");

        // Resolve handover window. It's the same for all assignments in this batch.
        var (windowDate, toShiftId) = _shiftBoundaryResolver.Resolve(DateTime.Now, shiftId);

        Console.WriteLine($"🔄 Shift Boundary - WindowDate: {windowDate}, ToShiftId: {toShiftId}");

        // Then, create handovers for each assignment
        foreach (var assignmentId in assignmentIds)
        {
            Console.WriteLine($"📋 Creating handover for assignment: {assignmentId}");
            await _repository.CreateHandoverForAssignmentAsync(assignmentId, userId, windowDate, shiftId, toShiftId);
        }
    }
}
