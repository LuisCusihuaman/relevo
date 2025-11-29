using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Patients.GetSummary;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Patients.GetSummary;

public class GetPatientSummaryHandlerHandle
{
    private readonly IPatientRepository _repository = Substitute.For<IPatientRepository>();
    private readonly GetPatientSummaryHandler _handler;

    public GetPatientSummaryHandlerHandle()
    {
        _handler = new GetPatientSummaryHandler(_repository);
    }

    [Fact]
    public async Task ReturnsSummaryGivenValidId()
    {
        var patientId = "pat-1";
        var summary = new PatientSummaryRecord(
            "sum-1", patientId, "dr-1", "Summary text", DateTime.UtcNow, DateTime.UtcNow, "dr-1"
        );

        _repository.GetPatientSummaryAsync(patientId)
            .Returns(Task.FromResult<PatientSummaryRecord?>(summary));

        var result = await _handler.Handle(new GetPatientSummaryQuery(patientId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(summary);
    }

    [Fact]
    public async Task ReturnsNotFoundWhenNoSummary()
    {
        var patientId = "no-summary";
        _repository.GetPatientSummaryAsync(patientId)
            .Returns(Task.FromResult<PatientSummaryRecord?>(null));

        var result = await _handler.Handle(new GetPatientSummaryQuery(patientId), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }
}

