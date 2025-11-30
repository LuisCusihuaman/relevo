using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Handovers.DeleteContingencyPlan;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Handovers.DeleteContingencyPlan;

public class DeleteContingencyPlanHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly DeleteContingencyPlanHandler _handler;

    public DeleteContingencyPlanHandlerHandle()
    {
        _handler = new DeleteContingencyPlanHandler(_repository);
    }

    [Fact]
    public async Task ReturnsSuccessWhenDeleted()
    {
        var handoverId = "hvo-1";
        var contingencyId = "plan-1";

        _repository.DeleteContingencyPlanAsync(handoverId, contingencyId)
            .Returns(Task.FromResult(true));

        var command = new DeleteContingencyPlanCommand(handoverId, contingencyId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnsNotFoundWhenNotDeleted()
    {
        var handoverId = "hvo-1";
        var contingencyId = "plan-1";

        _repository.DeleteContingencyPlanAsync(handoverId, contingencyId)
            .Returns(Task.FromResult(false));

        var command = new DeleteContingencyPlanCommand(handoverId, contingencyId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }
}

