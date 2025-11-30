using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Handovers.GetClinicalData;
using Relevo.UseCases.Handovers.UpdateClinicalData;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Ardalis.Result;

namespace Relevo.UnitTests.UseCases.Handovers.ClinicalData;

public class HandoverClinicalDataHandlersTests
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly GetHandoverClinicalDataHandler _getHandler;
    private readonly UpdateHandoverClinicalDataHandler _updateHandler;

    public HandoverClinicalDataHandlersTests()
    {
        _getHandler = new GetHandoverClinicalDataHandler(_repository);
        _updateHandler = new UpdateHandoverClinicalDataHandler(_repository);
    }

    [Fact]
    public async Task Get_ReturnsData()
    {
        var handoverId = "hvo-1";
        var data = new HandoverClinicalDataRecord(handoverId, "Unstable", "Summary", "dr-1", "draft", DateTime.UtcNow, DateTime.UtcNow);

        _repository.GetClinicalDataAsync(handoverId)
            .Returns(Task.FromResult<HandoverClinicalDataRecord?>(data));

        var result = await _getHandler.Handle(new GetHandoverClinicalDataQuery(handoverId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenNull()
    {
        var handoverId = "hvo-1";
        _repository.GetClinicalDataAsync(handoverId)
            .Returns(Task.FromResult<HandoverClinicalDataRecord?>(null));

        var result = await _getHandler.Handle(new GetHandoverClinicalDataQuery(handoverId), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Update_ReturnsSuccess()
    {
        var cmd = new UpdateHandoverClinicalDataCommand("hvo-1", "Stable", "New Summary", "dr-1");
        _repository.UpdateClinicalDataAsync(cmd.HandoverId, cmd.IllnessSeverity, cmd.SummaryText, cmd.UserId)
            .Returns(Task.FromResult(true));

        var result = await _updateHandler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}

