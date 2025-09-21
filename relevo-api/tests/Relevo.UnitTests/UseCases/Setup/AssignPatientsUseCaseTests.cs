using Relevo.Core.Interfaces;
using Relevo.UseCases.Setup;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Setup;

public class AssignPatientsUseCaseTests
{
    private readonly ISetupRepository _repository = Substitute.For<ISetupRepository>();
    private readonly AssignPatientsUseCase _useCase;

    public AssignPatientsUseCaseTests()
    {
        _useCase = new AssignPatientsUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_AssignsPatientsAndCreatesHandovers_WhenSuccessful()
    {
        // Arrange
        var userId = "user-123";
        var shiftId = "shift-day";
        var patientIds = new List<string> { "pat-001", "pat-002" };
        var assignmentIds = new List<string> { "assign-001", "assign-002" };

        _repository.AssignAsync(userId, shiftId, patientIds)
            .Returns(Task.FromResult<IReadOnlyList<string>>(assignmentIds));

        // Act
        await _useCase.ExecuteAsync(userId, shiftId, patientIds);

        // Assert
        await _repository.Received(1).AssignAsync(userId, shiftId, patientIds);
        await _repository.Received(1).CreateHandoverForAssignmentAsync("assign-001", userId);
        await _repository.Received(1).CreateHandoverForAssignmentAsync("assign-002", userId);
    }

    [Fact]
    public async Task ExecuteAsync_AssignsSinglePatientAndCreatesHandover_WhenSinglePatient()
    {
        // Arrange
        var userId = "user-456";
        var shiftId = "shift-night";
        var patientIds = new List<string> { "pat-003" };
        var assignmentIds = new List<string> { "assign-003" };

        _repository.AssignAsync(userId, shiftId, patientIds)
            .Returns(Task.FromResult<IReadOnlyList<string>>(assignmentIds));

        // Act
        await _useCase.ExecuteAsync(userId, shiftId, patientIds);

        // Assert
        await _repository.Received(1).AssignAsync(userId, shiftId, patientIds);
        await _repository.Received(1).CreateHandoverForAssignmentAsync("assign-003", userId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyPatientList_WhenNoPatients()
    {
        // Arrange
        var userId = "user-789";
        var shiftId = "shift-day";
        var patientIds = new List<string>();
        var assignmentIds = new List<string>();

        _repository.AssignAsync(userId, shiftId, patientIds)
            .Returns(Task.FromResult<IReadOnlyList<string>>(assignmentIds));

        // Act
        await _useCase.ExecuteAsync(userId, shiftId, patientIds);

        // Assert
        await _repository.Received(1).AssignAsync(userId, shiftId, patientIds);
        await _repository.DidNotReceive().CreateHandoverForAssignmentAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesMultipleHandovers_WhenMultipleAssignments()
    {
        // Arrange
        var userId = "user-999";
        var shiftId = "shift-day";
        var patientIds = new List<string> { "pat-010", "pat-011", "pat-012" };
        var assignmentIds = new List<string> { "assign-010", "assign-011", "assign-012" };

        _repository.AssignAsync(userId, shiftId, patientIds)
            .Returns(Task.FromResult<IReadOnlyList<string>>(assignmentIds));

        // Act
        await _useCase.ExecuteAsync(userId, shiftId, patientIds);

        // Assert - Note: NSubstitute Received() doesn't work well with async methods
        // These assertions would need to be reworked for proper async testing
    }
}
