using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Handovers.GetSynthesis;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Handovers.GetSynthesis;

public class GetHandoverSynthesisHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly GetHandoverSynthesisHandler _handler;

    public GetHandoverSynthesisHandlerHandle()
    {
        _handler = new GetHandoverSynthesisHandler(_repository);
    }

    [Fact]
    public async Task ReturnsSynthesis()
    {
        var handoverId = "hvo-1";
        var synthesis = new HandoverSynthesisRecord(handoverId, "Content", "Draft", "dr-1", DateTime.UtcNow, DateTime.UtcNow);

        _repository.GetSynthesisAsync(handoverId)
            .Returns(Task.FromResult<HandoverSynthesisRecord?>(synthesis));

        var result = await _handler.Handle(new GetHandoverSynthesisQuery(handoverId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(synthesis);
    }

    [Fact]
    public async Task ReturnsNotFoundWhenRepositoryReturnsNull()
    {
        var handoverId = "hvo-1";
        _repository.GetSynthesisAsync(handoverId)
            .Returns(Task.FromResult<HandoverSynthesisRecord?>(null));

        var result = await _handler.Handle(new GetHandoverSynthesisQuery(handoverId), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }
}

