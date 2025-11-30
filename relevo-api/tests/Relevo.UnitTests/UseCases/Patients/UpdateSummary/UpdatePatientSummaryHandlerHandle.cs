using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Patients.UpdateSummary;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Patients.UpdateSummary;

public class UpdatePatientSummaryHandlerHandle
{
    private readonly IPatientRepository _repository = Substitute.For<IPatientRepository>();
    private readonly UpdatePatientSummaryHandler _handler;

    public UpdatePatientSummaryHandlerHandle()
    {
        _handler = new UpdatePatientSummaryHandler(_repository);
    }

    [Fact]
    public async Task ReturnsSuccessGivenValidRequest()
    {
        var patientId = "pat-1";
        var summaryId = "sum-1";
        var userId = "dr-1";
        var existingSummary = new PatientSummaryRecord(summaryId, patientId, userId, "Old", DateTime.UtcNow, DateTime.UtcNow, userId);

        _repository.GetPatientSummaryAsync(patientId)
            .Returns(Task.FromResult<PatientSummaryRecord?>(existingSummary));
        
        _repository.UpdatePatientSummaryAsync(summaryId, "New", userId)
            .Returns(Task.FromResult(true));

        var command = new UpdatePatientSummaryCommand(patientId, "New", userId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnsNotFoundWhenSummaryDoesNotExist()
    {
        var patientId = "pat-1";
        _repository.GetPatientSummaryAsync(patientId)
            .Returns(Task.FromResult<PatientSummaryRecord?>(null));

        var command = new UpdatePatientSummaryCommand(patientId, "New", "dr-1");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }
}

