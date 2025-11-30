using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Patients.GetById;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Patients.GetById;

public class GetPatientByIdHandlerHandle
{
    private readonly IPatientRepository _repository = Substitute.For<IPatientRepository>();
    private readonly GetPatientByIdHandler _handler;

    public GetPatientByIdHandlerHandle()
    {
        _handler = new GetPatientByIdHandler(_repository);
    }

    [Fact]
    public async Task ReturnsPatientGivenValidId()
    {
        var patientId = "pat-001";
        var patient = new PatientDetailRecord(
            patientId, "Test Patient", "MRN123", "2010-01-01", "Male", "2023-01-01", 
            "ICU", "101", "Flu", new List<string>(), new List<string>(), "Notes");

        _repository.GetPatientByIdAsync(patientId)
            .Returns(Task.FromResult<PatientDetailRecord?>(patient));

        var result = await _handler.Handle(new GetPatientByIdQuery(patientId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(patientId);
    }

    [Fact]
    public async Task ReturnsNotFoundGivenInvalidId()
    {
        var patientId = "invalid-id";
        _repository.GetPatientByIdAsync(patientId)
            .Returns(Task.FromResult<PatientDetailRecord?>(null));

        var result = await _handler.Handle(new GetPatientByIdQuery(patientId), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }
}

