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
    private readonly IHandoverRepository _handoverRepository = Substitute.For<IHandoverRepository>();
    private readonly IPatientRepository _patientRepository = Substitute.For<IPatientRepository>();
    private readonly GetPatientSummaryHandler _handler;

    public GetPatientSummaryHandlerHandle()
    {
        _handler = new GetPatientSummaryHandler(_handoverRepository, _patientRepository);
    }

    [Fact]
    public async Task ReturnsSummaryGivenValidId()
    {
        var patientId = "pat-1";
        var handoverId = "handover-1";
        var userId = "dr-1";
        var summary = new PatientSummaryRecord(
            handoverId, patientId, userId, "Summary text", DateTime.UtcNow, DateTime.UtcNow, userId
        );

        _handoverRepository.GetCurrentHandoverIdAsync(patientId)
            .Returns(Task.FromResult<string?>(handoverId));
        _patientRepository.GetPatientSummaryFromHandoverAsync(handoverId)
            .Returns(Task.FromResult<PatientSummaryRecord?>(summary));

        var result = await _handler.Handle(new GetPatientSummaryQuery(patientId, userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(summary);
    }

    [Fact]
    public async Task ReturnsNotFoundWhenNoHandover()
    {
        var patientId = "no-handover";
        var userId = "dr-1";
        
        _handoverRepository.GetCurrentHandoverIdAsync(patientId)
            .Returns(Task.FromResult<string?>(null));

        var result = await _handler.Handle(new GetPatientSummaryQuery(patientId, userId), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }
}

