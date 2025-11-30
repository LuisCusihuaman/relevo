using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Handovers.GetById;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Handovers.GetById;

public class GetHandoverByIdHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly GetHandoverByIdHandler _handler;

    public GetHandoverByIdHandlerHandle()
    {
        _handler = new GetHandoverByIdHandler(_repository);
    }

    [Fact]
    public async Task ReturnsHandoverGivenValidId()
    {
        var handoverId = "hvo-1";
        var handover = new HandoverRecord(handoverId, "asn-1", "pat-1", "Test Patient", "Draft",
            new HandoverIllnessSeverity("Stable"), new HandoverPatientSummary("Summary"), null, null,
            "Day", "dr-1", "dr-2", null, null, null, "dr-1", "Dr. One",
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Draft", 1);
        
        var actionItems = new List<ActionItemRecord>
        {
            new ActionItemRecord("item-1", "Do something", false)
        };

        var detail = new HandoverDetailRecord(handover, actionItems);

        _repository.GetHandoverByIdAsync(handoverId)
            .Returns(Task.FromResult<HandoverDetailRecord?>(detail));

        var result = await _handler.Handle(new GetHandoverByIdQuery(handoverId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Handover.Id.Should().Be(handoverId);
        result.Value.ActionItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task ReturnsNotFoundGivenInvalidId()
    {
        var handoverId = "invalid-id";
        _repository.GetHandoverByIdAsync(handoverId)
            .Returns(Task.FromResult<HandoverDetailRecord?>(null));

        var result = await _handler.Handle(new GetHandoverByIdQuery(handoverId), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }
}

