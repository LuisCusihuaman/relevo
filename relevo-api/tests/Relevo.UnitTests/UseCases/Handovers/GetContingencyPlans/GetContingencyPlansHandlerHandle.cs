using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Handovers.GetContingencyPlans;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Handovers.GetContingencyPlans;

public class GetContingencyPlansHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly GetContingencyPlansHandler _handler;

    public GetContingencyPlansHandlerHandle()
    {
        _handler = new GetContingencyPlansHandler(_repository);
    }

    [Fact]
    public async Task ReturnsPlans()
    {
        var handoverId = "hvo-1";
        var plans = new List<ContingencyPlanRecord>
        {
            new ContingencyPlanRecord("plan-1", handoverId, "If stable", "Discharge", "Low", "active", "dr-1", DateTime.UtcNow, DateTime.UtcNow)
        };

        _repository.GetContingencyPlansAsync(handoverId)
            .Returns(Task.FromResult<IReadOnlyList<ContingencyPlanRecord>>(plans));

        var result = await _handler.Handle(new GetContingencyPlansQuery(handoverId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be("plan-1");
    }
}

