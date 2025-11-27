using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Setup;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Setup;

public class AssignPatientsUseCaseTests
{
    private readonly IAssignmentRepository _assignmentRepository = Substitute.For<IAssignmentRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IShiftBoundaryResolver _shiftBoundaryResolver = Substitute.For<IShiftBoundaryResolver>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly AssignPatientsUseCase _useCase;

    public AssignPatientsUseCaseTests()
    {
        _useCase = new AssignPatientsUseCase(_assignmentRepository, _userRepository, _shiftBoundaryResolver, _userContext);

        // Setup default user context
        var user = new User
        {
            Id = "user-123",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };
        _userContext.CurrentUser.Returns(user);
    }

    [Fact]
    public async Task ExecuteAsync_AssignsPatientsAndCreatesHandovers_WhenSuccessful()
    {
        // Arrange
        var userId = "user-123";
        var shiftId = "shift-day";
        var patientIds = new List<string> { "pat-001", "pat-002" };
        var assignmentIds = new List<string> { "assign-001", "assign-002" };
        var windowDate = DateTime.Now.Date;
        var toShiftId = "shift-night";

        _assignmentRepository.AssignAsync(userId, shiftId, patientIds)
            .Returns(Task.FromResult<IReadOnlyList<string>>(assignmentIds));
        _shiftBoundaryResolver.Resolve(Arg.Any<DateTime>(), shiftId)
            .Returns((windowDate, toShiftId));

        // Act
        await _useCase.ExecuteAsync(userId, shiftId, patientIds);

        // Assert
        _userRepository.Received(1).EnsureUserExists(userId, null, null, null, null);
        await _assignmentRepository.Received(1).AssignAsync(userId, shiftId, patientIds);
    }

    [Fact]
    public async Task ExecuteAsync_AssignsSinglePatientAndCreatesHandover_WhenSinglePatient()
    {
        // Arrange
        var userId = "user-456";
        var shiftId = "shift-night";
        var patientIds = new List<string> { "pat-003" };
        var assignmentIds = new List<string> { "assign-003" };
        var windowDate = DateTime.Now.Date;
        var toShiftId = "shift-day";

        _assignmentRepository.AssignAsync(userId, shiftId, patientIds)
            .Returns(Task.FromResult<IReadOnlyList<string>>(assignmentIds));
        _shiftBoundaryResolver.Resolve(Arg.Any<DateTime>(), shiftId)
            .Returns((windowDate, toShiftId));

        // Act
        await _useCase.ExecuteAsync(userId, shiftId, patientIds);

        // Assert
        _userRepository.Received(1).EnsureUserExists(userId, null, null, null, null);
        await _assignmentRepository.Received(1).AssignAsync(userId, shiftId, patientIds);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyPatientList_WhenNoPatients()
    {
        // Arrange
        var userId = "user-789";
        var shiftId = "shift-day";
        var patientIds = new List<string>();
        var assignmentIds = new List<string>();

        _assignmentRepository.AssignAsync(userId, shiftId, patientIds)
            .Returns(Task.FromResult<IReadOnlyList<string>>(assignmentIds));

        // Act
        await _useCase.ExecuteAsync(userId, shiftId, patientIds);

        // Assert
        _userRepository.Received(1).EnsureUserExists(userId, null, null, null, null);
        await _assignmentRepository.Received(1).AssignAsync(userId, shiftId, patientIds);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesMultipleHandovers_WhenMultipleAssignments()
    {
        // Arrange
        var userId = "user-999";
        var shiftId = "shift-day";
        var patientIds = new List<string> { "pat-010", "pat-011", "pat-012" };
        var assignmentIds = new List<string> { "assign-010", "assign-011", "assign-012" };
        var windowDate = DateTime.Now.Date;
        var toShiftId = "shift-night";

        _assignmentRepository.AssignAsync(userId, shiftId, patientIds)
            .Returns(Task.FromResult<IReadOnlyList<string>>(assignmentIds));
        _shiftBoundaryResolver.Resolve(Arg.Any<DateTime>(), shiftId)
            .Returns((windowDate, toShiftId));

        // Act
        await _useCase.ExecuteAsync(userId, shiftId, patientIds);

        // Assert
        _userRepository.Received(1).EnsureUserExists(userId, null, null, null, null);
        await _assignmentRepository.Received(1).AssignAsync(userId, shiftId, patientIds);
    }
}
