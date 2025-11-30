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
    private readonly IHandoverRepository _handoverRepository = Substitute.For<IHandoverRepository>();
    private readonly IPatientRepository _patientRepository = Substitute.For<IPatientRepository>();
    private readonly UpdatePatientSummaryHandler _handler;

    public UpdatePatientSummaryHandlerHandle()
    {
        _handler = new UpdatePatientSummaryHandler(_handoverRepository, _patientRepository);
    }

    [Fact]
    public async Task ReturnsSuccessGivenValidRequest()
    {
        var patientId = "pat-1";
        var handoverId = "handover-1";
        var userId = "dr-1";

        _handoverRepository.GetOrCreateCurrentHandoverIdAsync(patientId, userId)
            .Returns(Task.FromResult<string?>(handoverId));
        _patientRepository.UpdatePatientSummaryAsync(handoverId, "New", userId)
            .Returns(Task.FromResult(true));

        var command = new UpdatePatientSummaryCommand(patientId, "New", userId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnsErrorWhenNoHandoverCanBeCreated()
    {
        var patientId = "pat-1";
        var userId = "dr-1";
        
        _handoverRepository.GetOrCreateCurrentHandoverIdAsync(patientId, userId)
            .Returns(Task.FromResult<string?>(null));

        var command = new UpdatePatientSummaryCommand(patientId, "New", userId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("assignment"));
    }
}

