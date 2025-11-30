using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Handovers.GetPatientData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Handovers.GetPatientData;

public class GetPatientHandoverDataHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly GetPatientHandoverDataHandler _handler;

    public GetPatientHandoverDataHandlerHandle()
    {
        _handler = new GetPatientHandoverDataHandler(_repository);
    }

    [Fact]
    public async Task ReturnsDataGivenValidId()
    {
        var handoverId = "hvo-1";
        var physician = new PhysicianRecord("Dr. One", "Doctor", "", "07:00", "15:00", "handing-off", "assigned");
        var data = new PatientHandoverDataRecord(
            "pat-1", "John Doe", "1980-01-01", "MRN123", "2023-01-01", "ICU", "Flu", "101", "ICU",
            physician, null, "Stable", "Summary", "dr-1", "2023-01-02");

        _repository.GetPatientHandoverDataAsync(handoverId)
            .Returns(Task.FromResult<PatientHandoverDataRecord?>(data));

        var result = await _handler.Handle(new GetPatientHandoverDataQuery(handoverId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be("pat-1");
        result.Value.AssignedPhysician.Should().NotBeNull();
        result.Value.AssignedPhysician!.Name.Should().Be("Dr. One");
    }

    [Fact]
    public async Task ReturnsNotFoundGivenInvalidId()
    {
        var handoverId = "invalid-id";
        _repository.GetPatientHandoverDataAsync(handoverId)
            .Returns(Task.FromResult<PatientHandoverDataRecord?>(null));

        var result = await _handler.Handle(new GetPatientHandoverDataQuery(handoverId), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }
}

