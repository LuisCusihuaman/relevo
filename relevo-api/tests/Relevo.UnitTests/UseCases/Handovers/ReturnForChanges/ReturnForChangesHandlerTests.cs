using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.ReturnForChanges;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Ardalis.Result;

namespace Relevo.UnitTests.UseCases.Handovers.ReturnForChanges;

public class ReturnForChangesHandlerTests
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly ReturnForChangesHandler _handler;

    public ReturnForChangesHandlerTests()
    {
        _handler = new ReturnForChangesHandler(_repository);
    }

    [Fact]
    public async Task ReturnsSuccess_WhenHandoverIsReady()
    {
        // Arrange
        var handoverId = "hvo-1";
        var userId = "user-1";

        _repository.ReturnForChangesAsync(handoverId, userId).Returns(Task.FromResult(true));

        // Act
        var result = await _handler.Handle(new ReturnForChangesCommand(handoverId, userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).ReturnForChangesAsync(handoverId, userId);
    }

    [Fact]
    public async Task ReturnsError_WhenHandoverIsNotReady()
    {
        // Arrange
        var handoverId = "hvo-1";
        var userId = "user-1";

        _repository.ReturnForChangesAsync(handoverId, userId).Returns(Task.FromResult(false));

        // Act
        var result = await _handler.Handle(new ReturnForChangesCommand(handoverId, userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Failed to return handover for changes. It may not exist, is not in Ready state, or is already completed/cancelled.");
    }

    [Fact]
    public async Task ReturnsError_WhenHandoverIsCompleted()
    {
        // Arrange
        var handoverId = "hvo-1";
        var userId = "user-1";

        // Repository returns false when handover is completed (READY_AT is NULL or COMPLETED_AT is NOT NULL)
        _repository.ReturnForChangesAsync(handoverId, userId).Returns(Task.FromResult(false));

        // Act
        var result = await _handler.Handle(new ReturnForChangesCommand(handoverId, userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ReturnsError_WhenHandoverIsCancelled()
    {
        // Arrange
        var handoverId = "hvo-1";
        var userId = "user-1";

        // Repository returns false when handover is cancelled (CANCELLED_AT is NOT NULL)
        _repository.ReturnForChangesAsync(handoverId, userId).Returns(Task.FromResult(false));

        // Act
        var result = await _handler.Handle(new ReturnForChangesCommand(handoverId, userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}

