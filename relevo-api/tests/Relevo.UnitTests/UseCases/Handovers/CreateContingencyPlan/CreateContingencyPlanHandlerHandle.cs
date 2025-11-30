using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Handovers.CreateContingencyPlan;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Handovers.CreateContingencyPlan;

public class CreateContingencyPlanHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly CreateContingencyPlanHandler _handler;

    public CreateContingencyPlanHandlerHandle()
    {
        _handler = new CreateContingencyPlanHandler(_repository);
    }

    [Fact]
    public async Task ReturnsCreatedPlan()
    {
        var handoverId = "hvo-1";
        var condition = "If X";
        var action = "Do Y";
        var priority = "High";
        var userId = "dr-1";
        
        var plan = new ContingencyPlanRecord("plan-1", handoverId, condition, action, priority, "active", userId, DateTime.UtcNow, DateTime.UtcNow);

        _repository.CreateContingencyPlanAsync(handoverId, condition, action, priority, userId)
            .Returns(Task.FromResult(plan));

        var command = new CreateContingencyPlanCommand(handoverId, condition, action, priority, userId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(plan);
    }
}

