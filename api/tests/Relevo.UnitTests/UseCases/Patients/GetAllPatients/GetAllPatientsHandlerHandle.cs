using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Patients.GetAllPatients;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Patients.GetAllPatients;

public class GetAllPatientsHandlerHandle
{
    private readonly IPatientRepository _repository = Substitute.For<IPatientRepository>();
    private readonly GetAllPatientsHandler _handler;

    public GetAllPatientsHandlerHandle()
    {
        _handler = new GetAllPatientsHandler(_repository);
    }

    [Fact]
    public async Task ReturnsPatients()
    {
        var patients = new List<PatientRecord>
        {
            new PatientRecord("pat-001", "Test Patient", "not-started", null, 10, "101", "Flu", null, null)
        };
        _repository.GetAllPatientsAsync(1, 25)
            .Returns(Task.FromResult<(IReadOnlyList<PatientRecord>, int)>((patients, 1)));

        var result = await _handler.Handle(new GetAllPatientsQuery(1, 25), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Patients.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ReturnsEmptyListWhenNoPatients()
    {
        _repository.GetAllPatientsAsync(1, 25)
            .Returns(Task.FromResult<(IReadOnlyList<PatientRecord>, int)>((new List<PatientRecord>(), 0)));

        var result = await _handler.Handle(new GetAllPatientsQuery(1, 25), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Patients.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}

