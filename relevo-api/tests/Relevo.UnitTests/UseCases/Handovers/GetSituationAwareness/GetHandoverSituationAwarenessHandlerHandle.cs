using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Handovers.GetSituationAwareness;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Handovers.GetSituationAwareness;

public class GetHandoverSituationAwarenessHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly GetHandoverSituationAwarenessHandler _handler;

    public GetHandoverSituationAwarenessHandlerHandle()
    {
        _handler = new GetHandoverSituationAwarenessHandler(_repository);
    }

    [Fact]
    public async Task ReturnsSituationAwareness()
    {
        var handoverId = "hvo-1";
        var sa = new HandoverSituationAwarenessRecord(handoverId, "Content", "Draft", "dr-1", DateTime.UtcNow, DateTime.UtcNow);

        _repository.GetSituationAwarenessAsync(handoverId)
            .Returns(Task.FromResult<HandoverSituationAwarenessRecord?>(sa));

        var result = await _handler.Handle(new GetHandoverSituationAwarenessQuery(handoverId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(sa);
    }

    [Fact]
    public async Task ReturnsNotFoundWhenRepositoryReturnsNull()
    {
        var handoverId = "hvo-1";
        _repository.GetSituationAwarenessAsync(handoverId)
            .Returns(Task.FromResult<HandoverSituationAwarenessRecord?>(null));

        var result = await _handler.Handle(new GetHandoverSituationAwarenessQuery(handoverId), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }
}

