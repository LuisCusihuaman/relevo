using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.UpdateSituationAwareness;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Ardalis.Result;

namespace Relevo.UnitTests.UseCases.Handovers.UpdateSituationAwareness;

public class UpdateHandoverSituationAwarenessHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly UpdateHandoverSituationAwarenessHandler _handler;

    public UpdateHandoverSituationAwarenessHandlerHandle()
    {
        _handler = new UpdateHandoverSituationAwarenessHandler(_repository);
    }

    [Fact]
    public async Task ReturnsSuccessWhenUpdateSucceeds()
    {
        var handoverId = "hvo-1";
        var content = "New SA Content";
        var status = "Final";
        var userId = "user-1";

        _repository.UpdateSituationAwarenessAsync(handoverId, content, status, userId)
            .Returns(Task.FromResult(true));

        var result = await _handler.Handle(new UpdateHandoverSituationAwarenessCommand(handoverId, content, status, userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnsErrorWhenUpdateFails()
    {
        var handoverId = "hvo-1";
        _repository.UpdateSituationAwarenessAsync(handoverId, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(false));

        var result = await _handler.Handle(new UpdateHandoverSituationAwarenessCommand(handoverId, "c", "s", "u"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Failed to update situation awareness. Handover may not exist.");
    }
}

