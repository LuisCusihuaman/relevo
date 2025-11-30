using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Patients.CreateSummary;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Patients.CreateSummary;

public class CreatePatientSummaryHandlerHandle
{
    private readonly IPatientRepository _repository = Substitute.For<IPatientRepository>();
    private readonly CreatePatientSummaryHandler _handler;

    public CreatePatientSummaryHandlerHandle()
    {
        _handler = new CreatePatientSummaryHandler(_repository);
    }

    [Fact]
    public async Task ReturnsSummaryWhenCreatedSuccessfully()
    {
        var patientId = "pat-1";
        var userId = "dr-1";
        var summaryText = "New Summary";
        var summaryRecord = new PatientSummaryRecord(
            "sum-1", patientId, userId, summaryText, DateTime.UtcNow, DateTime.UtcNow, userId
        );

        _repository.CreatePatientSummaryAsync(patientId, userId, summaryText, userId)
            .Returns(Task.FromResult(summaryRecord));

        var command = new CreatePatientSummaryCommand(patientId, summaryText, userId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(summaryRecord);
    }
}

