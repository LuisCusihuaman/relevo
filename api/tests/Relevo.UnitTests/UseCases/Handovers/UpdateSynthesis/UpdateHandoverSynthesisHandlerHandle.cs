using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.UpdateSynthesis;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Handovers.UpdateSynthesis;

public class UpdateHandoverSynthesisHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly UpdateHandoverSynthesisHandler _handler;

    public UpdateHandoverSynthesisHandlerHandle()
    {
        _handler = new UpdateHandoverSynthesisHandler(_repository);
    }

    [Fact]
    public async Task ReturnsSuccessWhenUpdated()
    {
        var handoverId = "hvo-1";
        var content = "Synthesis content";
        var status = "draft";
        var userId = "dr-1";

        _repository.UpdateSynthesisAsync(handoverId, content, status, userId)
            .Returns(Task.FromResult(true));

        var command = new UpdateHandoverSynthesisCommand(handoverId, content, status, userId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnsNotFoundWhenUpdateFails()
    {
        var handoverId = "hvo-1";
        
        _repository.UpdateSynthesisAsync(handoverId, Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(false));

        var command = new UpdateHandoverSynthesisCommand(handoverId, "content", "draft", "dr-1");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }
}

