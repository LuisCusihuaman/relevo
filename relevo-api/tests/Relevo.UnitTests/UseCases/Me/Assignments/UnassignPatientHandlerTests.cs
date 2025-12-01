using Relevo.Core.Interfaces;
using Relevo.UseCases.Me.Assignments;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Ardalis.Result;

namespace Relevo.UnitTests.UseCases.Me.Assignments;

public class UnassignPatientHandlerTests
{
    private readonly IAssignmentRepository _repository = Substitute.For<IAssignmentRepository>();
    private readonly UnassignPatientHandler _handler;

    public UnassignPatientHandlerTests()
    {
        _handler = new UnassignPatientHandler(_repository);
    }

    [Fact]
    public async Task ReturnsSuccess_WhenValid()
    {
        // Arrange
        var userId = "user-1";
        var shiftInstanceId = "si-1";
        var patientId = "pat-1";

        _repository.UnassignPatientAsync(userId, shiftInstanceId, patientId).Returns(Task.FromResult(true));

        // Act
        var result = await _handler.Handle(
            new UnassignPatientCommand(userId, shiftInstanceId, patientId), 
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).UnassignPatientAsync(userId, shiftInstanceId, patientId);
    }

    [Fact]
    public async Task ReturnsError_WhenAssignmentDoesNotExist()
    {
        // Arrange
        var userId = "user-1";
        var shiftInstanceId = "si-1";
        var patientId = "pat-1";

        _repository.UnassignPatientAsync(userId, shiftInstanceId, patientId).Returns(Task.FromResult(false));

        // Act
        var result = await _handler.Handle(
            new UnassignPatientCommand(userId, shiftInstanceId, patientId), 
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Failed to unassign patient. The assignment may not exist.");
    }
}

