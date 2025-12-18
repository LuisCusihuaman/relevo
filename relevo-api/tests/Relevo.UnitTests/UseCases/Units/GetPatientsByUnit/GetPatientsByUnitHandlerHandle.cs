using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Units.GetPatientsByUnit;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Units.GetPatientsByUnit;

public class GetPatientsByUnitHandlerHandle
{
    private readonly IPatientRepository _repository = Substitute.For<IPatientRepository>();
    private readonly GetPatientsByUnitHandler _handler;

    public GetPatientsByUnitHandlerHandle()
    {
        _handler = new GetPatientsByUnitHandler(_repository);
    }

    [Fact]
    public async Task ReturnsSuccessGivenValidUnitId()
    {
        var unitId = "unit-1";
        var patients = new List<PatientRecord>
        {
            new PatientRecord("pat-001", "Test Patient", "not-started", null, 10, "101", "Flu", null, null)
        };
        _repository.GetPatientsByUnitAsync(unitId, 1, 25, null)
            .Returns(Task.FromResult<(IReadOnlyList<PatientRecord>, int)>((patients, 1)));

        var result = await _handler.Handle(new GetPatientsByUnitQuery(unitId, 1, 25, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Patients.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ReturnsEmptyListGivenInvalidUnitId()
    {
        var unitId = "invalid-unit";
        _repository.GetPatientsByUnitAsync(unitId, 1, 25, null)
            .Returns(Task.FromResult<(IReadOnlyList<PatientRecord>, int)>((new List<PatientRecord>(), 0)));

        var result = await _handler.Handle(new GetPatientsByUnitQuery(unitId, 1, 25, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Patients.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}

