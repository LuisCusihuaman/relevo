using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.SetReady;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Handovers.SetReady;

public class ReadyHandoverHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly ReadyHandoverHandler _handler;

    public ReadyHandoverHandlerHandle()
    {
        _handler = new ReadyHandoverHandler(_repository);
    }

    [Fact]
    public async Task ReturnsSuccessWhenRepositoryReturnsTrue()
    {
        var handoverId = "hvo-1";
        var userId = "user-1";

        _repository.MarkAsReadyAsync(handoverId, userId).Returns(Task.FromResult(true));

        var result = await _handler.Handle(new ReadyHandoverCommand(handoverId, userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnsErrorWhenRepositoryReturnsFalse()
    {
        var handoverId = "hvo-1";
        var userId = "user-1";

        _repository.MarkAsReadyAsync(handoverId, userId).Returns(Task.FromResult(false));

        var result = await _handler.Handle(new ReadyHandoverCommand(handoverId, userId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Failed to mark handover as ready. It may not exist or is in an invalid state.");
    }
}

