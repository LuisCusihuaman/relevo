using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Handovers.StateMachine;
using ActionItemRecord = Relevo.Core.Models.ActionItemRecord;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;
using Ardalis.Result;

namespace Relevo.UnitTests.UseCases.Handovers.StateMachine;

public class StartHandoverHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly HandoverStateMachineHandlers _handler;

    public StartHandoverHandlerHandle()
    {
        _handler = new HandoverStateMachineHandlers(_repository, _mediator);
    }

    [Fact]
    public async Task ReturnsSuccess_WhenUserHasCoverageAndIsNotSender()
    {
        // Arrange
        var handoverId = "hvo-1";
        var userId = "user-receiver";
        var senderUserId = "user-sender";

        var handoverDetail = new HandoverDetailRecord(
            new HandoverRecord(
                handoverId, "pat-1", "Patient 1", "Ready", "Stable", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Ready", 1, null, null, senderUserId, null, null, null, null, null),
            new List<ActionItemRecord>()
        );

        _repository.HasCoverageInToShiftAsync(handoverId, userId).Returns(Task.FromResult(true));
        _repository.GetHandoverByIdAsync(handoverId).Returns(Task.FromResult<HandoverDetailRecord?>(handoverDetail));
        _repository.StartHandoverAsync(handoverId, userId).Returns(Task.FromResult(true));

        // Act
        var result = await _handler.Handle(new StartHandoverCommand(handoverId, userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).HasCoverageInToShiftAsync(handoverId, userId);
        await _repository.Received(1).GetHandoverByIdAsync(handoverId);
        await _repository.Received(1).StartHandoverAsync(handoverId, userId);
    }

    [Fact]
    public async Task ReturnsError_WhenUserDoesNotHaveCoverage()
    {
        // Arrange
        var handoverId = "hvo-1";
        var userId = "user-no-coverage";

        _repository.HasCoverageInToShiftAsync(handoverId, userId).Returns(Task.FromResult(false));

        // Act
        var result = await _handler.Handle(new StartHandoverCommand(handoverId, userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Cannot start handover: user must have coverage in the TO shift.");
        await _repository.Received(1).HasCoverageInToShiftAsync(handoverId, userId);
        await _repository.DidNotReceive().StartHandoverAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ReturnsError_WhenUserIsSender()
    {
        // Arrange
        var handoverId = "hvo-1";
        var userId = "user-sender"; // Same as sender

        var handoverDetail = new HandoverDetailRecord(
            new HandoverRecord(
                handoverId, "pat-1", "Patient 1", "Ready", "Stable", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Ready", 1, null, null, userId, null, null, null, null, null),
            new List<ActionItemRecord>()
        );

        _repository.HasCoverageInToShiftAsync(handoverId, userId).Returns(Task.FromResult(true));
        _repository.GetHandoverByIdAsync(handoverId).Returns(Task.FromResult<HandoverDetailRecord?>(handoverDetail));

        // Act
        var result = await _handler.Handle(new StartHandoverCommand(handoverId, userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Cannot start handover: sender cannot start the handover.");
        await _repository.Received(1).HasCoverageInToShiftAsync(handoverId, userId);
        await _repository.Received(1).GetHandoverByIdAsync(handoverId);
        await _repository.DidNotReceive().StartHandoverAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ReturnsError_WhenRepositoryStartFails()
    {
        // Arrange
        var handoverId = "hvo-1";
        var userId = "user-receiver";
        var senderUserId = "user-sender";

        var handoverDetail = new HandoverDetailRecord(
            new HandoverRecord(
                handoverId, "pat-1", "Patient 1", "Ready", "Stable", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Ready", 1, null, null, senderUserId, null, null, null, null, null),
            new List<ActionItemRecord>()
        );

        _repository.HasCoverageInToShiftAsync(handoverId, userId).Returns(Task.FromResult(true));
        _repository.GetHandoverByIdAsync(handoverId).Returns(Task.FromResult<HandoverDetailRecord?>(handoverDetail));
        _repository.StartHandoverAsync(handoverId, userId).Returns(Task.FromResult(false));

        // Act
        var result = await _handler.Handle(new StartHandoverCommand(handoverId, userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Failed to update handover state.");
    }
}

