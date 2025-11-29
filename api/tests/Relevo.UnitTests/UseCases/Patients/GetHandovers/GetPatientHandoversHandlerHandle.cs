using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Patients.GetHandovers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Patients.GetHandovers;

public class GetPatientHandoversHandlerHandle
{
    private readonly IHandoverRepository _repository = Substitute.For<IHandoverRepository>();
    private readonly GetPatientHandoversHandler _handler;

    public GetPatientHandoversHandlerHandle()
    {
        _handler = new GetPatientHandoversHandler(_repository);
    }

    [Fact]
    public async Task ReturnsHandovers()
    {
        var patientId = "pat-001";
        var handovers = new List<HandoverRecord>
        {
            new HandoverRecord("hvo-1", "asn-1", patientId, "Test Patient", "Draft", 
                new HandoverIllnessSeverity("Stable"), new HandoverPatientSummary("Summary"), null, null,
                "Day", "dr-1", "dr-2", null, null, null, "dr-1", "Dr. One", 
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Draft", 1)
        };
        _repository.GetPatientHandoversAsync(patientId, 1, 25)
            .Returns(Task.FromResult<(IReadOnlyList<HandoverRecord>, int)>((handovers, 1)));

        var result = await _handler.Handle(new GetPatientHandoversQuery(patientId, 1, 25), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ReturnsEmptyListWhenNoHandovers()
    {
        var patientId = "pat-001";
        _repository.GetPatientHandoversAsync(patientId, 1, 25)
            .Returns(Task.FromResult<(IReadOnlyList<HandoverRecord>, int)>((new List<HandoverRecord>(), 0)));

        var result = await _handler.Handle(new GetPatientHandoversQuery(patientId, 1, 25), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }
}

