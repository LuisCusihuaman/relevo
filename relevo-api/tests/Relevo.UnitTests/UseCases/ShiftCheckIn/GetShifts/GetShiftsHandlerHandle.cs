using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.ShiftCheckIn.GetShifts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.ShiftCheckIn.GetShifts;

public class GetShiftsHandlerHandle
{
    private readonly IShiftRepository _repository = Substitute.For<IShiftRepository>();
    private readonly GetShiftsHandler _handler;

    public GetShiftsHandlerHandle()
    {
        _handler = new GetShiftsHandler(_repository);
    }

    [Fact]
    public async Task ReturnsShifts()
    {
        var shifts = new List<ShiftRecord>
        {
            new ShiftRecord("shift-1", "Morning", "07:00", "15:00"),
            new ShiftRecord("shift-2", "Evening", "15:00", "23:00")
        };
        _repository.GetShiftsAsync()
            .Returns(Task.FromResult<IReadOnlyList<ShiftRecord>>(shifts));

        var result = await _handler.Handle(new GetShiftsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Shifts.Should().HaveCount(2);
        result.Value.Shifts[0].Id.Should().Be("shift-1");
    }

    [Fact]
    public async Task ReturnsEmptyListWhenNoShifts()
    {
        _repository.GetShiftsAsync()
            .Returns(Task.FromResult<IReadOnlyList<ShiftRecord>>(new List<ShiftRecord>()));

        var result = await _handler.Handle(new GetShiftsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Shifts.Should().BeEmpty();
    }
}

