using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.ShiftCheckIn.GetUnits;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.ShiftCheckIn.GetUnits;

public class GetUnitsHandlerHandle
{
    private readonly IUnitRepository _repository = Substitute.For<IUnitRepository>();
    private readonly GetUnitsHandler _handler;

    public GetUnitsHandlerHandle()
    {
        _handler = new GetUnitsHandler(_repository);
    }

    [Fact]
    public async Task ReturnsUnits()
    {
        var units = new List<UnitRecord>
        {
            new UnitRecord("unit-1", "ICU"),
            new UnitRecord("unit-2", "ER")
        };
        _repository.GetUnitsAsync()
            .Returns(Task.FromResult<IReadOnlyList<UnitRecord>>(units));

        var result = await _handler.Handle(new GetUnitsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Units.Should().HaveCount(2);
        result.Value.Units[0].Id.Should().Be("unit-1");
    }

    [Fact]
    public async Task ReturnsEmptyListWhenNoUnits()
    {
        _repository.GetUnitsAsync()
            .Returns(Task.FromResult<IReadOnlyList<UnitRecord>>(new List<UnitRecord>()));

        var result = await _handler.Handle(new GetUnitsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Units.Should().BeEmpty();
    }
}

